namespace LLama.Common
{
    public record ModelParams
    {
        public uint? ContextSize { get; set; }

        public int MainGpu { get; set; }

        public int SplitMode { get; set; }

        public int GpuLayerCount { get; set; } = 20;


        public uint SeqMax { get; set; } = 1u;


        public uint? Seed { get; set; }

        public bool UseMemorymap { get; set; } = true;


        public bool UseMemoryLock { get; set; }

        public string ModelPath { get; set; }

        public object LoraAdapters { get; set; }


        public string LoraBase { get; set; } = string.Empty;


        public uint? Threads { get; set; }

        public uint? BatchThreads { get; set; }

        public uint BatchSize { get; set; } = 512u;


        public uint UBatchSize { get; set; } = 512u;


        public bool Embeddings { get; set; }


        public ModelParams(string modelPath)
        {
            ModelPath = modelPath;
        }

        private ModelParams()
        {
            ModelPath = "";
        }

    }
}
