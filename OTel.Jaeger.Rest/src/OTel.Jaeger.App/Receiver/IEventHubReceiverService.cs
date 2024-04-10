using OTel.Jaeger.App.Model;

namespace OTel.Jaeger.App.Receiver
{
    public interface IEventHubReceiverService
    {
        Task<OTelEventData> ReceiveLatestAsync();
    }
}