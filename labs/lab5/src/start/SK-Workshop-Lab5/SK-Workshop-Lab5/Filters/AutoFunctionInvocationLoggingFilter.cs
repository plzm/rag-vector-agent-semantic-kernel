// From https://github.com/microsoft/semantic-kernel/blob/main/dotnet/samples/Concepts/Filtering/TelemetryWithFilters.cs#L245
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using System.Text.Json;

namespace Filters;

public class AutoFunctionInvocationLoggingFilter : IAutoFunctionInvocationFilter
{
    private readonly ILogger logger;
    public AutoFunctionInvocationLoggingFilter(ILoggerFactory loggerFactory)
    {
        logger = loggerFactory.CreateLogger(nameof(AutoFunctionInvocationLoggingFilter));
    }

    public async Task OnAutoFunctionInvocationAsync(AutoFunctionInvocationContext context, Func<AutoFunctionInvocationContext, Task> next)
    {

        if (logger.IsEnabled(LogLevel.Trace))
        {
            logger.LogTrace("ChatHistory: {ChatHistory}", JsonSerializer.Serialize(context.ChatHistory));
        }

        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug("Function count: {FunctionCount}", context.FunctionCount);
        }

        var functionCalls = FunctionCallContent.GetFunctionCalls(context.ChatHistory.Last()).ToList();

        if (logger.IsEnabled(LogLevel.Trace))
        {
            functionCalls.ForEach(functionCall
                => logger.LogTrace(
                    "Function call requests: {PluginName}-{FunctionName}({Arguments})",
                    functionCall.PluginName,
                    functionCall.FunctionName,
                    JsonSerializer.Serialize(functionCall.Arguments)));
        }

        await next(context);
    }
}
