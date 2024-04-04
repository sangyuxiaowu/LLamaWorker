using System.Text;
using System.Text.Json.Nodes;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LLamaWorker.Middleware
{
    public class StopConversionMiddleware
    {
        private readonly RequestDelegate _next;

        /// <summary>
        /// 处理 Completions 请求中的 stop 字段
        /// </summary>
        /// <param name="next"></param>
        public StopConversionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Method == "POST" && context.Request.Path.ToString().Contains("completions"))
            {
                context.Request.EnableBuffering();

                var body = await new StreamReader(context.Request.Body).ReadToEndAsync();
                context.Request.Body.Position = 0;

                using (JsonDocument doc = JsonDocument.Parse(body))
                {
                    var data = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(body);

                    if (data != null && data.TryGetValue("stop", out var stop) && stop.ValueKind == JsonValueKind.String)
                    {
                        data["stop"] = JsonSerializer.Deserialize<JsonElement>(
                            JsonSerializer.Serialize(new string[] { stop.ToString() })
                        );
                        var newBody = JsonSerializer.Serialize(data);
                        context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(newBody));
                    }
                }
            }

            await _next(context);
        }
    }
}
