using MfgNewsAnalyzer.Core.Abstractions;
using MfgNewsAnalyzer.Functions.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Configuration;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

// Key Vault Set up
Uri vaultUrl = new Uri("https://jts-api-keys.vault.azure.net/");
var credentials = new DefaultAzureCredential(new DefaultAzureCredentialOptions()
{
    ExcludeVisualStudioCredential = true,
    ExcludeVisualStudioCodeCredential = true,
    ExcludeSharedTokenCacheCredential = true,
    ExcludeInteractiveBrowserCredential = true,
    ExcludeAzurePowerShellCredential = true,
    ExcludeAzureDeveloperCliCredential = true,
    ExcludeWorkloadIdentityCredential = true
});

builder.Configuration.AddAzureKeyVault(vaultUrl, credentials);

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

builder.Services.AddOptions<RssFeedReaderOptions>()
    .BindConfiguration(RssFeedReaderOptions.SectionName);

builder.Services.AddTransient<IRssFeedReader, RssFeedReader>();
builder.Services.AddTransient<IArticleContentExtractor, ArticleContentExtractor>();

builder.Build().Run();
