# LLamaWorker

LLamaWorker 是一个基于 [LLamaSharp](https://github.com/SciSharp/LLamaSharp) 项目开发的 HTTP API 服务器。它提供与 OpenAI 兼容的 API，使得开发者可以轻松地将大型语言模型（LLM）集成到自己的应用程序中。

[English](README.md)] | 中文

## 特性

- **兼容 OpenAI API**: 提供与 OpenAI 类似的 API，方便迁移和集成。
- **多模型支持**: 支持配置和切换不同的模型，满足不同场景的需求。
- **流式响应**: 支持流式响应，提高大型响应的处理效率。
- **嵌入支持**: 提供文本嵌入功能，支持多种嵌入模型。
- **对话模版**: 提供了一些常见的对话模版。

## 快速开始

1. 克隆仓库到本地
   ```bash
   git clone https://github.com/sangyuxiaowu/LLamaWorker.git
   ```
2. 进入项目目录
   ```bash
   cd LLamaWorker/src
   ```
3. 根据您的需求选择项目文件。项目提供了三个版本的项目文件：
   - `LLamaWorker.csproj`：适用于 CPU 环境。
   - `LLamaWorker_Cuad11.csproj`：适用于搭载 CUDA 11 的 GPU 环境。
   - `LLamaWorker_Cuad12.csproj`：适用于搭载 CUDA 12 的 GPU 环境。
   
   选择适合您环境的项目文件进行下一步。
   
4. 安装依赖项
   ```bash
   dotnet restore LLamaWorker.csproj
   ```
   如果您使用的是 CUDA 版本，请替换项目文件名。
   
5. 修改配置文件 `appsettings.json`。默认配置已包含一些常见的开源模型配置，您只需按需修改模型文件路径（`ModelPath`）即可。
   
6. 启动服务器
   ```bash
   dotnet run --project LLamaWorker.csproj
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