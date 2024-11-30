// From: https://github.com/microsoft/semantic-kernel/blob/main/dotnet/samples/Concepts/Filtering/TelemetryWithFilters.cs#L231

using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;

namespace SK_Workshop_Lab5.Filters;
public class PromptRenderLoggingFilter : IPromptRenderFilter
{
    private readonly ILogger logger;
    public PromptRenderLoggingFilter(ILoggerFactory loggerFactory)
    {
        logger = loggerFactory.CreateLogger(nameof(PromptRenderLoggingFilter));
    }
    public async Task OnPromptRenderAsync(PromptRenderContext context, Func<PromptRenderContext, Task> next)
    {
        await next(context);

        logger.LogTrace("Rendered prompt: {Prompt}", context.RenderedPrompt);
    }
}
