using System;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Energinet.DataHub.MarketRoles.EntryPoints.Outbox
{
    public static class ActorMessageDispatcher
    {
        [Function("ActorMessageDispatcher")]
        public static void Run(
            [TimerTrigger("0 */5 * * * *")] string timer,
            FunctionContext context)
        {
            var logger = context.GetLogger("ActorMessageDispatcher");
            logger.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
            logger.LogInformation($"Next timer schedule at: {timer}");
        }
    }
}
