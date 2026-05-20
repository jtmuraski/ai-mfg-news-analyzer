using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MfgNewsAnalyzer.Functions.Functions;

public class TestConfig
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<TestConfig> _logger;

    public TestConfig(IConfiguration configuration, ILogger<TestConfig> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    [Function(nameof(TestConfig))]
    public IActionResult Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
    {
        _logger.LogInformation("TestConfig invoked");

        var anthropicKey = _configuration["Anthropoc:MainApiKey"];

        return new OkObjectResult(new
        {
            HasAnthropicKey = !string.IsNullOrEmpty(anthropicKey),
            AnthropicKeyPrefix = string.IsNullOrEmpty(anthropicKey)
                ? "(not set)"
                : anthropicKey.Substring(0, Math.Min(10, anthropicKey.Length)) + "...",
            AnthropicKeyLength = anthropicKey?.Length ?? 0
        });
    }
}