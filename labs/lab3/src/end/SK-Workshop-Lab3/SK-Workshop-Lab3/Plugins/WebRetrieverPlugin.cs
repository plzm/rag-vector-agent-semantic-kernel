using Configuration;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Plugins.Web;
using Microsoft.SemanticKernel.Plugins.Web.Bing;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SK_Workshop_Lab4.Plugins;
public class WebRetrieverPlugin(IOptions<PluginOptions> pluginOptions)
{
    
    [KernelFunction, Description("Searches the web for answering user questions.")]
    public async Task<string> RetrieveAsync([Description("User's query"), Required] string question, Kernel kernel)
    {
        var searchQuery = await RewriteAsync(question, kernel);

        var searchEngine = new WebSearchEnginePlugin(new BingConnector(pluginOptions.Value.BingApiKey));
        var searchResults = await searchEngine.SearchAsync(searchQuery);
        


        var prompts = kernel.CreatePluginFromPromptDirectory("Prompts");

        var llmResult = await kernel.InvokeAsync(
            prompts["BasicRAG"],
            new() {
                { "question", question },
                { "context", searchResults }
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
