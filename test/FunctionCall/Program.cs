using Azure.AI.OpenAI;
using System.ClientModel;
using FunctionCall.Agent;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

int port = 5000;
if (args.Length > 0 && int.TryParse(args[0], out int parsedPort))
{
    if (parsedPort > 0 && parsedPort < 65535)
        port = parsedPort;
}

var builder = Kernel.CreateBuilder();
builder.Services.AddLogging(c => c.SetMinimumLevel(LogLevel.Trace).AddConsole());

builder.AddOpenAIChatCompletion("default", new AzureOpenAIClient(new Uri($"http://127.0.0.1:{port}"), new ApiKeyCredential("key")));

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
    var result = chatCompletionService.GetChatMessageContentsAsync(
        chatMessages,
        executionSettings: openAIPromptExecutionSettings,
        kernel: kernel);

    string fullMessage = "";
    System.Console.Write("Assistant > ");
    Console.WriteLine(result.Result.FirstOrDefault()?.Content);
    System.Console.WriteLine();

    chatMessages.AddAssistantMessage(fullMessage);
}