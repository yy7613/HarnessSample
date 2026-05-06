using Microsoft.Extensions.AI;
using OpenAI;
using System.ClientModel;

namespace HarnessSample;

public sealed class HarnessSampleApp
{
    private readonly HarnessSampleConfiguration _configuration;

    public HarnessSampleApp()
        : this(HarnessSampleConfiguration.Load())
    {
    }

    public HarnessSampleApp(HarnessSampleConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task RunAsync()
    {
        var clientOptions = new OpenAIClientOptions
        {
            Endpoint = new Uri(_configuration.LmStudioUrl)
        };

        var openAIClient = new OpenAIClient(
            new ApiKeyCredential(_configuration.DummyApiKey),
            clientOptions);

        IChatClient chatClient = openAIClient
            .GetChatClient(_configuration.ModelName)
            .AsIChatClient();

        string agentFilesRoot = Path.Combine(AppContext.BaseDirectory, "agent-files");
        Directory.CreateDirectory(agentFilesRoot);

        ConsoleRenderer.PrintStartup(_configuration);

        try
        {
            while (true)
            {
                string? researchTopic = ConsoleRenderer.ReadInteractiveTopic(_configuration.SampleTopics);
                if (researchTopic is null)
                {
                    break;
                }

                using HarnessRuntime runtime = HarnessRuntimeFactory.Create(chatClient, agentFilesRoot, _configuration);
                await HarnessScenarioRunner.RunScenarioAsync(runtime, researchTopic);
            }
        }
        catch (Exception ex)
        {
            ConsoleRenderer.PrintError(ex, _configuration);
        }
    }
}
