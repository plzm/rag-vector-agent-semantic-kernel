using Memory;
using Microsoft.SemanticKernel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace Plugins;

public class PdfRetrieverPlugin(MemoryStore memoryStore)
{
    
    [KernelFunction, Description("Searched a memory store for answering user's questions.")]
    public async Task<string> RetrieveAsync([Description("User's query"), Required] string question, Kernel kernel)
    {
        var searchQuery = await RewriteAsync(question, kernel);

        var searchResults = await memoryStore.SearchAsync(searchQuery);

        var prompts = kernel.CreatePluginFromPromptDirectory("Prompts");

        var llmResult = await kernel.InvokeAsync(
            prompts["BasicRAG"],
            new() {
                { "question", question },
                { "context", JsonSerializer.Serialize(searchResults) }
            }
        );

        return llmResult.ToString();
    }

    [KernelFunction, Description("Rewrites the user's question for calling a web search.")]
    public async Task<string> RewriteAsync([Description("User's query"), Required] string question, Kernel kernel)
    {
        var prompts = kernel.CreatePluginFromPromptDirectory("Prompts");

        var result = await kernel.InvokeAsync(
            prompts["RewriteQuery"],
            new() {
                { "question", question },
            }
        );

        return result.ToString().Trim('"');
    }
}
