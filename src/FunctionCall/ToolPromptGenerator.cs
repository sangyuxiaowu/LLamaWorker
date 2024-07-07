using LLamaWorker.Models.OpenAI;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace LLamaWorker.FunctionCall
{
    /// <summary>
    /// 基础工具提示生成器
    /// </summary>
    public class ToolPromptGenerator
    {
        private readonly ToolPromptConfig _config;

        public ToolPromptGenerator(IOptions<ToolPromptConfig> config)
        {
            _config = config.Value;
        }

        public string GenerateToolPrompt(ChatCompletionRequest req, string lang)
        {
            if (req.tools == null || req.tools.Length == 0)
            {
                return string.Empty;
            }

            var toolDescriptions = req.tools.Select(tool => GetFunctionDescription(tool.function, lang)).ToArray();
            var toolNames = string.Join(",", req.tools.Select(tool => tool.function.name));

            var toolDescTemplate = _config.FN_CALL_TEMPLATE_INFO[lang];
            var toolDesc = string.Join("\n\n", toolDescriptions);
            var toolSystem = toolDescTemplate.Replace("{tool_descs}", toolDesc);

            var parallelFunctionCalls = req.tool_choice?.ToString() == "parallel";
            var toolTemplate = parallelFunctionCalls ? _config.FN_CALL_TEMPLATE_FMT_PARA[lang] : _config.FN_CALL_TEMPLATE_FMT[lang];
            var toolPrompt = string.Format(toolTemplate, _config.FN_NAME, _config.FN_ARGS, _config.FN_RESULT, _config.FN_EXIT)
                .Replace("{tool_names}", toolNames);

            return $"{toolSystem}\n\n{toolPrompt}";
        }

        private string GetFunctionDescription(FunctionInfo function, string lang)
        {
            var toolDescTemplate = _config.ToolDescTemplate[lang];

            var nameForHuman = function.name;
            var nameForModel = function.name;
            var descriptionForModel = function.description ?? string.Empty;
            var parameters = JsonSerializer.Serialize(function.parameters, new JsonSerializerOptions { WriteIndented = true });

            return string.Format(toolDescTemplate, nameForHuman, nameForModel, descriptionForModel, parameters).Trim();
        }
    }
}