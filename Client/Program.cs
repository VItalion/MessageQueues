using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System.Text;
using Newtonsoft.Json;
using Client.Models;
using Client.Tools;

var factory = new ConnectionFactory() { HostName = "localhost" };
using (var connection = factory.CreateConnection())
using (var channel = connection.CreateModel()) {
    channel.ExchangeDeclare(exchange: "dataCaptureExchange", type: ExchangeType.Topic);
    var queueName = channel.QueueDeclare().QueueName;

    channel.QueueBind(queue: queueName,
                      exchange: "dataCaptureExchange",
                      routingKey: "capture.document");

    Console.WriteLine(" [*] Waiting for messages. To exit press CTRL+C");

    var consumer = new EventingBasicConsumer(channel);
    consumer.Received += (model, ea) => {
        var body = ea.Body.ToArray();
        var json = Encoding.UTF8.GetString(body);
        var message = JsonConvert.DeserializeObject<FileMessage>(json);
        var routingKey = ea.RoutingKey;

        Console.WriteLine($" [x] Received '{routingKey}':'{message.Name} part {message.Count} of {message.TotalCount}'");

        var targetFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
        targetFolder = Path.Combine(targetFolder, "RabbitMqClient");
        if (!Directory.Exists(targetFolder))
            Directory.CreateDirectory(targetFolder);

        var saver = new FileSaver();
        saver.Save(Path.Combine(targetFolder, message.Name), message.Bytes);
    };
    channel.BasicConsume(queue: queueName,
                         autoAck: true,
                         consumer: consumer);

    Console.WriteLine(" Press [enter] to exit.");
    Console.ReadLine();
}