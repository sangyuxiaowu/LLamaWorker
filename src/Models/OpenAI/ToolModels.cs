namespace LLamaWorker.Models.OpenAI
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
        public string? description { get; set; }

        /// <summary>
        /// 函数接受的参数，描述为JSON模式对象。
        /// </summary>
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
        public string? description { get; set; }

        /// <summary>
        /// 参数的可选值
        /// </summary>
        public string[]? @enum { get; set; }
    }

}
