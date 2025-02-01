﻿using Gradio.Net;
using Gradio.Net.Enums;
using LLamaWorker.Config;
using LLamaWorker.OpenAIModels;
using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpContextAccessor();
builder.Services.AddGradio();
var app = builder.Build();
app.UseGradio(await CreateBlocks());
app.Run();

static async Task<Blocks> CreateBlocks()
{
    using (var blocks = gr.Blocks(analyticsEnabled: false))
    {
        gr.Markdown("# LLamaWorker");
        Textbox input, token;
        Dropdown model;
        Button btnset;

        using (gr.Row())
        {
            input = gr.Textbox("http://localhost:5000", placeholder: "LLamaWorker Server URL", label: "Server");
            token = gr.Textbox(placeholder: "API Key", label: "API Key", maxLines: 1, type: TextboxType.Password);
            btnset = gr.Button("Get Models", variant: ButtonVariant.Primary);
            model = gr.Dropdown(choices: [], label: "Model Select", allowCustomValue: true);
        }

        btnset?.Click(update_models, inputs: [input, token], outputs: [model]);
        model?.Change(change_models, inputs: [input, model], outputs: [model]);

        using (gr.Tab("Chat"))
        {
            Chatbot chatBot = gr.Chatbot(label: "LLamaWorker Chat", showCopyButton: true, placeholder: "Chat history", height: 520);
            Textbox userInput = gr.Textbox(label: "Input", placeholder: "Type a message...");

            Button sendButton, resetButton, regenerateButton;

            using (gr.Row())
            {
                sendButton = gr.Button("✉️ Send", variant: ButtonVariant.Primary);
                regenerateButton = gr.Button("🔃 Retry", variant: ButtonVariant.Secondary);
                resetButton = gr.Button("🗑️  Clear", variant: ButtonVariant.Stop);
            }

            sendButton?.Click(streamingFn: i =>
            {
                string server = Textbox.Payload(i.Data[0]);
                string token = Textbox.Payload(i.Data[3]);
                string model = Dropdown.Payload(i.Data[4]).Single();
                IList<ChatbotMessagePair> chatHistory = Chatbot.Payload(i.Data[1]);
                string userInput = Textbox.Payload(i.Data[2]);
                return ProcessChatMessages(server, token, model, chatHistory, userInput);
            }, inputs: [input, chatBot, userInput, token, model], outputs: [userInput, chatBot]);
            regenerateButton?.Click(streamingFn: i =>
            {
                string server = Textbox.Payload(i.Data[0]);
                string token = Textbox.Payload(i.Data[2]);
                string model = Dropdown.Payload(i.Data[3]).Single();
                IList<ChatbotMessagePair> chatHistory = Chatbot.Payload(i.Data[1]);
                if (chatHistory.Count == 0)
                {
                    throw new Exception("No chat history available for regeneration.");
                }
                string userInput = chatHistory[^1].HumanMessage.TextMessage;
                chatHistory.RemoveAt(chatHistory.Count - 1);
                return ProcessChatMessages(server, token, model, chatHistory, userInput);
            }, inputs: [input, chatBot, token, model], outputs: [userInput, chatBot]);
            resetButton?.Click(i => Task.FromResult(gr.Output(Array.Empty<ChatbotMessagePair>(), "")), outputs: [chatBot, userInput]);
        }

        using (gr.Tab("Completion"))
        {
            var text_Result = gr.Textbox(label: "Generation", interactive: false, lines: 15);
            var text_Input = gr.Textbox(label: "Input", placeholder: "Type a message...", lines: 5);
            var button = gr.Button("Send", variant: ButtonVariant.Primary);
            button?.Click(i =>
            {
                string server = Textbox.Payload(i.Data[0]);
                string token = Textbox.Payload(i.Data[2]);
                string model = Dropdown.Payload(i.Data[3]).Single();
                var inputText = Textbox.Payload(i.Data[1]);
                return ProcessCompletion(server, token, model, inputText);
            }, inputs: [input, text_Input, token, model], outputs: [text_Result]);
        }

        return blocks;
    }
}

static async Task<Output> update_models(Input input)
{
    string server = Textbox.Payload(input.Data[0]);
    string token = Textbox.Payload(input.Data[1]);
    if (server == "")
    {
        throw new Exception("Server URL cannot be empty.");
    }
    if (!string.IsNullOrWhiteSpace(token))
    {
        Utils.client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    }
    var res = await Utils.client.GetFromJsonAsync<ConfigModels>(server + "/models/config");
    if (res?.Models == null || res.Models.Count == 0)
    {
        throw new Exception("Failed to fetch models from the server.");
    }
    Utils.config = res;
    var models = res.Models.Select(x => x.Name).ToList();
    return gr.Output(gr.Dropdown(choices: models, value: models[res.Current], interactive: true));
}

static async Task<Output> change_models(Input input)
{
    string server = Textbox.Payload(input.Data[0]);
    string model = Dropdown.Payload(input.Data[1]).Single();

    var models = Utils.config?.Models?.Select(x => x.Name).ToList();
    // 未使用服务端模型配置，允许自定义模型
    if (models == null)
    {
        return gr.Output(gr.Dropdown(choices: [model], value: model, interactive: true, allowCustomValue: true));
    }

    if (server == "")
    {
        throw new Exception("Server URL cannot be empty.");
    }

    // 取得模型是第几个
    var index = models.IndexOf(model);
    if (index == -1)
    {
        throw new Exception("Model not found in the list of available models.");
    }
    if (Utils.config.Current == index)
    {
        // 没有切换模型
        return gr.Output(gr.Dropdown(choices: models, value: model, interactive: true));
    }
    var res = await Utils.client.PutAsync($"{server}/models/{index}/switch", null);
    // 请求失败
    if (!res.IsSuccessStatusCode)
    {
        // 错误信息未返回
        gr.Warning("Failed to switch model.");
        await Task.Delay(2000);
        return gr.Output(gr.Dropdown(choices: models, value: models[Utils.config.Current], interactive: true));
    }
    Utils.config.Current = index;
    return gr.Output(gr.Dropdown(choices: models, value: model, interactive: true));
}

static async IAsyncEnumerable<Output> ProcessCompletion(string server, string token, string model, string inputText)
{
    if (inputText == "")
    {
        yield return gr.Output("");
        yield break;
    }

    var request = new HttpRequestMessage(HttpMethod.Post, $"{server}/v1/completions");
    request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("text/event-stream"));
    if (!string.IsNullOrWhiteSpace(token))
    {
        Utils.client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    }

    request.Content = new StringContent(JsonSerializer.Serialize(new CompletionRequest
    {
        model = model,
        max_tokens = 1024,
        prompt = inputText,
        stream = true,
    }), Encoding.UTF8, "application/json");

    using var response = await Utils.client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
    response.EnsureSuccessStatusCode();
    using (var stream = await response.Content.ReadAsStreamAsync())
    using (var reader = new System.IO.StreamReader(stream))
    {
        string showed = "";
        while (!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync();
            if (line.StartsWith("data:"))
            {
                var data = line.Substring(5).Trim();
                if (data == "[DONE]")
                {
                    yield break;
                }

                // 解析返回的数据
                var completionResponse = JsonSerializer.Deserialize<CompletionResponse>(data);
                var text = completionResponse?.choices[0]?.text;
                if (string.IsNullOrEmpty(text))
                {
                    continue;
                }
                showed += text;
                yield return gr.Output(showed);
            }
        }
    }

}

static async IAsyncEnumerable<Output> ProcessChatMessages(string server, string token, string model, IList<ChatbotMessagePair> chatHistory, string message)
{
    if (message == "")
    {
        yield return gr.Output("", chatHistory);
        yield break;
    }

    // 添加用户输入到历史记录
    chatHistory.Add(new ChatbotMessagePair(message, ""));

    // sse 请求
    var request = new HttpRequestMessage(HttpMethod.Post, $"{server}/v1/chat/completions");
    request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("text/event-stream"));
    if (!string.IsNullOrWhiteSpace(token))
    {
        Utils.client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    }

    var messages = new List<ChatCompletionMessage>();
    foreach (var item in chatHistory)
    {
        if (!string.IsNullOrEmpty(item.HumanMessage.TextMessage))
        {
            messages.Add(new ChatCompletionMessage
            {
                role = "user",
                content = item.HumanMessage.TextMessage
            });
        }
        if (!string.IsNullOrEmpty(item.AiMessage.TextMessage))
        {
            messages.Add(new ChatCompletionMessage
            {
                role = "assistant",
                content = item.AiMessage.TextMessage
            });
        }
    }

    request.Content = new StringContent(JsonSerializer.Serialize(new ChatCompletionRequest
    {
        stream = true,
        messages = messages.ToArray(),
        model = model,
        max_tokens = 1024,
        temperature = 0.9f,
        top_p = 0.9f,
    }), Encoding.UTF8, "application/json");

    using var response = await Utils.client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
    response.EnsureSuccessStatusCode();
    using (var stream = await response.Content.ReadAsStreamAsync())
    using (var reader = new System.IO.StreamReader(stream))
    {
        while (!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync();
            if (line.StartsWith("data:"))
            {
                var data = line.Substring(5).Trim();

                // 结束
                if (data == "[DONE]")
                {
                    yield break;
                }

                // 解析返回的数据
                var completionResponse = JsonSerializer.Deserialize<ChatCompletionChunkResponse>(data);
                var text = completionResponse?.choices[0]?.delta?.content;
                if (string.IsNullOrEmpty(text))
                {
                    continue;
                }
                chatHistory[^1].AiMessage.TextMessage += text;
                yield return gr.Output("", chatHistory);
            }
        }
    }
}


static class Utils
{
    /// <summary>
    /// HttpClient
    /// </summary>
    public static readonly HttpClient client = new HttpClient();

    /// <summary>
    /// 模型配置信息
    /// </summary>
    public static ConfigModels config;
}


public record ConfigModels
{
    /// <summary>
    /// 当前使用的模型
    /// </summary>
    public int Current { get; set; }
    /// <summary>
    /// 模型列表
    /// </summary>
    public List<ConfigModel> Models { get; set; }

}

public record ConfigModel
{
    /// <summary>
    /// 模型名称
    /// </summary>
    public string Name { get; set; }
}