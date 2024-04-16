using LLama.Abstractions;
using LLama.Common;
using System.Text;

namespace LLamaWorker.Transform
{
    /// <summary>
    /// ChatML 历史记录转换
    /// </summary>
    public class GemmaHistoryTransform : IHistoryTransform
    {
        private const string userToken = "<start_of_turn>user";
        private const string assistantToken = "<start_of_turn>model";
        private const string endToken = "<end_of_turn>";

        IHistoryTransform IHistoryTransform.Clone()
        {
            return new ChatMLHistoryTransform();
        }

        /// <summary>
        /// 历史记录转换为文本
        /// </summary>
        /// <param name="history"></param>
        /// <returns></returns>
        public virtual string HistoryToText(ChatHistory history)
        {

            // 若有系统消息，则会放在最开始
            var systemMessage = "";

            StringBuilder sb = new();
            foreach (var message in history.Messages)
            {
                if (message.AuthorRole == AuthorRole.User)
                {
                    sb.AppendLine($"{userToken}\n{systemMessage}{message.Content}{endToken}");
                    systemMessage = "";
                }
                else if (message.AuthorRole == AuthorRole.System)
                {
                    systemMessage = $"{message.Content} ";
                }
                else if (message.AuthorRole == AuthorRole.Assistant)
                {
                    sb.AppendLine($"{assistantToken}\n{message.Content}{endToken}");
                }
            }
            sb.AppendLine(assistantToken);
            Console.WriteLine(sb.ToString());
            return sb.ToString();
        }

        /// <summary>
        /// 文本转换为历史记录
        /// </summary>
        /// <param name="role"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        public virtual ChatHistory TextToHistory(AuthorRole role, string text)
        {
            ChatHistory history = new ChatHistory();
            history.AddMessage(role, TrimNamesFromText(text, role));
            return history;
        }


        /// <summary>
        /// 去除文本中的角色标记
        /// </summary>
        /// <param name="text"></param>
        /// <param name="role"></param>
        /// <returns></returns>
        public virtual string TrimNamesFromText(string text, AuthorRole role)
        {
            if (role == AuthorRole.User && text.StartsWith(userToken))
            {
                text = text.Substring(userToken.Length).TrimStart();
            }
            else if (role == AuthorRole.Assistant && text.EndsWith(assistantToken))
            {
                text = text.Substring(0, text.Length - assistantToken.Length).TrimEnd();
            }
            return text;
        }
    }

    /// <summary>
    /// 处理结尾多余的输出
    /// TODO: 优化
    /// </summary>
    public class GemmaTextStreamTransform
        : ITextStreamTransform
    {
        private const string startToken = "<start_of_turn>";

        /// <summary>
        /// 转换文本流
        /// </summary>
        /// <param name="tokens"></param>
        /// <returns></returns>
        public async IAsyncEnumerable<string> TransformAsync(IAsyncEnumerable<string> tokens)
        {
            var tmp = new StringBuilder();

            await foreach (var token in tokens)
            {
                tmp.Append(token);

                if (tmp.ToString().Contains(startToken))
                {
                    var result = tmp.ToString();
                    var index = result.IndexOf(startToken);
                    var startIndex = index;
                    for (int i = 0; i < 2 && startIndex > 0 && result[startIndex - 1] == '?'; i++)
                    {
                        startIndex--;
                    }
                    result = result.Substring(0, startIndex);
                    //File.AppendAllText("chatml.txt", result + "\n");
                    yield return result;
                    tmp.Clear();
                }
                else if (!startToken.Contains(tmp.ToString()))
                {
                    //var result = tmp.ToString();
                    //File.AppendAllText("chatml.txt", result + "\n");
                    yield return tmp.ToString();
                    tmp.Clear();
                }
            }
        }

        /// <summary>
        /// 克隆文本流转换器
        /// </summary>
        /// <returns></returns>
        public ITextStreamTransform Clone()
        {
            return new ChatMLTextStreamTransform();
        }
    }

}
