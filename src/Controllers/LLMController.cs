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

        public LLMController(ILogger<LLMController> logger)
        {
            _logger = logger;
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
        public JsonResult GetModels([FromServices] LLmModelService service)
        {
            var json = service.GetModelInfo();
            return new JsonResult(json);
        }

        /// <summary>
        /// 返回已配置的模型信息
        /// </summary>
        [HttpGet("/models/config")]
        public JsonResult GetConfigModels([FromServices] LLmModelService service)
        {
            var json = service.GetModelSettings();
            return new JsonResult(json);
        }


    }
}
