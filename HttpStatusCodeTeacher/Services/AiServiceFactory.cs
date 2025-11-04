namespace HttpStatusCodeTeacher.Services;

/// <summary>
/// Factory for creating AI service instances based on configuration
/// </summary>
public class AiServiceFactory(
    IServiceProvider serviceProvider,
    IConfiguration configuration,
    ILogger<AiServiceFactory> logger)
{
    public IAiService GetAiService()
    {
        var provider = configuration["AI:Provider"]?.ToLower() ?? "gemini";

        logger.LogInformation("Creating AI service for provider: {Provider}", provider);

        return provider switch
        {
            "claude" => serviceProvider.GetRequiredService<ClaudeService>(),
            "gemini" => serviceProvider.GetRequiredService<GeminiService>(),
            _ => throw new InvalidOperationException($"Unsupported AI provider: {provider}")
        };
    }
}
