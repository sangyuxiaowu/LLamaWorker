namespace LLamaWorker.OpenAIModels
{
    /// <summary>
    /// 基础完成请求
    /// </summary>
    public class BaseCompletionRequest
    {
        /// <summary>
        /// 模型名称
        /// </summary>
        /// <example>gpt</example>
        public string model { get; set; } = string.Empty;

        /// <summary>
        /// 为每条输入消息生成多少个聊天完成选项。
        /// </summary>
        public int? n { get; set; } = 1;

        /// <summary>
        /// 温度：控制随机性。降低温度意味着模型将产生更多重复和确定性的响应。
        /// 增加温度会导致更多意外或创造性的响应。
        /// 尝试调整温度或 Top P，但不要同时调整两者。
        /// </summary>
        /// <example>1.0</example>
        public float temperature { get; set; } = 1.0f;

        /// <summary>
        /// 顶部 P：与温度类似，这控制随机性但使用不同的方法。
        /// 降低 Top P 将缩小模型的令牌选择范围，使其更有可能选择令牌。
        /// 增加 Top P 将让模型从高概率和低概率的令牌中进行选择。
        /// 尝试调整温度或 Top P，但不要同时调整两者。
        /// </summary>
        /// <example>1.0</example>
        public float top_p { get; set; } = 1.0f;

        /// <summary>
        /// 流式响应，默认为 false。如果设置为 true，则服务端将在响应使用 HTTP Stream Response。
        /// </summary>
        /// <example>false</example>
        public bool stream { get; set; } = false;

        /// <summary>
        /// 停止序列：使模型在所需点结束响应。
        /// 模型响应将在指定序列之前结束，因此不包含停止序列文本。
        /// 对于 ChatGPT，使用<![CDATA[ <|im_end|> ]]>可确保模型响应不会生成后续用户查询。
        /// 可以包含多达四个停止序列。
        /// 可以为 string / array / null，中间件将处理为数组。
        /// </summary>
        /// <example>null</example>
        public string[]? stop { get; set; } = null;

        /// <summary>
        /// 最大生成长度：设置每个模型响应的令牌数量限制。
        /// API 支持最多 MaxTokensPlaceholderDoNotTranslate 个令牌，
        /// 这些令牌在提示（包括系统消息、示例、消息历史记录和用户查询）和模型响应之间共享。
        /// 一个令牌大约是典型英文文本的 4 个字符。
        /// </summary>
        /// <example>512</example>
        public int? max_completion_tokens { get; set; } = null;

        /// <summary>
        /// 频率损失：根据令牌到目前为止在文本中出现的频率，按比例减少重复令牌的几率。
        /// 这降低了在响应中重复完全相同的文本的可能性。
        /// </summary>
        public float presence_penalty { get; set; } = 0.0f;

        /// <summary>
        /// 状态惩罚：减少到目前为止文本中出现的任何标记重复的可能性。
        /// 这增加了在响应中引入新主题的可能性。
        /// </summary>
        public float frequency_penalty { get; set; } = 0.0f;

        /// <summary>
        /// 随机种子：用于控制模型生成的随机性。
        /// 如果指定，系统将尽最大努力进行确定性采样，这样具有相同种子和参数的重复请求应该返回相同的结果。
        /// </summary>
        /// <example>null</example>
        public uint? seed { get; set; }

        /// <summary>
        /// 用户：最终用户的唯一标识符
        /// </summary>
        public string? user { get; set; }
    }


    /// <summary>
    /// 基础完成响应
    /// </summary>
    public class BaseCompletionResponse
    {
        /// <summary>
        /// 聊天完成的唯一标识符
        /// </summary>
        public string id { get; set; } = string.Empty;

        /// <summary>
        /// 创建聊天完成的Unix时间戳（以秒为单位）
        /// </summary>
        public long created { get; set; }

        /// <summary>
        /// 用于聊天完成的模型
        /// </summary>
        public string model { get; set; } = string.Empty;

        /// <summary>
        /// 完成请求的使用统计信息
        /// </summary>
        public UsageInfo usage { get; set; } = new();
    }


    /// <summary>
    /// 基础完成响应内容选择
    /// </summary>
    public class BaseCompletionResponseChoice
    {
        /// <summary>
        /// 选择在选择列表中的索引
        /// </summary>
        public int index { get; set; }

        /// <summary>
        /// 模型停止生成令牌的原因。这将是stop，
        /// 如果模型触及了自然停止点或提供的停止序列，length，
        /// 如果在请求中指定的最大令牌数量已达到，content_filter，
        /// </summary>
        public string? finish_reason { get; set; }
    }
}