using Gradio.Net;
using Gradio.Net.Enums;
using LLamaWorker.Models;
using LLamaWorker.Models.OpenAI;
using System.Reflection;
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
    using (var blocks = gr.Blocks(analyticsEnabled:false,title: "LLamaWorker"))
    {
        gr.Markdown("# LLamaWorker");
        Textbox input;
        List<string> knownModels = new List<string>() { "default" };
        int curentModels = 0;
        Dropdown model;

        using (gr.Row())
        {
            input = gr.Textbox("http://localhost:5000", placeholder: "LLamaWorker Server URL",label:"Server");
            model = gr.Dropdown(knownModels, knownModels[curentModels], label: "Model Select");
        }

        Chatbot chatBot = gr.Chatbot(label: "LLamaWorker Chat", showCopyButton: true, placeholder: "Chat history");
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
            IList<ChatbotMessagePair> chatHistory = Chatbot.Payload(i.Data[1]);
            string userInput = Textbox.Payload(i.Data[2]);
            return Outputs(server, chatHistory, userInput);
        }, inputs: [input, chatBot, userInput], outputs: [userInput, chatBot]);
        regenerateButton?.Click(streamingFn: i =>
        {
            string server = Textbox.Payload(i.Data[0]);
            IList<ChatbotMessagePair> chatHistory = Chatbot.Payload(i.Data[1]);
            if (chatHistory.Count == 0)
            {
                throw new Exception("No chat history available for regeneration.");
            }
            string userInput = chatHistory[^1].HumanMessage.TextMessage;
            chatHistory.RemoveAt(chatHistory.Count - 1);
            return Outputs(server, chatHistory, userInput);
        }, inputs: [input, chatBot], outputs: [userInput, chatBot]);
        resetButton?.Click(i => Task.FromResult(gr.Output(Array.Empty<ChatbotMessagePair>(), "")), outputs: [chatBot, userInput]);

        return blocks;
    }
}

static async IAsyncEnumerable<Output> Outputs(string server, IList<ChatbotMessagePair> chatHistory, string message)
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

    var messages =new List<ChatCompletionMessage>();
    foreach (var item in chatHistory)
    {
        messages.Add(new ChatCompletionMessage
        {
            role = "user",
            content = item.HumanMessage.TextMessage
        });
        messages.Add(new ChatCompletionMessage
        {
            role = "assistant",
            content = item.AiMessage.TextMessage
        });
    }
    messages.Add(new ChatCompletionMessage
    {
        role = "user",
        content = message
    });


    request.Content = new StringContent(JsonSerializer.Serialize(new ChatCompletionRequest
    {
        stream = true,
        messages = messages.ToArray(),
        model = "default"
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
                if(data == "[DONE]")
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
    public static readonly HttpClient client = new HttpClient();
}