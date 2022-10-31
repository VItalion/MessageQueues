using DataCaptureService.Tools;

var _dataSource = args.Length > 0 ? args[0] : Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
var _rabbitMqUri = args.Length > 1 ? args[1] : "amqp://guest:guest@localhost:5672";


using (var monitor = new FileMonitor(_dataSource, new string[] { ".pdf", ".docx", ".doc", ".png", ".mp4" })) {      //.mp4 for testing ability send large files
    monitor.FileCreated += OnFileCreated;

    Console.WriteLine($"Begin monitoring {_dataSource}...");
    Console.WriteLine("Press Enter to stop monitoring and close application");
    Console.ReadLine();
}

void OnFileCreated(DataCaptureService.Models.FileModel file) {
    var messenger = new Messenger(_rabbitMqUri);
    messenger.SendLargeFile(file);
}