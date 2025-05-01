namespace LLamaWorker.Transform
{
    /// <summary>
    /// DeepSeek 历史记录转换
    /// </summary>
    public class DeepSeekTransform : BaseHistoryTransform
    {
        /// <inheritdoc/>
        protected override string userToken => "<｜User｜>";

        /// <inheritdoc/>
        protected override string assistantToken => "<｜Assistant｜>";

        /// <inheritdoc/>
        protected override string systemToken => " ";

        /// <inheritdoc/>
        protected override string endToken => "";

        /// <inheritdoc/>
        protected override string endSentence => "<｜end▁of▁sentence｜>";

        /// <inheritdoc/>
        protected override string thinkToken => "</think>";

        /// <inheritdoc/>
        protected override string stopThinking => "\n<think>\n\n</think>\n\n";

        /// <inheritdoc/>
        protected override bool promptTrim => true;
    }

}
