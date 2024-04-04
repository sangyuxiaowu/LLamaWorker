using LLama;
using LLama.Batched;
using LLama.Common;
using LLamaWorker.Models;
using LLamaWorker.Models.OpenAI;

namespace LLamaWorker.Services
{
    public class LLmModelService : IDisposable
    {
        private readonly ILogger<LLmModelService> _logger;
        private readonly LLmModelSettings _settings;
        private readonly LLamaWeights _model;
        private readonly LLamaContext _context;

        /// <summary>
        /// 是否已释放资源
        /// </summary>
        private bool _disposedValue = false;

        public LLmModelService(IConfiguration configuration, ILogger<LLmModelService> logger)
        {
            _logger = logger;
            _settings = configuration.GetSection(nameof(LLmModelSettings)).Get<LLmModelSettings>();
            _model = LLamaWeights.LoadFromFile(_settings.ModelParams);
            _context = new LLamaContext(_model, _settings.ModelParams);
        }

        public async Task<ChatCompletionResponse> CreateChatCompletionAsync(ChatCompletionRequest request)
        {
            var genParams = GetInferenceParams(request);
            return null;
        }

        private static InferenceParams GetInferenceParams(ChatCompletionRequest request)
        {
            InferenceParams inferenceParams = new InferenceParams()
            {
                MaxTokens = request.max_tokens ?? 512,
                AntiPrompts = new List<string> { "User:" }
            };
            return inferenceParams;
        }


        /// <summary>
        /// 释放非托管资源
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _model.Dispose();
                }
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
