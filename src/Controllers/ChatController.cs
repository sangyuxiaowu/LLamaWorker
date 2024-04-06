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
                    Response.Headers.ContentType = "text/event-stream";
                    Response.Headers.CacheControl = "no-cache";
                    await Response.Body.FlushAsync();

                    await foreach (var item in service.CreateChatCompletionStreamAsync(request))
                    {
                        await Response.WriteAsync(item);
                        await Response.Body.FlushAsync();
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
                return Results.Problem($"{ex}");
            }
                
        }
    }
}
