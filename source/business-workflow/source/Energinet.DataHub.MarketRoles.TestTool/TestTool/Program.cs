using System;
using System.Text;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;

namespace TestTool
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            var connectionString = "Endpoint=sb://sbn-marketroles-endk-u.servicebus.windows.net/;SharedAccessKeyName=sbnar-marketroles-sender;SharedAccessKey=iiKzbA0xI60YeZXU/T8uZ5a32rwp9f0Flera0FAQaoM=";
            var topicName = "sbq-marketroles";

            // create a Service Bus client
            await using (ServiceBusClient client = new ServiceBusClient(connectionString))
            {
                // create a sender for the topic
                ServiceBusSender sender = client.CreateSender(topicName);
                var content = "Hello, impl! " + DateTime.Now;
                var bytes = Encoding.UTF8.GetBytes(content);
                await sender.SendMessageAsync(new ServiceBusMessage(bytes));
                Console.WriteLine($"Sent a single message to the topic: {topicName}");
            }
        }
    }
}
