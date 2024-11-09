using Azure.AI.OpenAI;
using System.ClientModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

int port = 5000;
if (args.Length > 0 && int.TryParse(args[0], out int parsedPort))
{
    if (parsedPort > 0 && parsedPort < 65535)
        port = parsedPort;
}

var builder = Kernel.CreateBuilder();
builder.Services.AddLogging(c => c.SetMinimumLevel(LogLevel.Trace).AddConsole());

builder.AddOpenAIChatCompletion("default", new AzureOpenAIClient(new Uri($"http://127.0.0.1:{port}"), new ApiKeyCredential("key")));

var kernel = builder.Build();


IChatCompletionService chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

var result = await chatCompletionService.GetChatMessageContentsAsync("hello world");
Console.WriteLine(result.FirstOrDefault()?.Content);