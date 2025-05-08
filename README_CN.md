![](doc/logo.png)

[![Ask DeepWiki](https://deepwiki.com/badge.svg)](https://deepwiki.com/sangyuxiaowu/LLamaWorker)

# LLamaWorker

LLamaWorker 是一个基于 [LLamaSharp](https://github.com/SciSharp/LLamaSharp?wt.mc_id=DT-MVP-5005195) 项目开发的 HTTP API 服务器。它提供与 OpenAI 兼容的 API，使得开发者可以轻松地将大型语言模型（LLM）集成到自己的应用程序中。

[English](README.md) | 中文

## 特性

- **兼容 OpenAI API**: 提供与 OpenAI / Azure OpenAI 类似的 API，方便迁移和集成。
- **多模型支持**: 支持配置和切换不同的模型，满足不同场景的需求。
- **流式响应**: 支持流式响应，提高大型响应的处理效率。
- **嵌入支持**: 提供文本嵌入功能，支持多种嵌入模型，并提供 Base64 返回支持。
- **对话模版**: 提供了一些常见的对话模版。
- **自动释放**: 支持自动释放已加载模型。
- **函数调用**: 支持函数调用。
- **深度思考切换**: 提供 API 参数灵活切换深度思考模式。
- **API Key 认证**: 支持 API Key 认证。
- **测试 UI**: 提供了一个友好的开发测试 UI。
- **Gradio UI Demo**: 提供了一个基于 Gradio.NET 的 UI 演示。

## 测试 UI

当服务器启动后，会检查是否存在 `wwwroot` 目录，如果存在，则会启用静态文件服务。

在 [Releases](../../releases) 中提供了一个 `wwwroot.zip` 文件，您可以将其解压到项目目录下，然后运行项目即可访问测试 UI。

![ui](doc/wwwroot.png)

该界面提供了模型切换、对话完成、提示完成、聊天记录管理、令牌生成速率展示等功能，您可以通过该界面测试模型效果。

## 使用 Vulkan 编译版本

在发布中提供了 Vulkan 后端的编译版本，您可以从 [Releases](../../releases) 中下载对应的编译版本：

- `LLamaWorker-Vulkan-win-x64.zip`
- `LLamaWorker-Vulkan-linux-x64.zip`

下载并解压后，修改 `appsettings.json` 文件中的配置，即可运行软件并开始使用。

> 对于其他后端，您也可以下载 `Vulkan` 版本，前往 [llama.cpp](https://github.com/ggerganov/llama.cpp/releases) 下载对应的编译版本，替换相关类库即可。亦可自行编译 `llama.cpp` 项目获得所需的类库。

## 函数调用

LLamaWorker 支持函数调用，目前在配置文件中提供了三个模板，已经测试了 `Phi-3`，`Qwen2` 和 `Llama3.1` 的函数调用效果。

函数调用兼容 OpenAI 的 API，您可以通过以下 JSON 请求进行测试：

`POST /v1/chat/completions`

```json
{
  "model": "default",
  "messages": [
    {
      "role": "user",
      "content": "北京和上海哪里气温高？"
    }
  ],
  "tools": [
    {
      "function": {
        "name": "GetWeatherPlugin-GetCurrentTemperature",
        "description": "获取指定城市的当前气温。",
        "parameters": {
          "type": "object",
          "required": [
            "city"
          ],
          "properties": {
            "city": {
              "type": "string",
              "description": "城市名称"
            }
          }
        }
      },
      "type": "function"
    },
    {
      "function": {
        "name": "EmailPlugin-SendEmail",
        "description": "向收件人发送电子邮件。",
        "parameters": {
          "type": "object",
          "required": [
            "recipientEmails",
            "subject",
            "body"
          ],
          "properties": {
            "recipientEmails": {
              "type": "string",
              "description": "以分号分隔的收件人电子邮件列表"
            },
            "subject": {
              "type": "string"
            },
            "body": {
              "type": "string"
            }
          }
        }
      },
      "type": "function"
    }
  ],
  "tool_choice": "auto"
}
```

## 源码编译

1. 克隆仓库到本地
   ```bash
   git clone https://github.com/sangyuxiaowu/LLamaWorker.git
   ```
2. 进入项目目录
   ```bash
   cd LLamaWorker
   ```
3. 根据您的需求选择项目文件。项目提供了三个版本的项目文件：
   - `LLamaWorker.Backend.Cpu`：适用于 CPU 环境。
   - `LLamaWorker.Backend.Cuda11`：适用于搭载 CUDA 11 的 GPU 环境。
   - `LLamaWorker.Backend.Cuda12`：适用于搭载 CUDA 12 的 GPU 环境。                                                                                                      
   - `LLamaWorker.Backend.Vulkan`：Vulkan 方案。
   
   选择适合您环境的项目文件进行下一步。
   
4. 安装依赖项
   ```bash
   dotnet restore LLamaWorker.Backend.Cpu\LLamaWorker.Backend.Cpu.csproj
   ```
   如果您使用的是 CUDA 版本，请替换项目文件名。
   
5. 修改配置文件 `appsettings.json`。默认配置已包含一些常见的开源模型配置，您只需按需修改模型文件路径（`ModelPath`）即可。
   
6. 启动服务器
   ```bash
   dotnet run --project LLamaWorker.Backend.Cpu\LLamaWorker.Backend.Cpu.csproj
   ```
   如果您使用的是 CUDA 版本，请替换项目文件名。


## API 参考

LLamaWorker 提供以下 API 端点：

- `/v1/chat/completions`: 对话完成请求
- `/v1/completions`: 提示完成请求
- `/v1/embeddings`: 创建嵌入
- `/models/info`: 返回模型的基本信息
- `/models/config`: 返回已配置的模型信息
- `/models/{modelId}/switch`: 切换到指定模型

## 深度思考切换

提供 API 参数灵活切换深度思考模式。

在请求中添加 `enable_thinking` 参数，默认为 `true` ，针对支持深度思考的模型，设置为 `false`，即可关闭深度思考模式。

```json
{
  "model": "Qwen3-8B",
  "messages": [
    {
      "role": "user",
      "content": "北京和上海哪里气温高？"
    }
  ],
  "enable_thinking": false,
}
```

## Gradio UI Demo

这个 UI 基于 [Gradio.NET](https://github.com/feiyun0112/Gradio.Net?wt.mc_id=DT-MVP-5005195)。

你也可以通过运行以下命令尝试 Gradio UI 演示：

```bash
dotnet restore ChatUI\ChatUI.csproj
dotnet run --project ChatUI\ChatUI.csproj
```

然后打开浏览器访问 Gradio UI 演示。

![](doc/ui.png)
