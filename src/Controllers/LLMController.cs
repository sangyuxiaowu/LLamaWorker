using LLamaWorker.Config;
using LLamaWorker.Services;
using Microsoft.AspNetCore.Mvc;

namespace LLamaWorker.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class LLMController : ControllerBase
    {
        private readonly ILogger<LLMController> _logger;
        private readonly List<LLmModelSettings> _settings;

        public LLMController(ILogger<LLMController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _settings = configuration.GetSection(nameof(LLmModelSettings)).Get<List<LLmModelSettings>>();
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
        public JsonResult GetModels([FromServices] ILLmModelService service)
        {
            var json = service.GetModelInfo();
            return new JsonResult(json);
        }

        /// <summary>
        /// 返回已配置的模型信息
        /// </summary>
        [HttpGet("/models/config")]
        public ConfigModels GetConfigModels()
        {
            return new ConfigModels
            {
                Models = _settings,
                Loaded = GlobalSettings.IsModelLoaded,
                Current = GlobalSettings.CurrentModelIndex
            };
        }

        /// <summary>
        /// 切换到指定模型
        /// </summary>
        /// <param name="modelId">模型ID</param>
        [HttpPut("/models/{modelId}/switch")]
        public IActionResult SwitchModel(int modelId)
        {
            if (modelId < 0 || modelId >= _settings.Count)
            {
                return BadRequest("Invalid model id");
            }

            // 保存当前模型索引
            int index = GlobalSettings.CurrentModelIndex;

            if (GlobalSettings.CurrentModelIndex == modelId)
            {
                return Ok();
            }

            try
            {
                GlobalSettings.CurrentModelIndex = modelId;
                var service = HttpContext.RequestServices.GetRequiredService<ILLmModelService>();
                service.InitModelIndex();
            }
            catch (Exception e)
            {
                GlobalSettings.CurrentModelIndex = index;
                return BadRequest(e.Message);
            }
            return Ok();
        }
    }
}
