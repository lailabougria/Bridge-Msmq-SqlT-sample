using System;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Logging;

public class MyOtherCommandHandler : IHandleMessages<MyOtherCommand>
{
    static ILog log = LogManager.GetLogger<MyOtherCommandHandler>();
    static Random random = new();
    
    public Task Handle(MyOtherCommand message, IMessageHandlerContext context)
    {
        log.Info($"Received MyOtherCommand: {message.Property}");
        
        if(random.Next(0, 3) == 0)
        {
            throw new Exception("Simulated exception");
        }

        return context.Publish<OtherEvent>(@event => { @event.Property = "other event from MSMQ endpoint"; });
    }
}