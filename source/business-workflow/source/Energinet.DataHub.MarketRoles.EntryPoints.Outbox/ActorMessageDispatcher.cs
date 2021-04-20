using System;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Energinet.DataHub.MarketRoles.EntryPoints.Outbox
{
    public static class ActorMessageDispatcher
    {
        [Function("ActorMessageDispatcher")]
        public static void Run(
            [TimerTrigger("%ACTOR_MESSAGE_DISPATCH_TRIGGER_TIMER%")] TimerInfo timer,
            FunctionContext context)
        {
            var logger = context.GetLogger("ActorMessageDispatcher");
            logger.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
            logger.LogInformation($"Next timer schedule at: {timer.ScheduleStatus.Next}");
        }
    }

    #pragma warning disable SA1402 // Used only for demo usage
    public class TimerInfo
    {
        public MyScheduleStatus ScheduleStatus { get; set; }

        public bool IsPastDue { get; set; }
    }

    public class MyScheduleStatus
    {
        public DateTime Last { get; set; }

        public DateTime Next { get; set; }

        public DateTime LastUpdated { get; set; }
    }
    #pragma warning restore SA1402
}
