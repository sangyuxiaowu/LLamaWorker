﻿using LLamaWorker.Config;
using LLamaWorker.FunctionCall;
using LLamaWorker.OpenAIModels;
using System.Text;

namespace LLamaWorker.Transform
{
    /// <summary>
    /// ChatML 历史记录转换
    /// </summary>
    public class BaseHistoryTransform : ITemplateTransform
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
        /// 对话轮次结束标记
        /// </summary>
        protected virtual string endSentence => "";

        /// <summary>
        /// 思考标记
        /// </summary>
        protected virtual string thinkToken => "</think>";

        /// <summary>
        /// 跳过思考标记
        /// </summary>
        protected virtual string stopThinking => "\n<think>\n\n</think>\n\n";

        /// <summary>
        /// 系统标记
        /// </summary>
        protected virtual string systemToken => "<|im_start|>system";

        /// <summary>
        /// 结束标记
        /// </summary>
        protected virtual string endToken => "<|im_end|>";

        /// <summary>
        /// 推理提示词是否 Trim 处理
        /// 注意：DeekSeek 模型若在尾部使用换行符会导致推理返还异常
        /// </summary>
        protected virtual bool promptTrim => false;

        /// <summary>
        /// 记录 function 的调用信息
        /// </summary>
        private Dictionary<string, string> functionCalls = new Dictionary<string, string>();

        /// <summary>
        /// 历史记录转换为文本
        /// </summary>
        /// <param name="history"></param>
        /// <param name="generator"></param>
        /// <param name="toolinfo"></param>
        /// <param name="toolPrompt"></param>
        /// <param name="thinking"></param>
        /// <returns></returns>
        public virtual string HistoryToText(ChatCompletionMessage[] history, ToolPromptGenerator generator, ToolPromptInfo toolinfo, string toolPrompt = "", bool thinking = true)
        {

            // 若有系统消息，则会放在最开始
            // 用于处理模型不支持系统消息角色设定的情况
            var systemMessage = "";

            StringBuilder sb = new();

            // 是否等待工具调用
            bool toolWait = false;
            // 防止重复添加系统消息
            bool systemAdd = false;
            foreach (var message in history)
            {

                if (message.role == "user")
                {
                    if (toolWait)
                    {
                        // 此处处理异常，工具激活，但是后面没有工具调用反馈
                        functionCalls.Clear();
                        toolWait = false;
                        sb.Append(endToken + "\n");
                    }
                    sb.Append($"{userToken}\n{systemMessage}{message.content}{endToken}\n");
                    systemMessage = "";
                }
                else if (message.role == "system")
                {
                    if (systemAdd || toolWait) continue;
                    systemAdd = true;
                    // 模型不支持系统消息角色设定
                    if (string.IsNullOrEmpty(systemToken))
                    {
                        systemMessage = $"{message.content} {toolPrompt}";
                    }
                    else
                    {
                        sb.Append($"{systemToken}\n{message.content}{toolPrompt}{endToken}\n");
                    }
                }
                else if (message.role == "assistant")
                {
                    // 工具调用后的助理消息
                    if (toolWait)
                    {
                        // 保存工具调用结果
                        if (functionCalls.Count > 0)
                        {
                            foreach (var call in functionCalls)
                            {
                                sb.Append(call.Value + "\n");
                            }
                            functionCalls.Clear();
                        }
                        var toolCallReturn = generator.GenerateToolCallReturn(message.content, toolinfo.Index);
                        sb.Append($"{toolCallReturn}{endToken}{endSentence}\n");
                        toolWait = false;
                    }
                    else
                    {
                        // 存在工具调用
                        if (message.tool_calls?.Length > 0)
                        {
                            sb.Append($"{assistantToken}\n");

                            sb.Append(generator.GetToolPromptConfig(toolinfo.Index).FN_CALL_START + "\n");
                            foreach (var toolCall in message.tool_calls)
                            {
                                var toolCallPrompt = generator.GenerateToolCall(toolCall, toolinfo.Index);
                                sb.Append($"{toolCallPrompt}\n");
                                // 创建占位，等待工具调用结果
                                functionCalls.Add(toolCall.id, "");
                            }
                            sb.Append(generator.GetToolPromptConfig(toolinfo.Index).FN_CALL_END + "\n");

                            var toolSplit = generator.GetToolResultSplit(toolinfo.Index);
                            sb.Append($"{toolSplit}");
                            toolWait = true;
                        }
                        else
                        {
                            var content = message.content;
                            // 去除思考部分
                            if (!string.IsNullOrWhiteSpace(thinkToken))
                            {
                                var parts = content.Split(new[] { thinkToken }, StringSplitOptions.None);
                                content = parts.Last().Trim();
                            }
                            sb.Append($"{assistantToken}\n{content}{endToken}{endSentence}\n");
                        }
                    }
                }
                else if (message.role == "tool")
                {
                    // 异常情况，不应该出现
                    if (message.tool_call_id is null || !toolWait || functionCalls.Count == 0 || !functionCalls.ContainsKey(message.tool_call_id)) continue;
                    var toolCallResult = generator.GenerateToolCallResult(message.content, toolinfo.Index);
                    // 保存工具调用结果
                    functionCalls[message.tool_call_id] = toolCallResult;
                }
            }

            // 增加推理提示符
            var lastMessage = history.LastOrDefault();
            if (lastMessage?.role == "tool" && functionCalls.Count > 0)
            {
                sb.Append(generator.GetToolPromptConfig(toolinfo.Index).FN_RESULT_START + "\n");
                // 添加工具调用结果
                foreach (var call in functionCalls)
                {
                    sb.Append(call.Value + "\n");
                }
                sb.Append(generator.GetToolPromptConfig(toolinfo.Index).FN_RESULT_END + "\n");
                functionCalls.Clear();
                // 添加工具推理提示符
                sb.Append(generator.GetToolPromptConfig(toolinfo.Index).FN_EXIT + "\n");
            }
            else if (toolWait)
            {
                // 异常情况，说明最后一条消息是工具调用，激活了工具，但是没有工具调用结果
                // 结束工具调用，再次提示助理推理
                sb.Append($"{endToken}{assistantToken}\n");
            }
            else
            {
                // 一般情况，添加助理提示符
                sb.Append(assistantToken + "\n");
            }

            var historyText = sb.ToString();

            if (promptTrim)
            {
                // 去除开头末尾的换行符和空格
                historyText = historyText.Trim();
            }

            if (!thinking)
            {
                // 跳过思考过程
                historyText += stopThinking;
            }

            //Console.WriteLine(sb.ToString());
            return historyText;
        }

    }
}