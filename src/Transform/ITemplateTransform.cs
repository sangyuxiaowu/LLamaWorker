using LLamaWorker.Config;
using LLamaWorker.FunctionCall;
using LLamaWorker.OpenAIModels;

namespace LLamaWorker.Transform
{
    public interface ITemplateTransform
    {
        public string HistoryToText(ChatCompletionMessage[] history, ToolPromptGenerator generator, ToolPromptInfo toolinfo, string toolPrompt, bool thinking);
    }
}