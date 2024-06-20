using Gradio.Net;
using Gradio.Net.Enums;
using LLamaWorker.Models.OpenAI;
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
        Dropdown model;

        using (gr.Row())
        {
            input = gr.Textbox("http://localhost:5000", placeholder: "LLamaWorker Server URL",label:"Server");
            model = gr.Dropdown(choices: ["default"], label: "Model Select");
        }

        using (gr.Tab("Chat"))
        {
            Chatbot chatBot = gr.Chatbot(label: "LLamaWorker Chat", showCopyButton: true, placeholder: "Chat history",height:520);
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
                return ProcessChatMessages(server, chatHistory, userInput);
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
                return ProcessChatMessages(server, chatHistory, userInput);
            }, inputs: [input, chatBot], outputs: [userInput, chatBot]);
            resetButton?.Click(i => Task.FromResult(gr.Output(Array.Empty<ChatbotMessagePair>(), "")), outputs: [chatBot, userInput]);
        }

        using (gr.Tab("Completion"))
        {
            var text_Result = gr.Textbox(label: "Generation", interactive: false,lines:15);
            var text_Input = gr.Textbox(label: "Input", placeholder: "Type a message...", lines: 5);
            var button = gr.Button("Send", variant: ButtonVariant.Primary);
            button?.Click(i =>
            {
                string server = Textbox.Payload(i.Data[0]);
                var inputText = Textbox.Payload(i.Data[1]);
                return ProcessCompletion(server, inputText);
            }, inputs: [input, text_Input], outputs: [text_Result]);
        }

        return blocks;
    }
}

static async IAsyncEnumerable<Output> ProcessCompletion(string server, string inputText)
{
    if (inputText == "")
    {
        yield return gr.Output("");
        yield break;
    }

    var request = new HttpRequestMessage(HttpMethod.Post, $"{server}/v1/completions");
    request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("text/event-stream"));

    request.Content = new StringContent(JsonSerializer.Serialize(new CompletionRequest
    {
        model = "default",
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

static async IAsyncEnumerable<Output> ProcessChatMessages(string server, IList<ChatbotMessagePair> chatHistory, string message)
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
        model = "default",
        max_tokens = 1024
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