using LLamaWorker.OpenAIModels;
using Microsoft.Extensions.Options;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Text.Unicode;

namespace LLamaWorker.FunctionCall
{
    /// <summary>
    /// 基础工具提示生成器
    /// </summary>
    public class ToolPromptGenerator
    {
        private readonly List<ToolPromptConfig> _config;

        private readonly string[] _nullWords = new string[] { "null", "{}", "[]" };
        /// <summary>
        /// 基础工具提示生成器
        /// </summary>
        /// <param name="config">工具配置信息</param>
        public ToolPromptGenerator(IOptions<List<ToolPromptConfig>> config)
        {
            _config = config.Value;
        }

        /// <summary>
        /// 检查工具是否激活
        /// </summary>
        /// <param name="token"></param>
        /// <param name="tpl"></param>
        /// <returns></returns>
        public bool IsToolActive(List<string> tokens, int tpl = 0)
        {
            return string.Join("", tokens).Trim().StartsWith(_config[tpl].FN_NAME);
        }

        /// <summary>
        /// 获取工具停用词
        /// </summary>
        /// <param name="tpl">模版序号</param>
        /// <returns></returns>
        public string[] GetToolStopWords(int tpl = 0)
        {
            return _config[tpl].FN_STOP_WORDS;
        }

        /// <summary>
        /// 获取工具结果分割符
        /// </summary>
        /// <param name="tpl">模版序号</param>
        /// <returns></returns>
        public string GetToolResultSplit(int tpl = 0)
        {
            return _config[tpl].FN_RESULT_SPLIT;
        }

        /// <summary>
        /// 获取工具提示配置
        /// </summary>
        /// <param name="tpl">模版序号</param>
        /// <returns></returns>
        public ToolPromptConfig GetToolPromptConfig(int tpl = 0)
        {
            return _config[tpl];
        }

        /// <summary>
        /// 生成工具调用
        /// </summary>
        /// <param name="tool">工具调用消息</param>
        /// <param name="tpl">模版序号</param>
        /// <returns></returns>
        public string GenerateToolCall(ToolMeaasge tool, int tpl = 0)
        {
            return string.Format(_config[tpl].FN_CALL_TEMPLATE, tool.function.name, tool.function.arguments);
        }

        /// <summary>
        /// 生成工具返回结果
        /// </summary>
        /// <param name="res">工具调用结果</param>
        /// <param name="tpl">模版序号</param>
        /// <returns></returns>
        public string GenerateToolCallResult(string? res, int tpl = 0)
        {
            return string.Format(_config[tpl].FN_RESULT_TEMPLATE, res);
        }

        /// <summary>
        /// 生成工具推理结果
        /// </summary>
        /// <param name="res">工具推理结果</param>
        /// <param name="tpl">模版序号</param>
        /// <returns></returns>
        public string GenerateToolCallReturn(string? res, int tpl = 0)
        {
            return $"{_config[tpl].FN_EXIT}: {res}";
        }

        /// <summary>
        /// 检查并生成工具调用
        /// </summary>
        /// <param name="input">推理输出</param>
        /// <param name="tpl">模版序号</param>
        /// <returns></returns>
        public List<ToolMeaasgeFuntion> GenerateToolCall(string input, int tpl = 0)
        {
            string pattern = _config[tpl].FN_TEST;
            Regex regex = new Regex(pattern, RegexOptions.Singleline);
            MatchCollection matches = regex.Matches(input);
            List<ToolMeaasgeFuntion> results = new();
            foreach (Match match in matches)
            {
                string functionName = match.Groups[1].Value;
                string arguments = match.Groups[3].Success ? match.Groups[3].Value : "";
                if (string.IsNullOrWhiteSpace(arguments) || _nullWords.Contains(arguments))
                {
                    arguments = null;
                }
                results.Add(new ToolMeaasgeFuntion
                {
                    name = functionName,
                    arguments = arguments,
                });
            }
            return results;
        }


        /// <summary>
        /// 生成工具提示词
        /// </summary>
        /// <param name="req">原始对话生成请求</param>
        /// <param name="tpl">模版序号</param>
        /// <param name="lang">语言</param>
        /// <returns></returns>
        public string GenerateToolPrompt(ChatCompletionRequest req, int tpl = 0, string lang = "zh")
        {
            // 如果没有工具或者工具选择为 none，则返回空字符串
            if (req.tools == null || req.tools.Length == 0 || (req.tool_choice != null && req.tool_choice.ToString() == "none"))
            {
                return string.Empty;
            }

            var config = _config[tpl];

            var toolDescriptions = req.tools.Select(tool => GetFunctionDescription(tool.function, config.ToolDescTemplate[lang])).ToArray();
            var toolNames = string.Join(",", req.tools.Select(tool => tool.function.name));

            var toolDescTemplate = config.FN_CALL_TEMPLATE_INFO[lang];
            var toolDesc = string.Join("\n\n", toolDescriptions);
            var toolSystem = toolDescTemplate.Replace("{tool_descs}", toolDesc);

            var parallelFunctionCalls = req.tool_choice?.ToString() == "parallel";
            var toolTemplate = parallelFunctionCalls ? config.FN_CALL_TEMPLATE_FMT_PARA[lang] : config.FN_CALL_TEMPLATE_FMT[lang];
            var toolPrompt = string.Format(toolTemplate, config.FN_NAME, config.FN_ARGS, config.FN_RESULT, config.FN_EXIT, toolNames);
            return $"\n\n{toolSystem}\n\n{toolPrompt}";
        }

        private string GetFunctionDescription(FunctionInfo function, string toolDescTemplate)
        {
            // 如果没有工具描述模板，则直接返回序列化的函数信息
            if (string.IsNullOrWhiteSpace(toolDescTemplate))
            {
                return JsonSerializer.Serialize(new{
                    type = "function",
                    function = function
                }, new JsonSerializerOptions
                {
                    Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
                });
            }

            var nameForHuman = function.name;
            var nameForModel = function.name;
            var descriptionForModel = function.description ?? string.Empty;

            // 函数无参数
            if (function.parameters == null || function.parameters.properties == null || function.parameters.properties.Count == 0)
            {
                return string.Format(toolDescTemplate, nameForHuman, nameForModel, descriptionForModel, string.Empty).Trim();
            }

            //处理参数 required 字段
            var properties = function.parameters.properties;
            if (function.parameters.required?.Length > 0)
            {
                foreach (var key in function.parameters.required)
                {
                    if (properties.ContainsKey(key))
                    {
                        properties[key].required = true;
                    }
                }
            }
            var parameters = JsonSerializer.Serialize(properties, new JsonSerializerOptions { Encoder = JavaScriptEncoder.Create(UnicodeRanges.All) });
            return string.Format(toolDescTemplate, nameForHuman, nameForModel, descriptionForModel, parameters).Trim();
        }
    }
}