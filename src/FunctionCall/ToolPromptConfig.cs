
/// <summary>
/// 工具提示配置
/// </summary>
public class ToolPromptConfig
{
    /// <summary>
    /// 工具提示配置的介绍信息
    /// </summary>
    public string PromptConfigDesc { get; set; }

    /// <summary>
    /// 工具名称占位符
    /// </summary>
    public string FN_NAME { get; set; }

    /// <summary>
    /// 工具参数占位符
    /// </summary>
    public string FN_ARGS { get; set; }

    /// <summary>
    /// 工具结果占位符
    /// </summary>
    public string FN_RESULT { get; set; }

    /// <summary>
    /// 函数调用模板
    /// </summary>
    public string FN_CALL_TEMPLATE { get; set; }

    /// <summary>
    /// 函数调用开始标记
    /// </summary>
    public string FN_CALL_START { get; set; } = "";

    /// <summary>
    /// 函数调用结束标记
    /// </summary>
    public string FN_CALL_END { get; set; } = "";

    /// <summary>
    /// 函数调用与结果分隔符
    /// </summary>
    public string FN_RESULT_SPLIT { get; set; }

    /// <summary>
    /// 工具结果模板
    /// </summary>
    public string FN_RESULT_TEMPLATE { get; set; }

    /// <summary>
    /// 工具结果开始标记
    /// </summary>
    public string FN_RESULT_START { get; set; } = "";

    /// <summary>
    /// 工具结果结束标记
    /// </summary>
    public string FN_RESULT_END { get; set; } = "";

    /// <summary>
    /// 函数调提取的正则表达式，用于提取函数名和参数
    /// </summary>
    public string FN_TEST { get; set; }

    /// <summary>
    /// 工具返回占位符
    /// </summary>
    public string FN_EXIT { get; set; }

    /// <summary>
    /// 工具停止词列表
    /// </summary>
    public string[] FN_STOP_WORDS { get; set; }

    /// <summary>
    /// 工具描述模板信息，按语言区分
    /// </summary>
    public Dictionary<string, string> FN_CALL_TEMPLATE_INFO { get; set; }

    /// <summary>
    /// 工具调用模板，按语言区分
    /// </summary>
    public Dictionary<string, string> FN_CALL_TEMPLATE_FMT { get; set; }

    /// <summary>
    /// 并行工具调用模板，按语言区分
    /// </summary>
    public Dictionary<string, string> FN_CALL_TEMPLATE_FMT_PARA { get; set; }

    // TODO: 指定工具描述模板，按语言区分
    // TODO: 指定工具描述模板，按语言区分

    /// <summary>
    /// 工具描述模板，按语言区分
    /// </summary>
    public Dictionary<string, string> ToolDescTemplate { get; set; }
}