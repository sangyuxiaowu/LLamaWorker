using System.Text.Json.Serialization;

namespace LLamaWorker.OpenAIModels
{
    /// <summary>
    /// 推理完成令牌信息
    /// </summary>
    public class ToolInfo
    {
        /// <summary>
        /// 工具类型，当前只支持 function
        /// </summary>
        public string type { get; set; } = "function";

        /// <summary>
        /// 函数信息
        /// </summary>
        public required FunctionInfo function { get; set; }
    }

    /// <summary>
    /// 函数信息
    /// </summary>
    public class FunctionInfo
    {
        /// <summary>
        /// 要调用的函数的名称。必须是a-z、a-z、0-9，或包含下划线和短划线，最大长度为64。
        /// </summary>
        public required string name { get; set; }

        /// <summary>
        /// 函数作用的描述，由模型用来选择何时以及如何调用函数。
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? description { get; set; }

        /// <summary>
        /// 函数接受的参数，描述为JSON模式对象。
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Parameters? parameters { get; set; }
    }

    /// <summary>
    /// 函数参数
    /// </summary>
    public class Parameters
    {
        /// <summary>
        /// 参数类型，当前只支持 object
        /// </summary>
        public string type { get; set; } = "object";

        /// <summary>
        /// 参数的属性，描述为JSON模式对象。
        /// 所含键值的类型为 ParameterInfo
        /// </summary>
        public required Dictionary<string, ParameterInfo> properties { get; set; }

        /// <summary>
        /// 必需的参数名称列表。
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string[]? required { get; set; }
    }

    /// <summary>
    /// 参数信息，用于描述函数的参数
    /// </summary>
    public class ParameterInfo
    {
        /// <summary>
        /// 参数类型
        /// </summary>
        public required string type { get; set; }

        /// <summary>
        /// 参数的描述
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? description { get; set; }

        /// <summary>
        /// 参数的可选值
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string[]? @enum { get; set; }

        /// <summary>
        /// 参数是否必需
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? required { get; set; }
    }

    /// <summary>
    /// 工具消息块，流式处理
    /// </summary>
    public class ToolMeaasgeChunk : ToolMeaasge
    {
        /// <summary>
        /// 工具调用的索引
        /// </summary>
        public int index { get; set; }
    }

    /// <summary>
    /// 工具消息块
    /// </summary>
    public class ToolMeaasge
    {
        /// <summary>
        /// 工具调用的 ID
        /// </summary>
        public string id { get; set; }

        /// <summary>
        /// 工具类型，当前固定 function
        /// </summary>
        public string type { get; set; } = "function";

        /// <summary>
        /// 调用的函数信息
        /// </summary>
        public ToolMeaasgeFuntion function { get; set; }
    }

    /// <summary>
    /// 调用工具的响应选择
    /// </summary>
    public class ToolMeaasgeFuntion
    {
        /// <summary>
        /// 函数名称
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// 调用函数的参数，由JSON格式的模型生成。
        /// 请注意，该模型并不总是生成有效的JSON，并且可能会产生函数模式未定义的参数的幻觉。
        /// 在调用函数之前，验证代码中的参数。
        /// </summary>
        public string? arguments { get; set; }
    }
}
