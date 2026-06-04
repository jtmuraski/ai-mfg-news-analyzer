using MfgNewsAnalyzer.Core.Abstractions;
using MfgNewsAnalyzer.Functions.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Azure.Identity;
using Microsoft.Extensions.Configuration;
using MfgNewsAnalyzer.Functions.Services.Options;
using MfgNewsAnalyzer.Functions.Services.Repositories;
using Microsoft.Azure.Cosmos;
using Azure.Communication.Email;
using Microsoft.Extensions.Options;

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

// Add Service Options
builder.Services.AddOptions<RssFeedReaderOptions>()
    .BindConfiguration(RssFeedReaderOptions.SectionName);

builder.Services.AddOptions<ClaudeAnalyzerOptions>()
    .BindConfiguration(ClaudeAnalyzerOptions.SectionName);

builder.Services.AddOptions<CosmosOptions>()
    .BindConfiguration(CosmosOptions.SectionName);

builder.Services.AddOptions<EmailOptions>()
    .BindConfiguration(EmailOptions.SectionName);

// Add Instances of Services
builder.Services.AddTransient<IRssFeedReader, RssFeedReader>();
builder.Services.AddTransient<IArticleContentExtractor, ArticleContentExtractor>();
builder.Services.AddTransient<IArticleAnalyzer, ArticleAnalyzer>();
builder.Services.AddTransient<IEmailService, EmailService>();

// Clients
// Set the serializer
var serializer = new CosmosSerializationOptions
{
    PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
};

builder.Services.AddSingleton<CosmosClient>(sp =>
{
    var options = sp.GetRequiredService<IOptions<CosmosOptions>>().Value;
    return new CosmosClient(options.EndpointUri, credentials, new CosmosClientOptions
    {
        SerializerOptions = serializer
    });
});

builder.Services.AddSingleton<EmailClient>(sp =>
{
    var options = sp.GetRequiredService<IOptions<EmailOptions>>().Value;
    return new EmailClient(new Uri(options.AzureEmailServiceEndpoint), credentials);
});

// Repositories
builder.Services.AddSingleton<IArticleRepository, CosmosArticleRepository>();

builder.Build().Run();
