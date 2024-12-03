# LAB 6: First Agent, a City Poet  agent based on ChatCompletionAgent

## Setup

1. Create a Dotnet app console app Hello World

    ```powershell
    dotnet new console -n myAgentConsoleApp
    ```
    
    Test de app running it.

    ![Hello World](./assets/helloworld.jpg)
2. Installing the SDK 
    Semantic Kernel has several NuGet packages available. 
    
    You can install it using the following command in your console app directory:

    ```powershell
    cd .\myAgentConsoleApp\
    dotnet add package Microsoft.SemanticKernel
    dotnet add package Microsoft.Extensions.Logging
    dotnet add package Microsoft.Extensions.Logging.Console
    dotnet add package Azure.Identity
    dotnet add package Microsoft.Extensions.Configuration
    dotnet add package Microsoft.Extensions.Configuration.Binder
    dotnet add package Microsoft.Extensions.Configuration.UserSecrets
    dotnet add package Microsoft.Extensions.Configuration.EnvironmentVariables
    dotnet add package Microsoft.SemanticKernel.Connectors.AzureOpenAI
    dotnet add package Microsoft.SemanticKernel.Agents.Core --prerelease
    ```

    The Agent Framework is experimental and requires warning suppression. This may addressed in as a property in the project file (.csproj):

    ```XML
    <PropertyGroup>
        <NoWarn>$(NoWarn);CA2007;IDE1006;SKEXP0001;SKEXP0110;OPENAI001</NoWarn>
    </PropertyGroup>
    ```

3. To securely store and read secrets in your .NET application, you can use the Microsoft.Extensions.Configuration package along with a secrets management tool like Azure Key Vault or the .NET Secret Manager for local development.
    Using .NET Secret Manager (for local development)
    
    3.1 Install the necessary NuGet packages:

    ```powershell
    dotnet add package Microsoft.Extensions.Configuration
    dotnet add package Microsoft.Extensions.Configuration.Json
    dotnet add package Microsoft.Extensions.Configuration.UserSecrets 
    ```

    3.2 Initialize user secrets in your project:
    ```powershell
    dotnet user-secrets init
    ```

    3.3  Set secrets using the .NET CLI:
    ```powershell
    dotnet user-secrets set "OpenAI:ModelId" "your_model_id"
    dotnet user-secrets set "OpenAI:Endpoint" "your_endpoint"
    dotnet user-secrets set "OpenAI:ApiKey" "your_api_key"
    ```
 
## Basic Semantic Kernel code and Kernel creation
4. Replace Program.cs code to create your first SK Kernel
    ```csharp
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Microsoft.SemanticKernel;
    using Microsoft.SemanticKernel.ChatCompletion;
    using Microsoft.SemanticKernel.Connectors.OpenAI;


    using Microsoft.SemanticKernel.Agents;
    using Microsoft.SemanticKernel.Connectors.AzureOpenAI;

    using Microsoft.Extensions.Configuration;

    namespace AgentsSample;
    class Program
    {

        static Kernel CreateKernel()
        {
            var configuration = new ConfigurationBuilder()
                    .AddUserSecrets<Program>()
                    .Build();

            var modelId = configuration["OpenAI:ModelId"] ?? throw new InvalidOperationException("Model ID not set in secrets.");
            var endpoint = configuration["OpenAI:Endpoint"] ?? throw new InvalidOperationException("Endpoint not set in secrets.");
            var apiKey = configuration["OpenAI:ApiKey"] ?? throw new InvalidOperationException("API Key not set in secrets.");

            var builder = Kernel.CreateBuilder().AddAzureOpenAIChatCompletion(modelId, endpoint, apiKey);
            builder.Services.AddLogging(services => services.AddConsole().SetMinimumLevel(LogLevel.Trace));
            return builder.Build();
        }
        static async Task Main(string[] args)
        {
            //1. Create a Semantic Kernel KERNEL for the agent
            Console.WriteLine("Begining Kernel creation!");    
            Kernel myKernel =  CreateKernel();
            Console.WriteLine("Kernel created!");
        }

    }
    ```
5. Run and test your enviroment variables before move forward.

    You should see the Kernel messages in the console.


    ![First run](./assets/FirstRun.png)

## Agent Creation.

6. Create the first Agent, City Poet Agent. This agent has not tools only a LLM that helps it to answer.

    The system prompt of the agent define what the agend does.

    6.1 Create Agent Method.

    Add the following method to the Program class.
    ```csharp
    /// <summary>
    /// Create a CityPoetAgent with basic functionality
    /// </summary>
    /// <param name="AgentKernel">Kernel to be used by the agent</param>
    /// <returns></returns>
    static ChatCompletionAgent CreateAgentCityPoetBasic(Kernel AgentKernel)
    {
        //1. Create a CityPoetAgent
        Console.WriteLine("Defining agent...");
        ChatCompletionAgent agent =
            new()
            {
                // 1.1 Name of the agent
                Name = "CityPoetAgent",
                // 1.2 Instructions for the agent, this is the definition of the agent behavior. Should be clear.
                // This instruction is simple, the agent only writes poems about cities.
                // It also includes the current date and time. $now is a placeholder for the current date and time.
                Instructions =
                    """
                    You are an agent designed to write PoemagentKernel based on a suject and another poet tone.
                    
                    Use the current date and time to provide up-to-date details or time-sensitive responses, 
                    the current date and time is: {{$now}}. include the date and time in the poem.

                    You only write poems about Cities, if the subject is not a city, you will not write a poem.

                    """,
                // 1.3 Kernel to use for the agent
                Kernel = AgentKernel
            };
        Console.WriteLine(" Agent created!");
        return agent;
    }
    ```

    6.2 Create the Chat loop metod.
    ```csharp
    /// <summary>
    /// Call the CityPoetAgent with basic functionality, this method will start a conversation with the agent
    /// </summary>
    /// <param name="AgentKernel"> The agent's kernel</param>
    /// <returns></returns>
    static async Task Call_CityPoetAgentBasic(Kernel AgentKernel)
    {
        //1. Create a CityPoetAgent
        ChatCompletionAgent agent = CreateAgentCityPoetBasic(AgentKernel);
        //2. Create a ChatHistory
        ChatHistory history = [];
        bool isComplete = false;
        //3. Start the conversation loop, to exit the loop, the user must not provide a subject
        do
        {
            //3.1 User inputs
            Console.WriteLine("Enter a subject for the poem, or press enter to exit.");
            Console.Write("User > ");
            string userInput = Console.ReadLine() ?? string.Empty;
            
            if (string.IsNullOrWhiteSpace(userInput))
            {
                //continue;
                isComplete = true;
                break;
            }
            //3.2 Add user input to the history
            history.Add(new ChatMessageContent(AuthorRole.User, userInput));
            Console.WriteLine();
            DateTime now = DateTime.Now;
            //3.3 Create arguments to send to the Agent, in this case, we are only sending the current date and time
            KernelArguments arguments =
               new()
                {
                    { "now", $"{now.ToShortDateString()} {now.ToShortTimeString()}" }
                };
            //3.4 Invoke the agent
            Console.WriteLine();
            await foreach (var message in agent.InvokeStreamingAsync(history, arguments))
            {   
                Console.Write(message);
            }
            Console.WriteLine();

        } while (!isComplete);
        
    }
    ```
    6.3 Update Main method to call the Agent adding this code.

    ```csharp
        //1. Create a Semantic Kernel KERNEL for the agent
        Console.WriteLine("Begining Kernel creation!");    
        Kernel myKernel =  CreateKernel();
        Console.WriteLine("Kernel created!");

        //2. Clone the Kernel for the agent
        Kernel theAgentKernel = myKernel.Clone();
        //3. Select which LAB going to be executed
        Console.WriteLine("Select which lab to execute:");
        Console.WriteLine("1. Call_CityPoetAgentBasic");
        Console.WriteLine("2. Call_CityPoetAgentWithSkills");
        Console.WriteLine("3. WriterReviewGroupAgent");
        Console.WriteLine("4. TravelAgentGroupChatSecuential");
        Console.WriteLine("5. TravelAgentGroupChatStrategy");

        var choice = Console.ReadLine();
        switch (choice)
        {
            case "1":
                await Call_CityPoetAgentBasic(myKernel.Clone());
                break;
            case "2":
                //await Call_CityPoetAgentWithSkills(myKernel.Clone());
                new NotImplementedException("This lab is not implemented yet.");
                break;
            case "3":
                //await ProgramChatGroupAgent.WriterReviewGroupAgent(theAgentKernel);
                new NotImplementedException("This lab is not implemented yet.");
                break;
            case "4":
                //await ProgramChatGroupAgent.TravelAgentGroupChatSecuential(myKernel.Clone());
                new NotImplementedException("This lab is not implemented yet.");
                break;
            case "5":
                //await ProgramChatGroupAgent.TravelAgentGroupChatStrategy(myKernel.Clone());
                new NotImplementedException("This lab is not implemented yet.");
                break;
            default:
                Console.WriteLine("Invalid choice. Please select a valid lab number.");
                break;
        }
    ```

    6.4 Test the agent. The program ask for 2 user inputs: Subject of the poem and poet tone to use. After select option 1, try with this cities.
        
        a. Paris, France

        b. New York City 

        c. London, England  

        d. Moscow, Russia  

        e. Delhi, India  

        The agent write a peom about the city and include the day and time passed as argument to the agent. Example outcome

    ![SampleOutome](./assets/one.png)
    
    6.5 The agent only write poems about the cities, that is definedin the Agent instructions. Try to ask:
        
        Subject:   Paul Revere's Ride
       
        The answer should be similar to the following.
    ![SampleOutome2](./assets/two.png)

    6.6 Finish the interaction with the agent sending and empty input and th program would finish.
