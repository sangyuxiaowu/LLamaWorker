using LLama.Abstractions;
using LLama.Common;
using LLamaWorker.OpenAIModels;
using System.Text;

namespace LLamaWorker.Transform
{
    /// <summary>
    /// ChatML 历史记录转换
    /// </summary>
    public class BaseHistoryTransform: ITemplateTransform
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

        /// <summary>
        /// 历史记录转换为文本
        /// </summary>
        /// <param name="history"></param>
        /// <returns></returns>
        public virtual string HistoryToText(ChatCompletionMessage[] history)
        {

            // 若有系统消息，则会放在最开始
            // 用于处理模型不支持系统消息角色设定的情况
            var systemMessage = "";

            StringBuilder sb = new();
            foreach (var message in history)
            {
                if (message.role == "user")
                {
                    sb.AppendLine($"{userToken}\n{systemMessage}{message.content}{endToken}");
                    systemMessage = "";
                }
                else if (message.role == "system")
                {
                    // 模型不支持系统消息角色设定
                    if (string.IsNullOrWhiteSpace(systemToken))
                    {
                        systemMessage = $"{message.content} ";
                    }
                    else
                    {
                        sb.AppendLine($"{systemToken}\n{message.content}{endToken}");
                    }
                }
                else if (message.role == "assistant")
                {
                    sb.AppendLine($"{assistantToken}\n{message.content}{endToken}");
                }
            }
            sb.AppendLine(assistantToken);
            //Console.WriteLine(sb.ToString());
            return sb.ToString();
        }

    }
}
