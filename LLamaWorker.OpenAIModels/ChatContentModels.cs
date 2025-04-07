namespace LLamaWorker.OpenAIModels
{
    /// <summary>
    /// 对话内容，多媒体类型
    /// </summary>
    public class ChatContent
    {
        /// <summary>
        /// 内容类型
        /// 可选值为 text, image_url, input_audio, file
        /// </summary>
        public string type = "text";

        /// <summary>
        /// 文本内容，type 为 text 时必填
        /// </summary>
        public string? text { get; set; }

        /// <summary>
        /// 图像内容，type 为 image_url 时必填
        /// </summary>
        public ImageContent? image_url { get; set; }

        /// <summary>
        /// 音频内容，type 为 input_audio 时必填
        /// </summary>
        public AudioContent? input_audio { get; set; }

        /// <summary>
        /// 文件内容，type 为 file 时必填
        /// </summary>
        public FileContent? file { get; set; }
    }

    /// <summary>
    /// 文件内容
    /// </summary>
    public class FileContent
    {
        /// <summary>
        /// base64 编码的文件数据。
        /// </summary>
        public string? url { get; set; }

        /// <summary>
        /// 文件ID
        /// </summary>
        public string? file_id { get; set; }

        /// <summary>
        /// 文件名
        /// </summary>
        public string? filename { get; set; }
    }

    /// <summary>
    /// 音频内容
    /// </summary>
    public class AudioContent
    {
        /// <summary>
        /// base64 编码的音频数据。
        /// </summary>
        public string? data { get; set; }
        /// <summary>
        /// 编码音频数据的格式
        /// 目前支持 “wav” 和 “mp3”
        /// </summary>
        public string? format { get; set; }
    }


    /// <summary>
    /// 图像内容
    /// </summary>
    public class ImageContent
    {
        /// <summary>
        /// 图像的 URL 或 base64 编码的图像数据。
        /// </summary>
        public string? url { get; set; }

        /// <summary>
        /// 指定图像的详细程度。
        /// </summary>
        public string? detail { get; set; }
    }
}