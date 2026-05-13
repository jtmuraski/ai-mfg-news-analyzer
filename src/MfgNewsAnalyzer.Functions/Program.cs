using MfgNewsAnalyzer.Core.Abstractions;
using MfgNewsAnalyzer.Functions.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

builder.Services.AddOptions<RssFeedReaderOptions>()
    .BindConfiguration(RssFeedReaderOptions.SectionName);

builder.Services.AddTransient<IRssFeedReader, RssFeedReader>();

builder.Build().Run();
