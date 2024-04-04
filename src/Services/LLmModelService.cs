using LLama;
using LLama.Common;
using LLamaWorker.Models;

namespace LLamaWorker.Services
{
    public class LLmModelService : IDisposable
    {
        private readonly ILogger<LLmModelService> _logger;
        private readonly LLmModelSettings _settings;
        private readonly LLamaContext _context;

        public LLmModelService(ILogger<LLmModelService> logger, LLmModelSettings settings)
        {
            _logger = logger;
            _settings = settings;
            using var model = LLamaWeights.LoadFromFile(settings.ModelParams);
            _context = new LLamaContext(model, settings.ModelParams);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
