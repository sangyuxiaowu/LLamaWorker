using LLamaWorker.Models.OpenAI;
using LLamaWorker.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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

        /// <summary>
        /// 词嵌入控制器
        /// </summary>
        /// <param name="logger">日志</param>
        /// <param name="modelService">llama 服务</param>
        public EmbeddingController(ILogger<EmbeddingController> logger, LLmModelService modelService)
        {
            _logger = logger;
            _modelService = modelService;
        }

        /// <summary>
        /// 创建嵌入
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("/v1/embeddings")]
        [HttpPost("/embeddings")]
        public async Task<IResult> CreateEmbeddingAsync([FromBody] EmbeddingRequest request)
        {
            try
            {
                if(request == null)
                {
                    return Results.BadRequest("Request is null");
                }
                if (!_modelService.IsSupportEmbedding)
                {
                    return Results.BadRequest("Model does not support embedding");
                }
                var response = await _modelService.CreateEmbeddingAsync(request);
                return Results.Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CreateEmbeddingAsync");
                return Results.Problem($"{ex.Message}");
            }
        }

    }
}
