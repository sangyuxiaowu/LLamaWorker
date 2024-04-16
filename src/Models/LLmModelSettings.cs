using LLama.Common;

namespace LLamaWorker.Models
{
    /// <summary>
    /// 模型配置信息
    /// </summary>
    public class LLmModelSettings
    {
        /// <summary>
        /// 模型名称
        /// </summary>
        public string Name { get; set; } = "default";

        /// <summary>
        /// 模型描述
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 模型版本
        /// </summary>
        public string? Version { get; set; }

        /// <summary>
        /// 对话时未指定系统提示词时使用的默认配置
        /// </summary>
        public string? SystemPrompt { get; set; }

        /// <summary>
        /// 模型加载参数
        /// </summary>
        public ModelParams ModelParams { get; set; }

        /// <summary>
        /// 模型转换参数
        /// </summary>
        public WithTransform? WithTransform { get; set; }

        /// <summary>
        /// 停止词
        /// </summary>
        public string[]? AntiPrompts { get; set; }

        /// <summary>
        /// 模型配置信息
        /// </summary>
        /// <param name="modelPath">模型路径</param>
        public LLmModelSettings(string modelPath)
        {
            ModelParams = new ModelParams(modelPath);
        }

        /// <summary>
        /// 模型配置信息
        /// </summary>
        public LLmModelSettings()
        {
            ModelParams = new ModelParams("");
        }
    }

    /// <summary>
    /// 模型转换参数
    /// </summary>
    public class WithTransform
    {
        /// <summary>
        /// 对话转换
        /// </summary>
        public string? HistoryTransform { get; set; }

        /// <summary>
        /// 输出转换
        /// </summary>
        public string? OutputTransform { get; set; }
    }
}
