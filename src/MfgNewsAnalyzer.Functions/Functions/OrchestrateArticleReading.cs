using System;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace MfgNewsAnalyzer.Functions.Functions;

public class OrchestrateArticleReading
{
    private readonly ILogger _logger;

    public OrchestrateArticleReading(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<OrchestrateArticleReading>();
    }

    [Function("OrchestrateArticleReading")]
    public void Run([TimerTrigger("0 0 12 * * 1")] TimerInfo myTimer)
    {
        _logger.LogInformation("C# Timer trigger function executed at: {executionTime}", DateTime.Now);
        
        if (myTimer.ScheduleStatus is not null)
        {
            _logger.LogInformation("Next timer schedule at: {nextSchedule}", myTimer.ScheduleStatus.Next);
        }
    }
}