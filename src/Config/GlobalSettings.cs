namespace LLamaWorker.Config
{
    /// <summary>
    /// 全局设置
    /// </summary>
    public static class GlobalSettings
    {
        /// <summary>
        ///  初始加载的模型索引
        /// </summary>
        public static int CurrentModelIndex { get; set; } = 0;

        /// <summary>
        /// 工具提示词配置索引
        /// </summary>
        public static int CurrentToolPromptIndex { get; set; } = 0;

        /// <summary>
        /// 工具提示词语言
        /// </summary>
        public static string CurrentToolPromptLang { get; set; } = "zh";

        /// <summary>
        ///  模型是否完成了加载
        /// </summary>
        public static bool IsModelLoaded { get; set; } = false;

        /// <summary>
        /// 模型自动释放时间
        /// </summary>
        public static int AutoReleaseTime { get; set; } = 0;

        /// <summary>
        /// 初始化全局设置
        /// </summary>
        /// <param name="configuration">配置信息</param>
        public static void InitializeGlobalSettings(IConfiguration configuration)
        {
            var applicationSettings = configuration.GetSection("GlobalSettings");
            CurrentModelIndex = applicationSettings.GetValue<int>("CurrentModelIndex");
            CurrentToolPromptIndex = applicationSettings.GetValue<int>("CurrentToolPromptIndex");
            CurrentToolPromptLang = applicationSettings.GetValue<string>("CurrentToolPromptLang");
            AutoReleaseTime = applicationSettings.GetValue<int>("AutoReleaseTime");
        }
    }
}
