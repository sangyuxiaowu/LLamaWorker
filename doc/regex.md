# 工具输出结果的正则匹配说明

## 相关工具

- [正则可视化](https://regex-vis.com/)
- [正则匹配测试](https://regex101.com/)

## 相关正则

### Llama

```
\{"name" ?: ?"(.*?)(?:",)*\s*("parameters" ?: ?(.*?"\s*\}))(?=\}|\n|$)
```

### qwen2

```
✿FUNCTION✿:? ?(.*?)\s*(✿ARGS✿:? ?(.*?))(?=✿RESULT✿|$|\n)
```

## 模板编写

### 工具系统提示

系统提示词：

```
{systemToken}
{System}

{FN_CALL_TEMPLATE_INFO}

{FN_CALL_TEMPLATE_INFO.tool_descs}

{FN_CALL_TEMPLATE_FMT*}{endToken}
```

### 推理模板生成

用户消息：

```
{userToken}
{Content}{endToken}
```

工具调用反馈：

```
{assistantToken}
{FN_CALL_TEMPLATE}{FN_RESULT_SPLIT}
{FN_RESULT_TEMPLATE}
{FN_EXIT}
```

FN_CALL_TEMPLATE:
- FN_NAME
- tool.function.name
- FN_ARGS
- tool.function.arguments
