using LLamaWorker.Models.OpenAI;

namespace LLamaWorker.Services
{
    /// <summary>
    /// LLM 模型服务 接口
    /// </summary>
    public interface ILLmModelService : IDisposable
    {
        /// <summary>
        /// 是否支持嵌入
        /// </summary>
        bool IsSupportEmbedding { get; }

        /// <summary>
        /// 初始化指定模型
        /// </summary>
        void InitModelIndex();

        /// <summary>
        /// 主动释放模型资源
        /// </summary>
        void DisposeModel();


        /// <summary>
        /// 获取模型信息
        /// </summary>
        /// <returns></returns>
        IReadOnlyDictionary<string, string> GetModelInfo();

        /// <summary>
        /// 聊天完成
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<ChatCompletionResponse> CreateChatCompletionAsync(ChatCompletionRequest request);

        /// <summary>
        /// 流式生成-聊天完成
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        IAsyncEnumerable<string> CreateChatCompletionStreamAsync(ChatCompletionRequest request);


        /// <summary>
        /// 创建嵌入
        /// </summary>
        /// <param name="request">请求内容</param>
        /// <returns>词嵌入</returns>
        Task<EmbeddingResponse> CreateEmbeddingAsync(EmbeddingRequest request);


        /// <summary>
        /// 提示完成
        /// </summary>
        Task<CompletionResponse> CreateCompletionAsync(CompletionRequest request);

        /// <summary>
        /// 流式生成-提示完成
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        IAsyncEnumerable<string> CreateCompletionStreamAsync(CompletionRequest request);
    }
}
