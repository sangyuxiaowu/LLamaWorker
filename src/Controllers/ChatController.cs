using LLamaWorker.Models.OpenAI;
using LLamaWorker.Services;
using Microsoft.AspNetCore.Mvc;

namespace LLamaWorker.Controllers
{
    /// <summary>
    /// 对话完成控制器
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    [Produces("application/json")]
    public class ChatController : ControllerBase
    {

        private readonly ILogger<ChatController> _logger;

        /// <summary>
        /// 对话完成控制器
        /// </summary>
        /// <param name="logger">日志</param>
        public ChatController(ILogger<ChatController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 对话完成请求
        /// </summary>
        /// <param name="request"></param>
        /// <param name="service"></param>
        /// <remarks>
        /// 默认不开启流式，需要主动设置 stream:true
        /// </remarks>
        /// <response code="200">模型对话结果</response>
        /// <response code="400">错误信息</response>
        [HttpPost("/v1/chat/completions")]
        [HttpPost("/chat/completions")]
        [Produces("text/event-stream")]
        [ProducesResponseType(StatusCodes.Status200OK,Type = typeof(ChatCompletionResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
        public async Task<IResult> CreateChatCompletionAsync([FromBody] ChatCompletionRequest request, [FromServices] LLmModelService service)
        {
            try
            {
                if (request.stream)
                {

                    string first = " ";
                    await foreach (var item in service.CreateChatCompletionStreamAsync(request))
                    {
                        if(first == " ")
                        {
                            first = item;
                        }
                        else
                        {
                            if (first.Length > 1)
                            {
                                Response.Headers.ContentType = "text/event-stream";
                                Response.Headers.CacheControl = "no-cache";
                                await Response.Body.FlushAsync();
                                await Response.WriteAsync(first);
                                await Response.Body.FlushAsync();
                                first = "";
                            }
                            await Response.WriteAsync(item);
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
