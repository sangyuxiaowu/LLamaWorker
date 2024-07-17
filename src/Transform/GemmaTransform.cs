namespace LLamaWorker.Transform
{
    /// <summary>
    /// ChatML 历史记录转换
    /// </summary>
    public class GemmaHistoryTransform : BaseHistoryTransform
    {
        /// <inheritdoc/>
        protected override string userToken => "<start_of_turn>user";

        /// <inheritdoc/>
        protected override string assistantToken => "<start_of_turn>model";

        /// <inheritdoc/>
        protected override string systemToken => "";

        /// <inheritdoc/>
        protected override string endToken => "<end_of_turn>";
    }

}
