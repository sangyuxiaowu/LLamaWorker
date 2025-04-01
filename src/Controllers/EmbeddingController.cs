using LLamaWorker.Config;
using LLamaWorker.OpenAIModels;
using LLamaWorker.Services;
using Microsoft.AspNetCore.Mvc;
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
        private readonly ILLmModelService _modelService;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _client;

        /// <summary>
        /// 词嵌入控制器
        /// </summary>
        /// <param name="logger">日志</param>
        /// <param name="modelService">llama 服务</param>
        /// <param name="configuration">配置服务</param>
        /// <param name="client">HttpClient</param>
        public EmbeddingController(ILogger<EmbeddingController> logger, ILLmModelService modelService, IConfiguration configuration, HttpClient client)
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
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost("/v1/embeddings")]
        [HttpPost("/embeddings")]
        [HttpPost("/openai/deployments/{model}/embeddings")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(EmbeddingResponse))]
        public async Task<IResult> CreateEmbeddingAsync([FromBody] EmbeddingRequest request, CancellationToken cancellationToken)
        {
            try
            {
                if (request == null)
                {
                    return Results.BadRequest("Request is null");
                }

                if (string.IsNullOrWhiteSpace(GlobalSettings.EmbedingUse))
                {
                    return Results.BadRequest("Embeding support is not enabled");
                }

                if (GlobalSettings.EmbedingUse.StartsWith("http"))
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
                else
                {
                    var response = await _modelService.CreateEmbeddingAsync(request, cancellationToken);
                    return Results.Ok(response);
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
