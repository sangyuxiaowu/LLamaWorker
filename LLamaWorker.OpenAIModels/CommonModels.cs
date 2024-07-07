namespace LLamaWorker.OpenAIModels
{
    /// <summary>
    /// 推理完成令牌信息
    /// </summary>
    public class UsageInfo
    {
        /// <summary>
        /// 提示令牌数
        /// </summary>
        public int prompt_tokens { get; set; } = 0;
        /// <summary>
        /// 完成令牌数
        /// </summary>
        public int completion_tokens { get; set; } = 0;
        /// <summary>
        /// 总令牌数
        /// </summary>
        public int total_tokens { get; set; } = 0;
    }

    /// <summary>
    /// Embedding 完成令牌信息
    /// </summary>
    public class EmbeddingUsageInfo
    {
        /// <summary>
        /// 提示令牌数
        /// </summary>
        public int prompt_tokens { get; set; } = 0;
        /// <summary>
        /// 总令牌数
        /// </summary>
        public int total_tokens { get; set; } = 0;
    }
}
