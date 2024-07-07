namespace LLamaWorker.OpenAIModels
{
    /// <summary>
    /// 提示完成请求
    /// https://platform.openai.com/docs/api-reference/completions
    /// </summary>
    public class CompletionRequest:BaseCompletionRequest
    {
        /// <summary>
        /// 提示词
        /// 要为字符串、字符串数组、令牌数组或令牌数组数组生成补全的提示。
        /// </summary>
        /// <example>今天天气不错，</example>
        public string prompt { get; set; } = string.Empty;
    }


    /// <summary>
    /// 聊天完成响应
    /// </summary>
    public class CompletionResponse: BaseCompletionResponse
    {

        /// <summary>
        /// 对象类型，始终为text_completion
        /// </summary>
        public string @object = "text_completion";

        /// <summary>
        /// 聊天完成选择的列表。如果n大于1，则可以有多个
        /// </summary>
        public CompletionResponseChoice[] choices { get; set; } = Array.Empty<CompletionResponseChoice>();

    }

    /// <summary>
    /// 完成的一种选择
    /// </summary>
    public class CompletionResponseChoice: BaseCompletionResponseChoice
    {
        /// <summary>
        /// 模型生成的完成消息
        /// </summary>
        public string text { get; set; } = string.Empty;

    }
}