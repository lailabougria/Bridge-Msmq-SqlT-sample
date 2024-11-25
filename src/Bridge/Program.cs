using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using NServiceBus;

class Program
{
    static async Task Main()
    {
        Console.Title = "Bridge";
        var sqlConnectionString = "insert-connection-string";
        var host = CreateHostBuilder(sqlConnectionString).Build();
        await host.RunAsync();
    }

    static IHostBuilder CreateHostBuilder(string connString)
    {
        return Host.CreateDefaultBuilder()
            .UseConsoleLifetime()
            .UseNServiceBusBridge((ctx, bridgeConfiguration) =>
            {
                var sqlEndpoint = new BridgeEndpoint("Samples.MessagingBridge.SqlTransportEndpoint");
                var sqlBridgeTransport = new BridgeTransport(new SqlServerTransport(connString));
                sqlBridgeTransport.AutoCreateQueues = true;
                sqlBridgeTransport.HasEndpoint(sqlEndpoint);
                bridgeConfiguration.AddTransport(sqlBridgeTransport);

                // Everything on the SQL-T side must be mapped to the bridge
                // so that retries from ServicePulse can work (they will re-shoveled to SQL-T)

                var msmqServiceControlEndPoint = "particular.servicecontrol"; 
                var msmqErrorEndPoint = "error";

                var msmqTransport = new BridgeTransport(new MsmqTransport())
                {
                    ErrorQueue = "bridge.error", // this is the error queue for the shoveling mechanism
                    AutoCreateQueues = true
                };

                msmqTransport.HasEndpoint("particular.servicecontrol"); // heartbeats and custom checks
                msmqTransport.HasEndpoint("error"); // error messages
                msmqTransport.HasEndpoint("particular.monitoring"); // metrics
                
                bridgeConfiguration.AddTransport(msmqTransport);
                bridgeConfiguration.TranslateReplyToAddressForFailedMessages();
                bridgeConfiguration.RunInReceiveOnlyTransactionMode();

            });
    }
}