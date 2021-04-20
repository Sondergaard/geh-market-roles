using System;
using Microsoft.Extensions.Hosting;

[assembly: CLSCompliant(false)]

namespace Energinet.DataHub.MarketRoles.EntryPoints.Ingestion
{
    public static class Program
    {
        public static void Main()
        {
            var host = new HostBuilder()
                .ConfigureFunctionsWorkerDefaults()
                .Build();

            host.Run();
        }
    }
}
