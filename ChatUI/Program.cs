using Gradio.Net;
using Gradio.Net.Enums;
using LLamaWorker.Models;
using System.Reflection;

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

        Chatbot chatBot = gr.Chatbot(label: "聊天窗口", showCopyButton: true, placeholder: "这里显示聊天历史记录");
        Textbox userInput = gr.Textbox(label: "用户输入", placeholder: "请输入你的问题或指令...");

        Button sendButton, resetButton, regenerateButton;

        using (gr.Row())
        {
            sendButton = gr.Button("✉️发送", variant: ButtonVariant.Primary);
            regenerateButton = gr.Button("🔃重新生成", variant: ButtonVariant.Secondary);
            resetButton = gr.Button("🔄重置聊天", variant: ButtonVariant.Stop);
        }

        sendButton?.Click(streamingFn: i =>
        {
            IList<ChatbotMessagePair> chatHistory = Chatbot.Payload(i.Data[0]);
            string userInput = Textbox.Payload(i.Data[1]);
            return Outputs(chatHistory, userInput);
        }, inputs: [chatBot, userInput], outputs: [userInput, chatBot]);
        regenerateButton?.Click(streamingFn: i =>
        {
            IList<ChatbotMessagePair> chatHistory = Chatbot.Payload(i.Data[0]);
            if (chatHistory.Count == 0)
            {
                throw new Exception("No chat history available for regeneration.");
            }
            string userInput = chatHistory[^1].HumanMessage.TextMessage;
            chatHistory.RemoveAt(chatHistory.Count - 1);
            return Outputs(chatHistory, userInput);
        }, inputs: [chatBot], outputs: [userInput, chatBot]);
        resetButton?.Click(i => Task.FromResult(gr.Output(Array.Empty<ChatbotMessagePair>(), "")), outputs: [chatBot, userInput]);

        return blocks;
    }
}

static async IAsyncEnumerable<Output> Outputs(IList<ChatbotMessagePair> chatHistory, string message)
{
    if (message == "")
    {
        yield return gr.Output("", chatHistory);
        yield break;
    }
    
}