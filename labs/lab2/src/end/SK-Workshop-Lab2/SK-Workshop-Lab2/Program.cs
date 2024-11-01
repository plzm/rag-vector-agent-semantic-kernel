using Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Plugins.Web;
using Microsoft.SemanticKernel.Plugins.Web.Bing;
using SK_Workshop_Lab2.Plugins;

var builder = Host.CreateApplicationBuilder(args).AddAppSettings();

builder.Services.AddKernel().AddChatCompletionService(builder.Configuration.GetConnectionString("OpenAI"));

var pluginOptions = new PluginOptions();
builder.Configuration.GetSection(PluginOptions.PluginConfig).Bind(pluginOptions);

var app = builder.Build();

var chatCompletionService = app.Services.GetRequiredService<IChatCompletionService>();

#region Step 1
//////////////////////////////////////////////////////////////////////////////////////////////
// Step 1: Use a custom plugin with a prompt

var kernel = app.Services.GetRequiredService<Kernel>();
var plugins = kernel.ImportPluginFromType<DateTimePlugin>("dateTimePlugin");

var timeZone = await plugins["timeZone"].InvokeAsync(kernel);

Console.WriteLine("STEP 1 OUTPUT --------------------------------------------------------------------------------------");
Console.WriteLine($"Local time zone: {timeZone}");

var prompt1 = "What time is it one the west coast of the united states right now? My current timezone {{dateTimePlugin.timeZone}} and current date and time is {{dateTimePlugin.dateWithTime}}";

OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new()
{
    ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions,
    Temperature = 0.7f,
    MaxTokens = 250
};

var step1Result = await chatCompletionService.GetChatMessageContentsAsync(prompt1, openAIPromptExecutionSettings);
foreach (var content in step1Result)
{
    Console.WriteLine($"\nRESPONSE:\n{content}");
}

var promptTemplateFactory = new KernelPromptTemplateFactory();
string userMessage = await promptTemplateFactory.Create(new PromptTemplateConfig(prompt1)).RenderAsync(kernel);

Console.WriteLine($"User message: {userMessage}");

var step1AResult = await chatCompletionService.GetChatMessageContentsAsync(userMessage, openAIPromptExecutionSettings);
foreach (var content in step1AResult)
{
    Console.WriteLine($"\nRESPONSE:\n{content}");
}

#endregion Step 1

#region Step 2
//////////////////////////////////////////////////////////////////////////////////////////////
// Step 2:

var rewriter = kernel.ImportPluginFromType<QueryRewritePlugin>();

var prompt2 = "What are some popular Boston landmarks I should see?";

var result2 = await kernel.InvokeAsync(rewriter["Rewrite"], 
    new()
    {
        { "question", prompt2 }
    });

Console.WriteLine("STEP 2 OUTPUT --------------------------------------------------------------------------------------");
Console.WriteLine($"Rewritten query: {result2}");

#endregion Step 2

#region Step 3
//////////////////////////////////////////////////////////////////////////////////////////////
// Step 3: 

kernel.ImportPluginFromObject(
    new WebSearchEnginePlugin(new BingConnector(pluginOptions.BingApiKey)));

var prompt3 = result2.ToString().Trim('"');

var step3Result = await chatCompletionService.GetChatMessageContentsAsync(prompt3, openAIPromptExecutionSettings);
foreach (var content in step3Result)
{
    Console.WriteLine("STEP 3 OUTPUT --------------------------------------------------------------------------------------");
    Console.WriteLine($"\nRESPONSE:\n{content}");
}


#endregion Step 3