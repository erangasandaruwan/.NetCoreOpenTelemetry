namespace OTel.Jaeger.App.Sender
{
    public interface IEventHubSenderService
    {
        Task<bool> SendDataToPartitionAsync<T>(T data);
    }
}