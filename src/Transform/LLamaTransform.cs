using LLama.Abstractions;
using LLama.Common;
using System.Text;

namespace LLamaWorker.Transform
{
    /// <summary>
    /// ChatML 历史记录转换
    /// </summary>
    public class LLamaHistoryTransform : BaseHistoryTransform
    {
        /// <inheritdoc/>
        protected override string userToken => "<|start_header_id|>user<|end_header_id|>\n";

        /// <inheritdoc/>
        protected override string assistantToken => "<|start_header_id|>assistant<|end_header_id|>\n";

        /// <inheritdoc/>
        protected override string systemToken => "<|start_header_id|>system<|end_header_id|>\n";

        /// <inheritdoc/>
        protected override string endToken => "<|eot_id|>";
    }

    /// <summary>
    /// 处理结尾多余的输出
    /// </summary>
    public class LLamaTextStreamTransform
        : BaseTextStreamTransform
    {
        /// <inheritdoc/>
        protected override string startToken => "<|eot_id|>";
    }

}
