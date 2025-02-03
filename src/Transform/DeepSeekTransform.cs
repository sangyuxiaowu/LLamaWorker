namespace LLamaWorker.Transform
{
    /// <summary>
    /// ChatML 历史记录转换
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
    }

}
