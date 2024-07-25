using LLamaWorker.FunctionCall;
using LLamaWorker.OpenAIModels;

namespace LLamaWorker.Transform
{
    public interface ITemplateTransform
    {
        public string HistoryToText(ChatCompletionMessage[] history, ToolPromptGenerator generator, string toolPrompt);
    }
}