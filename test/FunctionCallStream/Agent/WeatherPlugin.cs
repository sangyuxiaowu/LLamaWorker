using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace FunctionCall.Agent
{
    public class WeatherPlugin
    {
        [KernelFunction]
        [Description("获取指定城市的当前气温。")]
        public async Task<string> GetCurrentTemperature(
            Kernel kernel,
            [Description("请输入一个城市名称")] string city
        )
        {
            return city switch
            {
                "北京" => "当前气温是 20°C。",
                "上海" => "当前气温是 25°C。",
                "广州" => "当前气温是 30°C。",
                "深圳" => "当前气温是 28°C。",
                _ => "抱歉，我不知道这个城市的气温。"
            };
        }
    }
}
