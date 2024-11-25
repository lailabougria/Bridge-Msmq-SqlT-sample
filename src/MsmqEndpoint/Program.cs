using System;
using System.Linq;
using System.Threading.Tasks;
using NServiceBus;

class Program
{
    static async Task Main()
    {
        Console.Title = "MsmqEndpoint";

        const string letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        var random = new Random();

        var endpointConfiguration = new EndpointConfiguration("Samples.MessagingBridge.MsmqEndpoint");
        endpointConfiguration.EnableInstallers();
        endpointConfiguration.SendFailedMessagesTo("error");
        endpointConfiguration.UsePersistence<NonDurablePersistence>();

        endpointConfiguration.UseSerialization<SystemJsonSerializer>();
        var routingConfig = endpointConfiguration.UseTransport(new MsmqTransport());
        routingConfig.RegisterPublisher(typeof(OtherEvent), "Samples.MessagingBridge.MsmqEndpoint");

        var metrics = endpointConfiguration.EnableMetrics();
        endpointConfiguration.Recoverability().Delayed(settings => settings.NumberOfRetries(0));
        endpointConfiguration.Recoverability().Immediate(settings => settings.NumberOfRetries(0));

        metrics.SendMetricDataToServiceControl(
            serviceControlMetricsAddress: "Particular.Monitoring.BridgeSampleMSMQ",
            interval: TimeSpan.FromSeconds(5));

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
            await endpointInstance.SendLocal(new MyOtherCommand() { Property = prop });
            Console.WriteLine($"\nCommand with value '{prop}' sent");
        }

        await endpointInstance.Stop();
    }
}
