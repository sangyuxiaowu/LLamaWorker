using LLama.Abstractions;
using LLama.Common;
using System.Text;

namespace LLamaWorker.Transform
{
    /// <summary>
    /// ChatML 历史记录转换
    /// </summary>
    public class BaseHistoryTransform : IHistoryTransform
    {
        /// <summary>
        /// 用户标记
        /// </summary>
        protected virtual string userToken => "<|im_start|>user";

        /// <summary>
        /// 助理标记
        /// </summary>
        protected virtual string assistantToken => "<|im_start|>assistant";

        /// <summary>
        /// 系统标记
        /// </summary>
        protected virtual string systemToken => "<|im_start|>system";

        /// <summary>
        /// 结束标记
        /// </summary>
        protected virtual string endToken => "<|im_end|>";
        

        IHistoryTransform IHistoryTransform.Clone()
        {
            return new BaseHistoryTransform();
        }

        /// <summary>
        /// 历史记录转换为文本
        /// </summary>
        /// <param name="history"></param>
        /// <returns></returns>
        public virtual string HistoryToText(ChatHistory history)
        {

            // 若有系统消息，则会放在最开始
            // 用于处理模型不支持系统消息角色设定的情况
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
                    // 模型不支持系统消息角色设定
                    if (string.IsNullOrWhiteSpace(systemToken))
                    {
                        systemMessage = $"{message.Content} ";
                    }
                    else
                    {
                        sb.AppendLine($"{systemToken}\n{message.Content}{endToken}");
                    }
                }
                else if (message.AuthorRole == AuthorRole.Assistant)
                {
                    sb.AppendLine($"{assistantToken}\n{message.Content}{endToken}");
                }
            }
            sb.AppendLine(assistantToken);
            //Console.WriteLine(sb.ToString());
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
    /// </summary>
    public class BaseTextStreamTransform
        : ITextStreamTransform
    {

        /// <summary>
        /// 开始标记
        /// </summary>
        protected virtual string startToken => "<|im_start|>";

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
                    yield return tmp.ToString().Replace(startToken, "");
                    tmp.Clear();
                }
                else if (!startToken.Contains(tmp.ToString()))
                {
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
            return new BaseTextStreamTransform();
        }
    }

}
