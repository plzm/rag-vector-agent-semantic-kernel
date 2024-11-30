# Lab 5: Putting it all together

## Learning Objectives

1. Use filters to add logging and understand the call flows
2. Have the LLM determine which plugin functions to call
3. Create a plugin to determine the user's intent
4. Dynamically control the functions available to the LLM depending on the user's intent

### Visual Studio Code

In this lab we extend the chat bot to determine if a user's question should be answered by using the web search (`WebRetrieverPlugin` from lab 3) or the semantic search (`PdfRetrieverPlugin` from lab 4). 

### Add Filters and step through execution
First we are going use [Filters](https://learn.microsoft.com/en-us/semantic-kernel/concepts/enterprise-readiness/filters?pivots=programming-language-csharp) to understand the logic flow and add some logging.

1. Open the labs\lab5\src\start\SK-Workshop-Lab5 folder in VS Code

2. In the Program.cs file, replace line 22 with the following lines:

```C#
builder.Services.AddSingleton<IFunctionInvocationFilter, FunctionInvocationLoggingFilter>();
builder.Services.AddSingleton<IPromptRenderFilter, PromptRenderLoggingFilter>();
builder.Services.AddSingleton<IAutoFunctionInvocationFilter, AutoFunctionInvocationLoggingFilter>();
```

These lines wire up the filters to the dependency container for Semantic Kernel to load at execution time.

All three of these filters were taken form the [Semantic Kernel Samples](https://github.com/microsoft/semantic-kernel/blob/main/dotnet/samples/Concepts/Filtering/TelemetryWithFilters.cs) and modified slightly.

#### AutoFunctionInvocationLoggingFilter
This filter implements `IAutoFunctionInvocationFilter` and is executed during an automatic function calling process - driven by the LLM. Use cases include: early termination of auto function calling, tracking function calling, etc. 

#### FunctionInvocationLoggingFilter
This filter implements `IFunctionInvocationFilter` and is called when a Semantic Kernel function is invoked. Uses cases for this filter include: handling exceptions during function execution, modifying a function result, retries on failures.

#### PromptRenderLoggingFilter
This filter implements `IPromptRenderFilter` and is triggered when a prompt is being rendered. Use cases for this filter include: modifying the prompt before sending to LLM, caching of prompts, removal of PII, etc.

