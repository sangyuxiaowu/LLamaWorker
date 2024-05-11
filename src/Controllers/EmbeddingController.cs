using LLamaWorker.Models.OpenAI;
using LLamaWorker.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace LLamaWorker.Controllers
{
    /// <summary>
    /// 词嵌入控制器
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    [Produces("application/json")]
    public class EmbeddingController : ControllerBase
    {
        private readonly ILogger<EmbeddingController> _logger;
        private readonly LLmModelService _modelService;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _client;

        /// <summary>
        /// 词嵌入控制器
        /// </summary>
        /// <param name="logger">日志</param>
        /// <param name="modelService">llama 服务</param>
        /// <param name="configuration">配置服务</param>
        /// <param name="client">HttpClient</param>
        public EmbeddingController(ILogger<EmbeddingController> logger, LLmModelService modelService, IConfiguration configuration, HttpClient client)
        {
            _logger = logger;
            _modelService = modelService;
            _configuration = configuration;
            _client = client;
        }

        /// <summary>
        /// 创建嵌入
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("/v1/embeddings")]
        [HttpPost("/embeddings")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(EmbeddingResponse))]
        public async Task<IResult> CreateEmbeddingAsync([FromBody] EmbeddingRequest request)
        {
            try
            {
                if(request == null)
                {
                    return Results.BadRequest("Request is null");
                }
                // 使用模型服务创建嵌入
                if (_modelService.IsSupportEmbedding)
                {
                    var response = await _modelService.CreateEmbeddingAsync(request);
                    return Results.Ok(response);
                }
                else
                {
                    // 转发请求
                    var url = _configuration["EmbedingForward"];

                    if (string.IsNullOrEmpty(url))
                    {
                        return Results.BadRequest("EmbedingForward is null");
                    }

                    var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
                    var response = await _client.PostAsync(url, content);
                    if (response.IsSuccessStatusCode)
                    {
                        var result = await response.Content.ReadAsStringAsync();
                        var embeddingResponse = JsonSerializer.Deserialize<EmbeddingResponse>(result);
                        return Results.Ok(embeddingResponse);
                    }
                    else
                    {
                        return Results.BadRequest(response.ReasonPhrase);
                    }
                }
                
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CreateEmbeddingAsync");
                return Results.Problem($"{ex.Message}");
            }
        }

    }
}
