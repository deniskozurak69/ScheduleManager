using Azure.Messaging.ServiceBus;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;
namespace LibraryWebApplication1.Services
{
    public class FileProcessingBackgroundService : BackgroundService
    {
        private readonly ServiceBusProcessor _processor;
        private readonly ILogger<FileProcessingBackgroundService> _logger;
        private readonly ImageProcessingService _imageProcessingService;
        private readonly string _uploadsFolder;
        public FileProcessingBackgroundService(ServiceBusClient serviceBusClient,
                                               string queueName,
                                               ILogger<FileProcessingBackgroundService> logger,
                                               ImageProcessingService imageProcessingService, IWebHostEnvironment webHostEnvironment)
        {
            queueName = "queue";
            _processor = serviceBusClient.CreateProcessor(queueName, new ServiceBusProcessorOptions());
            _logger = logger;
            _imageProcessingService = imageProcessingService; 
            _uploadsFolder = Path.Combine(webHostEnvironment.WebRootPath, "uploads");
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _processor.ProcessMessageAsync += async args =>
            {
                string filePath = args.Message.Body.ToString();              
                _logger.LogInformation($"Processing file: {filePath}");
                await _imageProcessingService.CompressAndConvertImageAsync(filePath);
                await args.CompleteMessageAsync(args.Message);
            };
            _processor.ProcessErrorAsync += args =>
            {
                _logger.LogError(args.Exception.ToString());
                return Task.CompletedTask;
            };
            await _processor.StartProcessingAsync(stoppingToken);
        }
    }
}
