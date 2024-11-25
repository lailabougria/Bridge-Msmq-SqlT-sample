using System;
using System.Linq;
using System.Threading.Tasks;
using NServiceBus;

class Program
{
    static async Task Main()
    {
        const string letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        var random = new Random();

        Console.Title = "SqlTransportEndpoint";
        var endpointConfiguration = new EndpointConfiguration("Samples.MessagingBridge.SqlTransportEndpoint");
        endpointConfiguration.EnableInstallers();
        endpointConfiguration.UsePersistence<NonDurablePersistence>();

        var connectionString = "insert-connection-string-here";
       
        endpointConfiguration.UseSerialization<SystemJsonSerializer>();
        endpointConfiguration.UseTransport(new SqlServerTransport(connectionString));
       var metrics = endpointConfiguration.EnableMetrics();

        metrics.SendMetricDataToServiceControl(
            serviceControlMetricsAddress: "Particular.Monitoring.BridgeSample",
            interval: TimeSpan.FromSeconds(5));

        endpointConfiguration.Recoverability().Delayed(settings => settings.NumberOfRetries(0));
        endpointConfiguration.Recoverability().Immediate(settings => settings.NumberOfRetries(0));

        var endpointInstance = await Endpoint.Start(endpointConfiguration);

        Console.WriteLine("Press Enter to send a command");
        Console.WriteLine("Press any other key to exit");

        while (true)
        {
            var key = Console.ReadKey().Key;
            if (key != ConsoleKey.Enter)
            {
                break;
            }

            var prop = new string(Enumerable.Range(0, 3).Select(i => letters[random.Next(letters.Length)]).ToArray());
            await endpointInstance.SendLocal(new MyCommand { Property = prop });
            Console.WriteLine($"\nCommand with value '{prop}' sent");
        }

        await endpointInstance.Stop();
    }
}
