using LLamaWorker.OpenAIModels;
using Microsoft.Extensions.Options;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

namespace LLamaWorker.FunctionCall
{
    /// <summary>
    /// 基础工具提示生成器
    /// </summary>
    public class ToolPromptGenerator
    {
        private readonly List<ToolPromptConfig> _config;


        /// <summary>
        /// 基础工具提示生成器
        /// </summary>
        /// <param name="config">工具配置信息</param>
        public ToolPromptGenerator(IOptions<List<ToolPromptConfig>> config)
        {
            _config = config.Value;
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
            var nameForHuman = function.name;
            var nameForModel = function.name;
            var descriptionForModel = function.description ?? string.Empty;
            var parameters = JsonSerializer.Serialize(function.parameters, new JsonSerializerOptions { WriteIndented = true, Encoder = JavaScriptEncoder.Create(UnicodeRanges.All) });

            return string.Format(toolDescTemplate, nameForHuman, nameForModel, descriptionForModel, parameters).Trim();
        }
    }
}