using System;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace RetreiveMfgNewsArticles
{
    public class GetMfgNews
    {
        private readonly ILogger _logger;

        public GetMfgNews(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<GetMfgNews>();
        }

        [Function("GetMfgNews")]
        public void Run([TimerTrigger("0 0 6 * * MON")] TimerInfo myTimer)
        {
            _logger.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
            
            if (myTimer.ScheduleStatus is not null)
            {
                _logger.LogInformation($"Next timer schedule at: {myTimer.ScheduleStatus.Next}");
            }


        }
    }
}
