using Azure.AI.OpenAI;
using FunctionCall.Agent;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

var builder = Kernel.CreateBuilder();
builder.Services.AddLogging(c => c.SetMinimumLevel(LogLevel.Trace).AddConsole());

builder.AddOpenAIChatCompletion("default", new OpenAIClient(new Uri("http://127.0.0.1:5114"), new Azure.AzureKeyCredential("key")));

builder.Plugins.AddFromType<EmailPlugin>();
builder.Plugins.AddFromType<WeatherPlugin>();

var kernel = builder.Build();


IChatCompletionService chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

// 交互式测试工具

ChatHistory chatMessages = new ChatHistory();
while (true)
{
    System.Console.Write("User > ");
    chatMessages.AddUserMessage(Console.ReadLine()!);

    OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new()
    {
        ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions,
    };
    var result = chatCompletionService.GetStreamingChatMessageContentsAsync(
        chatMessages,
        executionSettings: openAIPromptExecutionSettings,
        kernel: kernel);

    string fullMessage = "";
    System.Console.Write("Assistant > ");
    await foreach (var content in result)
    {
        System.Console.Write(content.Content);
        fullMessage += content.Content;
    }
    System.Console.WriteLine();

    chatMessages.AddAssistantMessage(fullMessage);
}