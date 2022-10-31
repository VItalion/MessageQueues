using DataCaptureService.Models;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System.Text;

namespace DataCaptureService.Tools {
    internal class Messenger {
        private readonly ConnectionFactory _factory;

        //max message size is 512 mb but I think use so big message is bad idea
        //proof https://github.com/rabbitmq/rabbitmq-server/issues/147
        private const int BufferSize = 8 * 1024 * 1024;     //8 mb

        public Messenger(string uri) {
            _factory = new ConnectionFactory();
            _factory.Uri = new Uri(uri);
        }

        public void Send(FileModel file) {
            var connection = _factory.CreateConnection();
            var channel = connection.CreateModel();

            try {
                channel.ExchangeDeclare("dataCaptureExchange", ExchangeType.Topic);
                var bytes = File.ReadAllBytes(file.Path);
                channel.BasicPublish(exchange: "dataCaptureExchange",
                    routingKey: "capture.document",
                    basicProperties: null,
                    body: bytes);
            } catch (Exception ex) {
                //log ewxception
            } finally {
                channel.Close();
                connection.Close();
            }
        }

        public void SendLargeFile(FileModel file) {
            var connection = _factory.CreateConnection();
            var channel = connection.CreateModel();

            try {
                channel.ExchangeDeclare("dataCaptureExchange", ExchangeType.Topic);

                var fileId = Guid.NewGuid();
                using (var s = File.OpenRead(file.Path)) {
                    var totalMessagesCount = (int)Math.Round((double)s.Length / BufferSize, MidpointRounding.ToPositiveInfinity);
                    var count = 0;
                    var currentSendedSize = 0;
                    while (currentSendedSize < s.Length) {
                        var bytes = new byte[BufferSize];
                        s.Read(bytes, 0, bytes.Length);
                        currentSendedSize += bytes.Length;
                        count++;

                        var message = new FileMessage {
                            Id = fileId,
                            Name = file.Name,
                            Count = count,
                            TotalCount = totalMessagesCount,
                            Bytes = bytes
                        };
                        SendMessage(channel, message);
                    }
                }

            } catch (Exception ex) {
                //log ewxception
                Console.WriteLine(ex.ToString());
            } finally {
                channel.Close();
                connection.Close();
            }
        }

        private void SendMessage(IModel channel, FileMessage message) {
            var bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));
            channel.BasicPublish(exchange: "dataCaptureExchange",
                    routingKey: "capture.document",
                    basicProperties: null,
                    body: bytes);
        }
    }
}
