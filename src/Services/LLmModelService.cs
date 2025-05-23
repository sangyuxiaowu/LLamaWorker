﻿using LLama;
using LLama.Common;
using LLama.Sampling;
using LLamaWorker.Config;
using LLamaWorker.FunctionCall;
using LLamaWorker.OpenAIModels;
using LLamaWorker.Transform;
using Microsoft.Extensions.Options;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;


namespace LLamaWorker.Services
{
    /// <summary>
    /// LLM 模型服务
    /// </summary>
    public class LLmModelService : ILLmModelService
    {
        private readonly ILogger<LLmModelService> _logger;
        private readonly List<LLmModelSettings> _settings;
        private readonly ToolPromptGenerator _toolPromptGenerator;
        private LLmModelSettings _usedset;
        private LLamaWeights _model;
        private LLamaWeights _emb_model;
        private LLamaEmbedder? _embedder;

        // 已加载模型ID，-1表示未加载
        private int _loadModelIndex = -1;

        private readonly JsonSerializerOptions _jsonSerializerOptions = new()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = false,
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
        };

        private readonly object _modelLock = new object();

        /// <summary>
        /// 初始化指定模型
        /// </summary>
        public void InitModelIndex()
        {
            lock (_modelLock)
            {

                if (_settings.Count == 0)
                {
                    _logger.LogError("No model settings.");
                    throw new ArgumentException("No model settings.");
                }

                // 从配置中获取模型索引
                int loadModelIndex = GlobalSettings.CurrentModelIndex;

                // 检查模型是否已加载，且索引相同
                if (GlobalSettings.IsModelLoaded && _loadModelIndex == loadModelIndex)
                {
                    _logger.LogInformation("Model has been loaded.");
                    return;
                }

                if (loadModelIndex < 0 || loadModelIndex >= _settings.Count)
                {
                    _logger.LogError("Invalid model index: {modelIndex}.", loadModelIndex);
                    throw new ArgumentException("Invalid model index.");
                }

                var usedset = _settings[loadModelIndex];

                if (!usedset.IsExist)
                {
                    _logger.LogError("Model path is error: {path}.", usedset.ModelParams.ModelPath);
                    throw new ArgumentException("Model path is error.");
                }

                // 适用于模型切换，先释放模型资源
                DisposeModel();

                _model = LLamaWeights.LoadFromFile(usedset.ModelParams);
                _usedset = usedset;
                _loadModelIndex = loadModelIndex;
                GlobalSettings.IsModelLoaded = true;

                // 初始化嵌入
                InitEmbedding();
            }
        }

        /// <summary>
        /// 初始化嵌入
        /// </summary>
        public void InitEmbedding()
        {
            // 未启用嵌入或者嵌入是转发模式
            if (string.IsNullOrWhiteSpace(GlobalSettings.EmbedingUse) || GlobalSettings.EmbedingUse.StartsWith("http")) return;
            if (GlobalSettings.EmbedingUse == "now")
            {
                if (_usedset.EmbeddingSupport == 0)
                {
                    _embedder = new LLamaEmbedder(_model, _usedset.ModelParams);
                }
            }
            else
            {
                // 解析嵌入配置为数字
                if (int.TryParse(GlobalSettings.EmbedingUse, out int embIndex) && embIndex >= 0 && embIndex < _settings.Count)
                {
                    var embset = _settings[embIndex];
                    if (!embset.IsExist)
                    {
                        _logger.LogError("Embedding model path is error: {path}.", embset.ModelParams.ModelPath);
                        throw new ArgumentException("Embedding model path is error.");
                    }
                    if (embset.EmbeddingSupport != 1)
                    {
                        _logger.LogError("Embedding set is error: {embedingUse}.", embset.EmbeddingSupport);
                        throw new ArgumentException("Embedding set is error.");
                    }
                    _emb_model = LLamaWeights.LoadFromFile(embset.ModelParams);
                    _embedder = new LLamaEmbedder(_emb_model, embset.ModelParams);
                }
                else
                {
                    _logger.LogError("Invalid embedding model index: {embedingUse}.", GlobalSettings.EmbedingUse);
                    throw new ArgumentException("Invalid embedding model index.");
                }
            }
        }


        /// <summary>
        /// LLmModelService
        /// </summary>
        /// <param name="options">模型配置列表</param>
        /// <param name="logger">日志</param>
        /// <param name="toolPromptGenerator">工具提示词生成器</param>
        public LLmModelService(IOptions<List<LLmModelSettings>> options, ILogger<LLmModelService> logger, ToolPromptGenerator toolPromptGenerator)
        {
            _logger = logger;
            _settings = options.Value;
            _toolPromptGenerator = toolPromptGenerator;
            InitModelIndex();
            if (_usedset == null || _model == null)
            {
                throw new InvalidOperationException("Failed to initialize the model.");
            }
        }

        /// <summary>
        /// 获取模型信息
        /// </summary>
        /// <returns></returns>
        public IReadOnlyDictionary<string, string> GetModelInfo()
        {
            return _model.Metadata;
        }

        #region ChatCompletion

        /// <summary>
        /// 聊天完成
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<ChatCompletionResponse> CreateChatCompletionAsync(ChatCompletionRequest request, CancellationToken cancellationToken)
        {
            // 没有消息
            if (request.messages is null || request.messages.Length == 0)
            {
                _logger.LogWarning("No message in chat history.");
                return new ChatCompletionResponse();
            }

            var chatHistory = GetChatHistory(request);
            var genParams = GetInferenceParams(request, chatHistory.ToolStopWords);
            var ex = new MyStatelessExecutor(_model, _usedset.ModelParams);
            var result = new StringBuilder();

            var messagesContent = request.messages.Select(x => x.content).ToArray();
            var prompt_context = string.Join("", messagesContent);
            var completion_tokens = 0;

            _logger.LogDebug("Prompt context: {prompt_context}", chatHistory.ChatHistory);

            await foreach (var output in ex.InferAsync(chatHistory.ChatHistory, genParams, cancellationToken))
            {
                _logger.LogTrace("Message: {output}", output);
                result.Append(output);
                completion_tokens++;
            }
            var prompt_tokens = ex.PromptTokens;

            _logger.LogDebug("Prompt tokens: {prompt_tokens}, Completion tokens: {completion_tokens}", prompt_tokens, completion_tokens);
            _logger.LogDebug("Completion result: {result}", result);

            // 工具返回检测
            if (chatHistory.IsToolPromptEnabled)
            {
                var tools = _toolPromptGenerator.GenerateToolCall(result.ToString(), _usedset.ToolPrompt.Index);
                if (tools.Count > 0)
                {
                    _logger.LogDebug("Tool calls: {tools}", tools.Select(x => x.name));
                    return new ChatCompletionResponse
                    {
                        id = $"chatcmpl-{Guid.NewGuid():N}",
                        model = request.model,
                        created = DateTimeOffset.Now.ToUnixTimeSeconds(),
                        choices = [
                            new ChatCompletionResponseChoice
                            {
                                index = 0,
                                finish_reason = "tool_calls",
                                message = new ChatCompletionMessage
                                {
                                    role = "assistant",
                                    tool_calls = tools.Select(x => new ToolMeaasge
                                    {
                                        id = $"call_{Guid.NewGuid():N}",
                                        function = new ToolMeaasgeFuntion
                                        {
                                            name = x.name,
                                            arguments = x.arguments
                                        }
                                    }).ToArray()
                                }
                            }
                        ],
                        usage = new UsageInfo
                        {
                            prompt_tokens = prompt_tokens,
                            completion_tokens = completion_tokens,
                            total_tokens = prompt_tokens + completion_tokens
                        }
                    };
                }
            }

            return new ChatCompletionResponse
            {
                id = $"chatcmpl-{Guid.NewGuid():N}",
                model = request.model,
                created = DateTimeOffset.Now.ToUnixTimeSeconds(),
                choices =
                [
                    new ChatCompletionResponseChoice
                    {
                        index = 0,
                        finish_reason = completion_tokens >= request.max_completion_tokens ? "length" : "stop",
                        message = new ChatCompletionMessage
                        {
                            role = "assistant",
                            content = result.ToString()
                        }
                    }
                ],
                usage = new UsageInfo
                {
                    prompt_tokens = prompt_tokens,
                    completion_tokens = completion_tokens,
                    total_tokens = prompt_tokens + completion_tokens
                }
            };
        }

        /// <summary>
        /// 流式生成-聊天完成
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async IAsyncEnumerable<string> CreateChatCompletionStreamAsync(ChatCompletionRequest request, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            // 没有消息
            if (request.messages is null || request.messages.Length == 0)
            {
                _logger.LogWarning("No message in chat history.");
                yield break;
            }

            var chatHistory = GetChatHistory(request);
            var genParams = GetInferenceParams(request, chatHistory.ToolStopWords);
            var ex = new MyStatelessExecutor(_model, _usedset.ModelParams);

            var id = $"chatcmpl-{Guid.NewGuid():N}";
            var created = DateTimeOffset.Now.ToUnixTimeSeconds();

            int index = 0;

            _logger.LogDebug("Prompt context: {prompt_context}", chatHistory.ChatHistory);

            // 第一个消息，带着角色名称
            var chunk = JsonSerializer.Serialize(new ChatCompletionChunkResponse
            {
                id = id,
                created = created,
                model = request.model,
                choices = [
                    new ChatCompletionChunkResponseChoice
                    {
                        index = index,
                        delta = new ChatCompletionMessage
                        {
                            role = "assistant"
                        },
                        finish_reason = null
                    }
                ]
            }, _jsonSerializerOptions);
            yield return $"data: {chunk}\n\n";

            // 拦截前三个 token 存入
            List<string> tokens = new();
            // 工具返回是否激活
            bool toolActive = false;

            // 处理模型输出
            await foreach (var output in ex.InferAsync(chatHistory.ChatHistory, genParams, cancellationToken))
            {
                _logger.LogTrace("Message: {output}", output);

                // 存在工具提示时
                if (chatHistory.IsToolPromptEnabled)
                {
                    // 激活工具提示后保持拦截，未激活时拦截前三个token
                    if (toolActive || tokens.Count < 3)
                    {
                        tokens.Add(output);
                        continue;
                    }
                    // 检查是否有工具提示
                    toolActive = _toolPromptGenerator.IsToolActive(tokens, _usedset.ToolPrompt.Index);
                    if (toolActive)
                    {
                        _logger.LogDebug("Tool is active.");
                        // 激活，继续拦截
                        tokens.Add(output);
                        continue;
                    }
                    else
                    {
                        // 未激活，变更工具启用状态
                        chatHistory.IsToolPromptEnabled = false;
                        // 并快速发送，之前的消息
                        foreach (var token in tokens)
                        {
                            chunk = JsonSerializer.Serialize(new ChatCompletionChunkResponse
                            {
                                id = id,
                                created = created,
                                model = request.model,
                                choices = [
                                    new ChatCompletionChunkResponseChoice
                                    {
                                        index = ++index,
                                        delta = new ChatCompletionMessage
                                        {
                                            role = null,
                                            content = token
                                        },
                                        finish_reason = null
                                    }
                                ]
                            }, _jsonSerializerOptions);
                            yield return $"data: {chunk}\n\n";
                        }
                    }
                }

                chunk = JsonSerializer.Serialize(new ChatCompletionChunkResponse
                {
                    id = id,
                    created = created,
                    model = request.model,
                    choices = [
                           new ChatCompletionChunkResponseChoice
                          {
                            index = ++index,
                            delta = new ChatCompletionMessage
                            {
                                 role = null,
                                 content = output
                            },
                            finish_reason= null
                          }
                      ],

                }, _jsonSerializerOptions);
                yield return $"data: {chunk}\n\n";
            }

            // 是否激活了工具提示拦截
            if (toolActive)
            {
                var result = string.Join("", tokens).Trim();
                var tools = _toolPromptGenerator.GenerateToolCall(result, _usedset.ToolPrompt.Index);
                if (tools.Count > 0)
                {
                    _logger.LogDebug("Tool calls: {tools}", tools.Select(x => x.name));
                    chunk = JsonSerializer.Serialize(new ChatCompletionChunkResponse
                    {
                        id = id,
                        created = created,
                        model = request.model,
                        choices = [
                            new ChatCompletionChunkResponseChoice
                            {
                                index = ++index,
                                delta = new ChatCompletionMessage
                                {
                                    role = null,
                                    tool_calls = tools.Select(x => new ToolMeaasge
                                    {
                                        id = $"call_{Guid.NewGuid():N}",
                                        function = new ToolMeaasgeFuntion
                                        {
                                            name = x.name,
                                            arguments = x.arguments
                                        }
                                    }).ToArray()
                                },
                                finish_reason = "tool_calls"
                            }
                        ]
                    }, _jsonSerializerOptions);
                    yield return $"data: {chunk}\n\n";
                }
                else
                {
                    foreach (var token in tokens)
                    {
                        chunk = JsonSerializer.Serialize(new ChatCompletionChunkResponse
                        {
                            id = id,
                            created = created,
                            model = request.model,
                            choices = [
                                new ChatCompletionChunkResponseChoice
                                {
                                    index = ++index,
                                    delta = new ChatCompletionMessage
                                    {
                                        role = null,
                                        content = token
                                    },
                                    finish_reason = null
                                }
                            ]
                        }, _jsonSerializerOptions);
                        yield return $"data: {chunk}\n\n";
                    }
                }
                // 结束
                chunk = JsonSerializer.Serialize(new ChatCompletionChunkResponse
                {
                    id = id,
                    created = created,
                    model = request.model,
                    choices = [
                        new ChatCompletionChunkResponseChoice
                    {
                        index = tools.Count > 0 ? 0 : ++index,
                        delta = null,
                        finish_reason = tools.Count > 0 ? "tool_calls":"stop"
                    }
                    ]
                }, _jsonSerializerOptions);
                yield return $"data: {chunk}\n\n";
                yield return "data: [DONE]\n\n";
                yield break;
            }

            // 结束
            chunk = JsonSerializer.Serialize(new ChatCompletionChunkResponse
            {
                id = id,
                created = created,
                model = request.model,
                choices = [
                    new ChatCompletionChunkResponseChoice
                    {
                        index = ++index,
                        delta = null,
                        finish_reason = "stop"
                    }
                ]
            }, _jsonSerializerOptions);
            yield return $"data: {chunk}\n\n";
            yield return "data: [DONE]\n\n";
            yield break;
        }


        /// <summary>
        /// 生成对话历史
        /// </summary>
        /// <param name="request">请求信息</param>
        /// <returns></returns>
        private ChatHistoryResult GetChatHistory(ChatCompletionRequest request)
        {
            // 生成工具提示
            var toolPrompt = _toolPromptGenerator.GenerateToolPrompt(request, _usedset.ToolPrompt.Index, _usedset.ToolPrompt.Lang);
            var toolenabled = !string.IsNullOrWhiteSpace(toolPrompt);
            var toolstopwords = toolenabled ? _toolPromptGenerator.GetToolStopWords(_usedset.ToolPrompt.Index) : null;

            var messages = request.messages;

            // 不存在系统消息时，需要添加默认或者空白的系统提示
            if ((toolenabled || !string.IsNullOrWhiteSpace(_usedset.SystemPrompt)) && messages.First()?.role != "system")
            {
                _logger.LogDebug("Add system prompt.");
                messages = messages.Prepend(new ChatCompletionMessage
                {
                    role = "system",
                    content = _usedset.SystemPrompt
                }).ToArray();
            }

            // 使用对话模版
            var history = "";
            if (_usedset.WithTransform?.HistoryTransform != null)
            {
                var type = Type.GetType(_usedset.WithTransform.HistoryTransform);
                if (type != null)
                {
                    var historyTransform = Activator.CreateInstance(type) as ITemplateTransform;
                    if (historyTransform != null)
                    {
                        history = historyTransform.HistoryToText(messages, _toolPromptGenerator, _usedset.ToolPrompt, toolPrompt, request.enable_thinking);
                    }
                }
            }
            else
            {
                history = new BaseHistoryTransform().HistoryToText(messages, _toolPromptGenerator, _usedset.ToolPrompt, toolPrompt, request.enable_thinking);
            }

            return new ChatHistoryResult(history, toolenabled, toolstopwords);
        }

        #endregion

        #region Embedding

        /// <summary>
        /// 创建嵌入
        /// </summary>
        /// <param name="request">请求内容</param>
        /// <param name="cancellationToken"></param>
        /// <returns>词嵌入</returns>
        public async Task<EmbeddingResponse> CreateEmbeddingAsync(EmbeddingRequest request, CancellationToken cancellationToken)
        {

            var embeddings = new List<EmbeddingObject>();

            if (request.input is null || request.input.Length == 0)
            {
                _logger.LogWarning("No input.");
                return new EmbeddingResponse();
            }

            if (_embedder == null)
            {
                _logger.LogWarning("Embedder is null.");
                return new EmbeddingResponse();
            }

            int index = 0;
            foreach (var text in request.input)
            {
                var emb = await _embedder.GetEmbeddings(text, cancellationToken);
                embeddings.Add(new EmbeddingObject
                {
                    embedding = request.encoding_format == "base64" ? ConvertToBase64(emb.First()) : emb.First(),
                    index = index++
                });
            }

            return new EmbeddingResponse
            {
                data = embeddings.ToArray(),
                model = request.model
            };
        }

        /// <summary>
        /// 转换 float 数组 为 Base64 字符串
        /// </summary>
        /// <param name="floatList"></param>
        /// <returns></returns>
        private string ConvertToBase64(float[] floatList)
        {
            byte[] byteArray = new byte[floatList.Length * sizeof(float)];
            Buffer.BlockCopy(floatList, 0, byteArray, 0, byteArray.Length);
            return Convert.ToBase64String(byteArray);
        }

        /// <summary>
        /// 是否支持嵌入
        /// </summary>
        public bool IsSupportEmbedding => _embedder is not null;

        #endregion

        #region Completion

        /// <summary>
        /// 提示完成
        /// </summary>
        public async Task<CompletionResponse> CreateCompletionAsync(CompletionRequest request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.prompt))
            {
                _logger.LogWarning("No prompt.");
                return new CompletionResponse();
            }
            var genParams = GetInferenceParams(request, null);
            var ex = new MyStatelessExecutor(_model, _usedset.ModelParams);
            var result = new StringBuilder();

            var completion_tokens = 0;
            await foreach (var output in ex.InferAsync(request.prompt, genParams, cancellationToken))
            {
                _logger.LogDebug("Message: {output}", output);
                result.Append(output);
                completion_tokens++;
            }
            var prompt_tokens = ex.PromptTokens;

            return new CompletionResponse
            {
                id = $"cmpl-{Guid.NewGuid():N}",
                model = request.model,
                created = DateTimeOffset.Now.ToUnixTimeSeconds(),
                choices = new[]
                {
                    new CompletionResponseChoice
                    {
                        index = 0,
                        text = result.ToString()
                    }
                },
                usage = new UsageInfo
                {
                    prompt_tokens = prompt_tokens,
                    completion_tokens = completion_tokens,
                    total_tokens = prompt_tokens + completion_tokens
                }
            };
        }

        /// <summary>
        /// 流式生成-提示完成
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async IAsyncEnumerable<string> CreateCompletionStreamAsync(CompletionRequest request, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.prompt))
            {
                _logger.LogWarning("No prompt.");
                yield break;
            }
            var genParams = GetInferenceParams(request, null);
            var ex = new MyStatelessExecutor(_model, _usedset.ModelParams);
            var id = $"cmpl-{Guid.NewGuid():N}";
            var created = DateTimeOffset.Now.ToUnixTimeSeconds();
            int index = 0;
            var chunk = JsonSerializer.Serialize(new CompletionResponse
            {
                id = id,
                created = created,
                model = request.model,
                choices = new[]
                {
                    new CompletionResponseChoice
                    {
                        index = index,
                        text = "",
                        finish_reason = null
                    }
                }
            }, _jsonSerializerOptions);
            yield return $"data: {chunk}\n\n";
            await foreach (var output in ex.InferAsync(request.prompt, genParams, cancellationToken))
            {
                _logger.LogDebug("Message: {output}", output);
                chunk = JsonSerializer.Serialize(new CompletionResponse
                {
                    id = id,
                    created = created,
                    model = request.model,
                    choices = new[]
                    {
                        new CompletionResponseChoice
                        {
                            index = ++index,
                            text = output,
                            finish_reason = null
                        }
                    }
                }, _jsonSerializerOptions);
                yield return $"data: {chunk}\n\n";
            }

            chunk = JsonSerializer.Serialize(new CompletionResponse
            {
                id = id,
                created = created,
                model = request.model,
                choices = new[]
                {
                    new CompletionResponseChoice
                    {
                        index = ++index,
                        text = null,
                        finish_reason = "stop"
                    }
                }
            }, _jsonSerializerOptions);
            yield return $"data: {chunk}\n\n";
            yield return "data: [DONE]\n\n";
            yield break;
        }

        #endregion


        /// <summary>
        /// 生成推理参数
        /// </summary>
        /// <param name="request"></param>
        /// <param name="toolstopwords"></param>
        /// <returns></returns>
        private InferenceParams GetInferenceParams(BaseCompletionRequest request, string[]? toolstopwords)
        {
            var stop = new List<string>();
            if (request.stop != null)
            {
                stop.AddRange(request.stop);
            }
            if (_usedset.AntiPrompts?.Length > 0)
            {
                stop.AddRange(_usedset.AntiPrompts);
            }
            if (toolstopwords?.Length > 0)
            {
                stop.AddRange(toolstopwords);
            }
            if (stop.Count > 0)
            {
                //去重，去空，并且至多4个
                stop = stop.Distinct().Where(x => !string.IsNullOrWhiteSpace(x)).Take(4).ToList();
            }

            InferenceParams inferenceParams = new InferenceParams()
            {
                MaxTokens = request.max_completion_tokens.HasValue && request.max_completion_tokens.Value > 0 ? request.max_completion_tokens.Value : -1,
                AntiPrompts = stop,
                SamplingPipeline = request.seed.HasValue ? new DefaultSamplingPipeline
                {
                    Temperature = request.temperature,
                    TopP = request.top_p,
                    PresencePenalty = request.presence_penalty,
                    FrequencyPenalty = request.frequency_penalty,
                    // 确定性采样设置
                    Seed = request.seed.Value
                } : new DefaultSamplingPipeline
                {
                    Temperature = request.temperature,
                    TopP = request.top_p,
                    PresencePenalty = request.presence_penalty,
                    FrequencyPenalty = request.frequency_penalty
                }
            };
            return inferenceParams;
        }


        #region Dispose

        /// <summary>
        /// 主动释放模型资源
        /// </summary>
        public void DisposeModel()
        {
            if (GlobalSettings.IsModelLoaded)
            {
                _embedder?.Dispose();
                _emb_model?.Dispose();
                _model.Dispose();
                GlobalSettings.IsModelLoaded = false;
                _loadModelIndex = -1;
            }
        }

        /// <summary>
        /// 是否已释放资源
        /// </summary>
        private bool _disposedValue = false;

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
                    DisposeModel();
                }
                _disposedValue = true;
            }
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

    }
}
