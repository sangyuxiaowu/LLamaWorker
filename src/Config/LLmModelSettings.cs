﻿using LLama.Common;

namespace LLamaWorker.Config
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
        /// 模型站点介绍
        /// </summary>
        public string? WebSite { get; set; }

        /// <summary>
        /// 模型版本
        /// </summary>
        public string? Version { get; set; }


        /// <summary>
        /// 嵌入支持
        /// -1: 不支持
        /// 0 : 支持
        /// 1 : 仅支持嵌入
        /// </summary>
        public int EmbeddingSupport { get; set; } = -1;

        /// <summary>
        /// 模型是否存在
        /// </summary>
        public bool IsExist
        {
            get
            {
                return !string.IsNullOrWhiteSpace(ModelParams.ModelPath) && File.Exists(ModelParams.ModelPath);
            }
        }

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
        /// 工具提示配置
        /// </summary>
        public ToolPromptInfo ToolPrompt { get; set; }

        /// <summary>
        /// 模型配置信息
        /// </summary>
        /// <param name="modelPath">模型路径</param>
        public LLmModelSettings(string modelPath)
        {
            ModelParams = new ModelParams(modelPath);
            ToolPrompt = new ToolPromptInfo();
        }

        /// <summary>
        /// 模型配置信息
        /// </summary>
        public LLmModelSettings()
        {
            ModelParams = new ModelParams("");
            ToolPrompt = new ToolPromptInfo();
        }
    }

    /// <summary>
    /// 工具提示配置
    /// </summary>
    public class ToolPromptInfo
    {
        /// <summary>
        /// 提示模板索引
        /// </summary>
        public int Index { get; set; } = 0;

        /// <summary>
        /// 模板提示语言
        /// </summary>
        public string Lang { get; set; } = "en";
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

    /// <summary>
    /// 模型配置信息
    /// </summary>
    public class ConfigModels
    {
        /// <summary>
        /// 当前使用的模型
        /// </summary>
        public int Current { get; set; }

        /// <summary>
        /// 是否已加载
        /// </summary>
        public bool Loaded { get; set; }

        /// <summary>
        /// 模型列表
        /// </summary>
        public List<LLmModelSettings>? Models { get; set; }
    }
}
