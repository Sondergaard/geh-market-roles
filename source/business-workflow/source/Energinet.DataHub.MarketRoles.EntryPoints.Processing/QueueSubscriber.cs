using System;
using System.Text;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Energinet.DataHub.MarketRoles.EntryPoints.Processing
{
    public static class QueueSubscriber
    {
        [Function("QueueSubscriber")]
        public static string Run(
            [ServiceBusTrigger("sbq-marketroles", Connection = "ServiceBusConnection")] byte[] item,
            FunctionContext context)
        {
            var logger = context.GetLogger("ServiceBusFunction");

            var content = Encoding.UTF8.GetString(item);

            logger.LogInformation(content);

            var message = $"Output message created at {DateTime.Now}";
            return message;
        }
    }
}
