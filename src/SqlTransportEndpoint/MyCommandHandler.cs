using System;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Logging;

public class MyCommandHandler : IHandleMessages<MyCommand>
{
    static ILog log = LogManager.GetLogger<MyCommandHandler>();
    static Random random = new();
    
    public Task Handle(MyCommand message, IMessageHandlerContext context)
    {
        log.Info($"Received MyCommand: {message.Property}");

       
        if (random.Next(0, 2) == 0)
        {
            throw new Exception("Simulated exception");
        }

        return context.Publish<MyEvent>(@event => { @event.Property = "event from MSMQ endpoint"; });
    }
}