
using DataCaptureService.Tools;

const string RabbitMqUri = "amqp://guest:guest@localhost:5672";
const string DataSource = "D:/rabbit_test";
using (var monitor = new FileMonitor(DataSource, new string[] { ".png", ".jpg", ".mp4" })) {
    monitor.FileCreated += OnFileCreated;

    Console.WriteLine($"Begin monitoring {DataSource}...");
    Console.WriteLine("Press Enter to stop monitoring and close application");
    Console.ReadLine();
}

void OnFileCreated(DataCaptureService.Models.FileModel file) {
    var messenger = new Messenger(RabbitMqUri);
    messenger.SendLargeFile(file);
}