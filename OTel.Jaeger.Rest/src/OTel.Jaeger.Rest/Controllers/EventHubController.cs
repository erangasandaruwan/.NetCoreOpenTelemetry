using Microsoft.AspNetCore.Mvc;
using OTel.Jaeger.App.Model;
using OTel.Jaeger.App.Receiver;
using OTel.Jaeger.App.Sender;

namespace OTel.Jaeger.Rest.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventHubController : ControllerBase
    {
        private readonly IEventHubReceiverService _eventHubReceiverService;
        private readonly IEventHubSenderService _eventHubSenderService;
        public EventHubController(IEventHubReceiverService eventHubReceiverService, IEventHubSenderService eventHubSenderService)
        {
            _eventHubReceiverService = eventHubReceiverService;
            _eventHubSenderService = eventHubSenderService;
        }

        [HttpGet]
        [Route("GetLatestEvent")]
        public async Task<OTelEventData> GetLatestEvent()
        {
            return await _eventHubReceiverService.ReceiveLatestAsync();
        }

        [HttpPost]
        [Route("SendEvent")]
        public async Task SendEvent(OTelEventData oTelEventData)
        {
            await _eventHubSenderService.SendDataToPartitionAsync(oTelEventData);
        }
    }
}
