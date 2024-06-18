﻿using LLamaWorker.Models;
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
        public JsonResult GetModels([FromServices] LLmModelService service)
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
            return new ConfigModels { 
                Models = _settings,
                Current = GlobalSettings.CurrentModelIndex
            };
        }

        /// <summary>
        /// 切换到指定模型
        /// </summary>
        /// <param name="modelId">模型ID</param>
        /// <param name="service"></param>
        [HttpPut("/models/{modelId}/switch")]
        public IActionResult SwitchModel(int modelId, [FromServices] LLmModelService service)
        {
            int index = GlobalSettings.CurrentModelIndex;
            try
            {
                GlobalSettings.CurrentModelIndex = modelId;
                // 加载模型
                service.InitModelIndex();
            }
            catch(Exception e)
            {
                GlobalSettings.CurrentModelIndex = index;
                return BadRequest(e.Message);
            }
            return Ok();
        }
    }
}
