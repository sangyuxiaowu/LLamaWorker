using LLamaWorker.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LLamaWorker.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class LLMController : ControllerBase
    {
        private readonly ILogger<LLMController> _logger;
        private readonly LLmModelService _modelService;

        public LLMController(ILogger<LLMController> logger, LLmModelService modelService)
        {
            _logger = logger;
            _modelService = modelService;
        }

        /// <summary>
        /// 返回模型的基本信息
        /// </summary>
        /// <param name="service"></param>
        /// <remarks>
        /// 模型 Metadata 信息
        /// </remarks>
        /// <returns></returns>
        [HttpGet("/models/info")]
        public JsonResult GetModels()
        {
            var json = _modelService.GetModelInfo();
            return new JsonResult(json);
        }

        /// <summary>
        /// 返回已配置的模型信息
        /// </summary>
        [HttpGet("/models/config")]
        public JsonResult GetConfigModels()
        {
            var json = _modelService.GetModelSettings();
            return new JsonResult(json);
        }

        /// <summary>
        /// 切换到指定模型
        /// </summary>
        /// <param name="modelId">模型ID</param>
        [HttpPut("/models/{modelId}/switch")]
        public IActionResult SwitchModel(int modelId)
        {
            try
            {
                _modelService.InitModelIndex(modelId, true);
            }
            catch(Exception e)
            {
                return BadRequest(e.Message);
            }
            return Ok();
        }
    }
}
