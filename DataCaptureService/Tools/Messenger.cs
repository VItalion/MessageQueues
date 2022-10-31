using DataCaptureService.Models;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System.Text;

namespace DataCaptureService.Tools {
    internal class Messenger {
        private readonly ConnectionFactory _factory;

        //max message size is 512 mb but I think use so big message is bad idea
        //proof https://github.com/rabbitmq/rabbitmq-server/issues/147
        private const int BufferSize = 8 * 1024 * 1024;     //8 Mb
        private const int MaxShortWaitAttemptCount = 5;     //for documents
        private const int MaxLongWaitAttemptCount = 10;     //for large files

        private int _attemptCount = 0;

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
                Console.WriteLine(ex.ToString());
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
                        var size = currentSendedSize + BufferSize < s.Length ? BufferSize : s.Length - currentSendedSize;
                        var bytes = new byte[size];
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
                _attemptCount = 0;
            } catch (IOException ex) {
                //FileWatcher raise file created event when coping begin and sometimes file explorer does not have time to unlock copied file
                if (_attemptCount == MaxLongWaitAttemptCount) {
                    Console.WriteLine(ex.ToString());
                    return;
                } else if (_attemptCount >= MaxShortWaitAttemptCount) {
                    Thread.Sleep(TimeSpan.FromMinutes(1));
                    _attemptCount++;
                    SendLargeFile(file);
                } else {
                    Thread.Sleep(1000);
                    _attemptCount++;
                    SendLargeFile(file);
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
