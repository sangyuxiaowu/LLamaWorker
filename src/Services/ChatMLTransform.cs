using LLama.Abstractions;
using LLama.Common;
using System.Text;

namespace LLamaWorker.Services
{
    public class ChatMLHistoryTransform : IHistoryTransform
    {
        private const string userToken = "<|im_start|>user";
        private const string assistantToken = "<|im_start|>assistant";
        private const string systemToken = "<|im_start|>system";
        private const string endToken = "<|im_end|>";

        IHistoryTransform IHistoryTransform.Clone()
        {
            return new ChatMLHistoryTransform();
        }

        public virtual string HistoryToText(ChatHistory history)
        {
            StringBuilder sb = new();
            foreach (var message in history.Messages)
            {
                if (message.AuthorRole == AuthorRole.User)
                {
                    sb.AppendLine($"{userToken}\n{message.Content}{endToken}");
                }
                else if (message.AuthorRole == AuthorRole.System)
                {
                    sb.AppendLine($"{systemToken}\n{message.Content}{endToken}");
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

        public virtual ChatHistory TextToHistory(AuthorRole role, string text)
        {
            ChatHistory history = new ChatHistory();
            history.AddMessage(role, TrimNamesFromText(text, role));
            return history;
        }

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

    public class ChatMLTextStreamTransform
        : ITextStreamTransform
    {
        private const string startToken = "<|im_start|>";
        // 奇奇怪怪，总是在结尾有一个或多个问号
        private const string keyToken = "???<|im_start|>";
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
                else if (!keyToken.Contains(tmp.ToString()))
                {
                    //var result = tmp.ToString();
                    //File.AppendAllText("chatml.txt", result + "\n");
                    yield return tmp.ToString();
                    tmp.Clear();
                }
            }
        }

        public ITextStreamTransform Clone()
        {
            return new ChatMLTextStreamTransform();
        }
    }

}
