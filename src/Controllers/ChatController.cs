using LLamaWorker.Models.OpenAI;
using LLamaWorker.Services;
using Microsoft.AspNetCore.Mvc;
using static System.Net.WebRequestMethods;

namespace LLamaWorker.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ChatController : ControllerBase
    {

        private readonly ILogger<ChatController> _logger;

        public ChatController(ILogger<ChatController> logger)
        {
            _logger = logger;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        [HttpPost("/chat/completions")]
        public async Task<IResult> CreateChatCompletionAsync([FromBody] ChatCompletionRequest request, [FromServices] LLmModelService service)
        {
            try
            {
                if (request.stream)
                {

                    var queue = new Queue<string>();
                    await foreach (var item in service.CreateChatCompletionStreamAsync(request))
                    {
                        queue.Enqueue(item);
                        if (queue.Count > 1)
                        {
                            if (queue.Count == 2)
                            {
                                Response.Headers.ContentType = "text/event-stream";
                                Response.Headers.CacheControl = "no-cache";
                                await Response.Body.FlushAsync();
                                await Response.WriteAsync(queue.Dequeue());
                            }
                            await Response.WriteAsync(queue.Dequeue());
                            await Response.Body.FlushAsync();
                        }
                    }
                    return Results.Empty;
                }
                else
                {
                    return Results.Ok(await service.CreateChatCompletionAsync(request));
                }
                
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error in CreateChatCompletionAsync");
                return Results.Problem($"{ex.Message}");
            }
                
        }
    }
}
