using LLamaWorker.Models.OpenAI;
using Microsoft.AspNetCore.Mvc;

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
        public async Task<ChatCompletionResponse> CreateChatCompletionAsync([FromBody] ChatCompletionRequest request)
        {
            return null;
        }
    }
}
