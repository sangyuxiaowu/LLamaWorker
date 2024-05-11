using LLamaWorker.Models.OpenAI;

namespace LLamaWorker.Models.OpenAI
{
    /// <summary>
    /// 对话完成请求
    /// https://platform.openai.com/docs/api-reference/chat/create
    /// </summary>
    public class ChatCompletionRequest:BaseCompletionRequest
    {

        /// <summary>
        /// 对话历史
        /// </summary>
        public ChatCompletionMessage[] messages { get; set; } = Array.Empty<ChatCompletionMessage>();
    }

    /// <summary>
    /// 对话消息列表
    /// </summary>
    public class ChatCompletionMessage
    {
        /// <summary>
        /// 角色
        /// </summary>
        /// <example>user</example>
        public string? role { get; set; } = string.Empty;
        /// <summary>
        /// 对话内容
        /// </summary>
        /// <example>你好</example>
        public string content { get; set; } = string.Empty;
    }

    /// <summary>
    /// 聊天完成响应
    /// </summary>
    public class ChatCompletionResponse: BaseCompletionResponse
    {

        /// <summary>
        /// 对象类型，始终为chat.completion
        /// </summary>
        public string @object = "chat.completion";

        /// <summary>
        /// 聊天完成选择的列表。如果n大于1，则可以有多个
        /// </summary>
        public ChatCompletionResponseChoice[] choices { get; set; } = Array.Empty<ChatCompletionResponseChoice>();

    }

    /// <summary>
    /// 完成的一种选择
    /// </summary>
    public class ChatCompletionResponseChoice: BaseCompletionResponseChoice
    {
        /// <summary>
        /// 模型生成的聊天完成消息
        /// </summary>
        public ChatCompletionMessage message { get; set; } = new ();

    }

    /// <summary>
    /// 流式响应的聊天完成响应
    /// https://platform.openai.com/docs/api-reference/chat/streaming
    /// </summary>
    public class ChatCompletionChunkResponse : BaseCompletionResponse
    {

        /// <summary>
        /// 对象类型，始终为chat.completion.chunk
        /// </summary>
        public string @object = "chat.completion.chunk";

        /// <summary>
        /// 聊天完成选择的列表。如果n大于1，则可以有多个
        /// </summary>
        public ChatCompletionChunkResponseChoice[] choices { get; set; } = Array.Empty<ChatCompletionChunkResponseChoice>();
    }

    /// <summary>
    /// 流式响应完成的详情
    /// </summary>
    public class ChatCompletionChunkResponseChoice: BaseCompletionResponseChoice
    {
        /// <summary>
        /// 由流式模型响应生成的聊天完成增量。
        /// </summary>
        public ChatCompletionMessage? delta { get; set; } = new();
    }
}