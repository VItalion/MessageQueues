using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System.Text;
using Newtonsoft.Json;
using Client.Models;
using Client.Tools;

var rabbitMqUri = args.Length > 0 ? args[0] : "amqp://guest:guest@localhost:5672";
var targetFolder = args.Length > 1 ? args[1] : Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);

var factory = new ConnectionFactory();
factory.Uri = new Uri(rabbitMqUri);

using (var connection = factory.CreateConnection())
using (var channel = connection.CreateModel()) {
    channel.ExchangeDeclare(exchange: "dataCaptureExchange", type: ExchangeType.Topic);
    var queueName = channel.QueueDeclare().QueueName;

    channel.QueueBind(queue: queueName,
                      exchange: "dataCaptureExchange",
                      routingKey: "capture.document");

    Console.WriteLine("Waiting for messages...");

    var consumer = new EventingBasicConsumer(channel);
    consumer.Received += (model, ea) => {
        var body = ea.Body.ToArray();
        var json = Encoding.UTF8.GetString(body);
        var message = JsonConvert.DeserializeObject<FileMessage>(json);
        var routingKey = ea.RoutingKey;

        Console.WriteLine($"Received '{routingKey}':'{message.Name} part {message.Count} of {message.TotalCount}'");

        targetFolder = Path.Combine(targetFolder, "RabbitMqClient");
        if (!Directory.Exists(targetFolder))
            Directory.CreateDirectory(targetFolder);

        //TODO: mb worth saving file in another thread
        var saver = new FileSaver();
        saver.Save(Path.Combine(targetFolder, message.Name), message.Bytes);
    };
    channel.BasicConsume(queue: queueName,
                         autoAck: true,
                         consumer: consumer);

    Console.WriteLine("Press Enter to close application.");
    Console.ReadLine();
}