namespace LLamaWorker.Transform
{
    /// <summary>
    /// ChatML 历史记录转换
    /// </summary>
    public class ZephyrHistoryTransform : BaseHistoryTransform
    {
        /// <inheritdoc/>
        protected override string userToken => "<|user|>";

        /// <inheritdoc/>
        protected override string assistantToken => "<|assistant|>";

        /// <inheritdoc/>
        protected override string systemToken => "<|system|>";

        /// <inheritdoc/>
        protected override string endToken => "<|end|>";
    }

}
