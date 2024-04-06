using LLama.Common;

namespace LLamaWorker.Models
{
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

        public LLmModelSettings(string modelPath)
        {
            ModelParams = new ModelParams(modelPath);
        }

        public LLmModelSettings()
        {
            ModelParams = new ModelParams("");
        }
    }
}
