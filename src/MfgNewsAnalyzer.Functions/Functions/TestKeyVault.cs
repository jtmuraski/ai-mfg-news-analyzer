using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace MfgNewsAnalyzer.Functions.Functions;

public class TestKeyVault
{
    private readonly ILogger<TestKeyVault> _logger;

    public TestKeyVault(ILogger<TestKeyVault> logger)
    {
        _logger = logger;
    }

    [Function(nameof(TestKeyVault))]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req,
        CancellationToken cancellationToken)
    {
        const string vaultUrl = "https://jts-api-keys.vault.azure.net/";
        const string secretName = "Anthropoc--MainApiKey";

        _logger.LogInformation("TestKeyVault invoked");

        var credential = new AzureCliCredential();
        var client = new SecretClient(new Uri(vaultUrl), credential);

        _logger.LogInformation("Attempting to retrieve secret SecretName");
        var secret = await client.GetSecretAsync(secretName, cancellationToken: cancellationToken);

        var secretPrefix = secret.Value.Value.Length >= 10
            ? secret.Value.Value.Substring(0, 10) + "..."
            : "(value shorter than expected)";

        return new OkObjectResult(new
        {
            Status = "Successfully retrieved secret",
            SecretName = secretName,
            SecretPrefix = secretPrefix,
            SecretLength = secret.Value.Value.Length
        });
    }
}