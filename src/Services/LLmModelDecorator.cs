using LLamaWorker.Models;
using LLamaWorker.Models.OpenAI;
using Microsoft.Extensions.Options;
using Timer = System.Timers.Timer;

namespace LLamaWorker.Services
{
    /// <summary>
    /// LLM 模型服务 装饰器类
    /// </summary>
    public class LLmModelDecorator : ILLmModelService
    {
        private readonly ILLmModelService _llmService;
        private readonly ILogger<LLmModelService> _logger;

        public bool IsSupportEmbedding => _llmService.IsSupportEmbedding;

        public LLmModelDecorator(IOptions<List<LLmModelSettings>> options, ILogger<LLmModelService> logger)
        {
            _logger = logger;
            _llmService = new LLmModelService(options, logger);

            // 定时器
            if (GlobalSettings.AutoReleaseTime > 0)
            {
                _logger.LogInformation("Auto release time: {time} min.", GlobalSettings.AutoReleaseTime);
                _idleThreshold = TimeSpan.FromMinutes(GlobalSettings.AutoReleaseTime);
                _lastUsedTime = DateTime.Now;
                _idleTimer = new Timer(60000); // 每分钟检查一次
                _idleTimer.Elapsed += CheckIdle;
                _idleTimer.Start();
            }
        }

        public async Task<ChatCompletionResponse> CreateChatCompletionAsync(ChatCompletionRequest request)
        {
            try
            {
                BeginUseModel();
                return await _llmService.CreateChatCompletionAsync(request);
            }
            finally
            {
                EndUseModel();
                _lastUsedTime = DateTime.Now;
            }
        }

        public async IAsyncEnumerable<string> CreateChatCompletionStreamAsync(ChatCompletionRequest request)
        {
            try
            {
                BeginUseModel();
                await foreach (var item in _llmService.CreateChatCompletionStreamAsync(request))
                {
                    yield return item;
                }
            }
            finally
            {
                EndUseModel();
                _lastUsedTime = DateTime.Now;
            }
        }

        public async Task<CompletionResponse> CreateCompletionAsync(CompletionRequest request)
        {
            try
            {
                BeginUseModel();
                return await _llmService.CreateCompletionAsync(request);
            }
            finally
            {
                EndUseModel();
                _lastUsedTime = DateTime.Now;
            }
        }

        public async IAsyncEnumerable<string> CreateCompletionStreamAsync(CompletionRequest request)
        {
            try
            {
                BeginUseModel();
                await foreach (var item in _llmService.CreateCompletionStreamAsync(request))
                {
                    yield return item;
                }
            }
            finally
            {
                EndUseModel();
                _lastUsedTime = DateTime.Now;
            }
        }

        public async Task<EmbeddingResponse> CreateEmbeddingAsync(EmbeddingRequest request)
        {
            try
            {
                BeginUseModel();
                return await _llmService.CreateEmbeddingAsync(request);
            }
            finally
            {
                EndUseModel();
                _lastUsedTime = DateTime.Now;
            }
            
        }

        public IReadOnlyDictionary<string, string> GetModelInfo()
        {
            try
            {
                BeginUseModel();
                return _llmService.GetModelInfo();
            }
            finally
            {
                EndUseModel();
                _lastUsedTime = DateTime.Now;
            }

        }

        public void InitModelIndex()
        {
            if (GlobalSettings.IsModelLoaded && _modelUsageCount != 0)
            {
                _logger.LogWarning("Model is in use.");
                throw new InvalidOperationException("Model is in use.");
            }
            _llmService.InitModelIndex();
        }

        public void Dispose()
        {
            _llmService.Dispose();
        }

        public void DisposeModel()
        {
            _llmService.DisposeModel();
        }

        // 资源释放计时器
        private Timer _idleTimer;
        private DateTime _lastUsedTime;
        private readonly TimeSpan _idleThreshold;

        // 模型使用计数
        // 暂未使用
        private int _modelUsageCount = 0;

        /// <summary>
        /// 模型使用计数 - 开始
        /// </summary>
        public void BeginUseModel()
        {
            // 模型未加载时，初始化模型
            if (!GlobalSettings.IsModelLoaded)
            {
                _llmService.InitModelIndex();
            }
            Interlocked.Increment(ref _modelUsageCount);
        }

        /// <summary>
        /// 模型使用计数 - 结束
        /// </summary>
        public void EndUseModel()
        {
            Interlocked.Decrement(ref _modelUsageCount);
        }

        /// <summary>
        /// 模型自动释放检查
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckIdle(object? sender, object e)
        {
            if (DateTime.Now - _lastUsedTime > _idleThreshold && GlobalSettings.IsModelLoaded && _modelUsageCount == 0)
            {
                _logger.LogInformation("Auto release model.");
                DisposeModel();
            }
        }
    }
}
