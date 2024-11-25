using NServiceBus;

public class MyOtherCommand : IMessage
{
    public string Property { get; set; }
}