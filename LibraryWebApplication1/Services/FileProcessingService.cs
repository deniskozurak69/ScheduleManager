using Azure.Messaging.ServiceBus;
using System.Threading.Tasks;
namespace LibraryWebApplication1.Services
{
    public class FileProcessingService
    {
        private readonly string _queueName = "queue";
        private readonly ServiceBusClient _serviceBusClient;
        public FileProcessingService(ServiceBusClient serviceBusClient)
        {
            _serviceBusClient = serviceBusClient;
        }
        public async Task SendFileProcessingMessageAsync(string filePath)
        {
            var sender = _serviceBusClient.CreateSender(_queueName);
            var message = new ServiceBusMessage(filePath);
            await sender.SendMessageAsync(message);
        }
    }
}
