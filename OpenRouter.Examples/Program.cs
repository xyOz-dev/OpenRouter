using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenRouter.Core;
using OpenRouter.Examples.BasicUsage;
using OpenRouter.Extensions;

namespace OpenRouter.Examples;

class Program
{
    static async Task Main(string[] args)
    {
        var host = CreateHostBuilder(args).Build();
        
        using var scope = host.Services.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        
        try
        {
            logger.LogInformation("OpenRouter SDK Examples Starting...");
            
            if (!ValidateConfiguration(scope.ServiceProvider))
            {
                logger.LogError("Configuration validation failed. Please set your OpenRouter API key.");
                return;
            }

            var menuOptions = new Dictionary<string, (string Description, Func<IServiceProvider, Task> Action)>
            {
                ["1"] = ("Basic Chat Completion", RunBasicChatExample),
                ["2"] = ("Streaming Chat", RunStreamingChatExample),
                ["3"] = ("Conversation Example", RunConversationExample),
                ["4"] = ("Custom Parameters", RunCustomParametersExample),
                ["5"] = ("List Models", RunListModelsExample),
                ["6"] = ("Filter Models", RunFilterModelsExample),
                ["7"] = ("Model Details", RunModelDetailsExample),
                ["8"] = ("Multimodal Models", RunMultimodalModelsExample),
                ["9"] = ("Compare Models", RunCompareModelsExample),
                ["10"] = ("Text Summarization", RunTextSummarizationExample),
                ["all"] = ("Run All Examples", RunAllExamples),
                ["quit"] = ("Exit", _ => Task.CompletedTask)
            };

            while (true)
            {
                DisplayMenu(menuOptions);
                
                var choice = Console.ReadLine()?.Trim().ToLower();
                
                if (string.IsNullOrEmpty(choice))
                    continue;

                if (choice == "quit" || choice == "q")
                    break;

                if (menuOptions.TryGetValue(choice, out var option))
                {
                    try
                    {
                        await option.Action(scope.ServiceProvider);
                        
                        if (choice != "all")
                        {
                            logger.LogInformation("Example completed. Press any key to continue...");
                            Console.ReadKey();
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Error running example: {Message}", ex.Message);
                        Console.WriteLine("Press any key to continue...");
                        Console.ReadKey();
                    }
                }
                else
                {
                    logger.LogWarning("Invalid choice: {Choice}", choice);
                }
                
                Console.Clear();
            }
            
            logger.LogInformation("Examples completed. Goodbye!");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception in examples program");
        }
    }

    private static void DisplayMenu(Dictionary<string, (string Description, Func<IServiceProvider, Task> Action)> options)
    {
        Console.WriteLine("=== OpenRouter SDK Examples ===");
        Console.WriteLine();
        
        foreach (var kvp in options.Where(x => x.Key != "quit"))
        {
            Console.WriteLine($"{kvp.Key.PadRight(4)} - {kvp.Value.Description}");
        }
        
        Console.WriteLine();
        Console.WriteLine("quit - Exit");
        Console.WriteLine();
        Console.Write("Choose an option: ");
    }

    private static bool ValidateConfiguration(IServiceProvider serviceProvider)
    {
        var client = serviceProvider.GetRequiredService<IOpenRouterClient>();
        var apiKey = client.Options.ApiKey;
        
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            Console.WriteLine();
            Console.WriteLine("ERROR: OpenRouter API key not configured.");
            Console.WriteLine("Please set your API key in one of the following ways:");
            Console.WriteLine("1. Set the OPENROUTER_API_KEY environment variable");
            Console.WriteLine("2. Update appsettings.json with your API key");
            Console.WriteLine("3. Pass the API key as a command line argument");
            Console.WriteLine();
            Console.WriteLine("You can get your API key at: https://openrouter.ai/keys");
            return false;
        }
        
        return true;
    }

    private static async Task RunBasicChatExample(IServiceProvider serviceProvider)
    {
        var example = serviceProvider.GetRequiredService<BasicChatExample>();
        await example.RunBasicChatAsync();
    }

    private static async Task RunStreamingChatExample(IServiceProvider serviceProvider)
    {
        var example = serviceProvider.GetRequiredService<BasicChatExample>();
        await example.RunStreamingChatAsync();
    }

    private static async Task RunConversationExample(IServiceProvider serviceProvider)
    {
        var example = serviceProvider.GetRequiredService<BasicChatExample>();
        await example.RunConversationAsync();
    }

    private static async Task RunCustomParametersExample(IServiceProvider serviceProvider)
    {
        var example = serviceProvider.GetRequiredService<BasicChatExample>();
        await example.RunCustomParametersAsync();
    }

    private static async Task RunListModelsExample(IServiceProvider serviceProvider)
    {
        var example = serviceProvider.GetRequiredService<ModelsExample>();
        await example.RunListModelsAsync();
    }

    private static async Task RunFilterModelsExample(IServiceProvider serviceProvider)
    {
        var example = serviceProvider.GetRequiredService<ModelsExample>();
        await example.RunFilterModelsAsync();
    }

    private static async Task RunModelDetailsExample(IServiceProvider serviceProvider)
    {
        var example = serviceProvider.GetRequiredService<ModelsExample>();
        await example.RunModelDetailsAsync();
    }

    private static async Task RunMultimodalModelsExample(IServiceProvider serviceProvider)
    {
        var example = serviceProvider.GetRequiredService<ModelsExample>();
        await example.RunMultimodalModelsAsync();
    }

    private static async Task RunCompareModelsExample(IServiceProvider serviceProvider)
    {
        var example = serviceProvider.GetRequiredService<ModelsExample>();
        await example.RunCompareModelsAsync();
    }

    private static async Task RunTextSummarizationExample(IServiceProvider serviceProvider)
    {
        var example = serviceProvider.GetRequiredService<BasicChatExample>();
        await example.RunTextSummarizationAsync();
    }

    private static async Task RunAllExamples(IServiceProvider serviceProvider)
    {
        var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("Running all examples...");
        
        var chatExample = serviceProvider.GetRequiredService<BasicChatExample>();
        var modelsExample = serviceProvider.GetRequiredService<ModelsExample>();
        
        await chatExample.RunBasicChatAsync();
        await Task.Delay(1000);
        
        await chatExample.RunStreamingChatAsync();
        await Task.Delay(1000);
        
        await chatExample.RunConversationAsync();
        await Task.Delay(1000);
        
        await modelsExample.RunListModelsAsync();
        await Task.Delay(1000);
        
        await modelsExample.RunModelDetailsAsync();
        await Task.Delay(1000);

        await chatExample.RunTextSummarizationAsync();
        
        logger.LogInformation("All examples completed!");
    }

    private static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((context, config) =>
            {
                config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                config.AddEnvironmentVariables();
                config.AddCommandLine(args);
            })
            .ConfigureServices((context, services) =>
            {
                var apiKey = context.Configuration["OpenRouter:ApiKey"] ?? 
                           Environment.GetEnvironmentVariable("OPENROUTER_API_KEY");

                services.AddOpenRouter(options =>
                {
                    options.ApiKey = apiKey;
                    options.BaseUrl = context.Configuration["OpenRouter:BaseUrl"] ?? "https://openrouter.ai/api/v1";
                    options.HttpReferer = context.Configuration["OpenRouter:HttpReferer"];
                    options.XTitle = context.Configuration["OpenRouter:XTitle"];
                    // User agent can be set via DefaultHeaders if needed
                    var userAgent = context.Configuration["OpenRouter:UserAgent"] ?? "OpenRouter.Examples/1.0.0";
                    options.DefaultHeaders["User-Agent"] = userAgent;
                });

                services.AddTransient<BasicChatExample>();
                services.AddTransient<ModelsExample>();
            })
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddConsole();
                logging.SetMinimumLevel(LogLevel.Information);
            });
}