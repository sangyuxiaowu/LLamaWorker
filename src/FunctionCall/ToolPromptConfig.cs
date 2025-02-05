
/// <summary>
/// ������ʾ����
/// </summary>
public class ToolPromptConfig
{
    /// <summary>
    /// ������ʾ���õĽ�����Ϣ
    /// </summary>
    public string PromptConfigDesc { get; set; }

    /// <summary>
    /// ��������ռλ��
    /// </summary>
    public string FN_NAME { get; set; }

    /// <summary>
    /// ���߲���ռλ��
    /// </summary>
    public string FN_ARGS { get; set; }

    /// <summary>
    /// ���߽��ռλ��
    /// </summary>
    public string FN_RESULT { get; set; }

    /// <summary>
    /// ��������ģ��
    /// </summary>
    public string FN_CALL_TEMPLATE { get; set; }

    /// <summary>
    /// �������ÿ�ʼ���
    /// </summary>
    public string FN_CALL_START { get; set; } = "";

    /// <summary>
    /// �������ý������
    /// </summary>
    public string FN_CALL_END { get; set; } = "";

    /// <summary>
    /// �������������ָ���
    /// </summary>
    public string FN_RESULT_SPLIT { get; set; }

    /// <summary>
    /// ���߽��ģ��
    /// </summary>
    public string FN_RESULT_TEMPLATE { get; set; }

    /// <summary>
    /// ���߽����ʼ���
    /// </summary>
    public string FN_RESULT_START { get; set; } = "";

    /// <summary>
    /// ���߽���������
    /// </summary>
    public string FN_RESULT_END { get; set; } = "";

    /// <summary>
    /// ��������ȡ��������ʽ��������ȡ�������Ͳ���
    /// </summary>
    public string FN_TEST { get; set; }

    /// <summary>
    /// ���߷���ռλ��
    /// </summary>
    public string FN_EXIT { get; set; }

    /// <summary>
    /// ����ֹͣ���б�
    /// </summary>
    public string[] FN_STOP_WORDS { get; set; }

    /// <summary>
    /// ��������ģ����Ϣ������������
    /// </summary>
    public Dictionary<string, string> FN_CALL_TEMPLATE_INFO { get; set; }

    /// <summary>
    /// ���ߵ���ģ�壬����������
    /// </summary>
    public Dictionary<string, string> FN_CALL_TEMPLATE_FMT { get; set; }

    /// <summary>
    /// ���й��ߵ���ģ�壬����������
    /// </summary>
    public Dictionary<string, string> FN_CALL_TEMPLATE_FMT_PARA { get; set; }

    // TODO: ָ����������ģ�壬����������
    // TODO: ָ����������ģ�壬����������

    /// <summary>
    /// ��������ģ�壬����������
    /// </summary>
    public Dictionary<string, string> ToolDescTemplate { get; set; }
}