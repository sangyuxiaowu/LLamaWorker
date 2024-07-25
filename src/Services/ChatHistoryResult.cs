
namespace LLamaWorker.Services
{
    /// <summary>
    /// 消息历史记录结果
    /// </summary>
    public class ChatHistoryResult
    {
        /// <summary>
        /// 历史记录
        /// </summary>
        public string ChatHistory { get; set; }

        /// <summary>
        /// 是否启用了工具提示
        /// </summary>
        public bool IsToolPromptEnabled { get; set; }

        /// <summary>
        /// 工具结束标记
        /// </summary>
        public string[]? ToolStopWords { get; set; }

        /// <summary>
        /// 消息历史记录结果
        /// </summary>
        /// <param name="chatHistory">历史记录</param>
        /// <param name="isToolPromptEnabled">是否启用了工具提示</param>
        /// <param name="toolStopWords">工具结束标记</param>
        public ChatHistoryResult(string chatHistory, bool isToolPromptEnabled, string[]? toolStopWords)
        {
            ChatHistory = chatHistory;
            IsToolPromptEnabled = isToolPromptEnabled;
            ToolStopWords = toolStopWords;
        }
    }
}