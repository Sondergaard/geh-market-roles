using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace Energinet.DataHub.MarketRoles.EntryPoints.Ingestion
{
    public static class CommandApi
    {
        [Function("CommandApi")]
        public static async Task<HttpResponseData> RunAsync(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData request,
            FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger("CommandApi");
            logger.LogInformation("C# HTTP trigger function processed a request.");

            var response = request.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

            var connectionString = Environment.GetEnvironmentVariable("MARKET_DATA_QUEUE_CONNECTION_STRING");
            var topicName = Environment.GetEnvironmentVariable("MARKET_DATA_QUEUE_TOPIC_NAME");

            // create a Service Bus client
            await using (ServiceBusClient client = new ServiceBusClient(connectionString))
            {
                // create a sender for the topic
                var sender = client.CreateSender(topicName);
                var content = "Welcome to Azure Functions! " + DateTime.Now;
                var bytes = Encoding.UTF8.GetBytes(content);
                await sender.SendMessageAsync(new ServiceBusMessage(bytes)).ConfigureAwait(false);
                Console.WriteLine($"Sent a single message to the topic: {topicName}");
            }

            return response;
        }
    }
}
