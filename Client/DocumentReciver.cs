using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client {
    internal class DocumentReciver : IDisposable {
        private readonly ConnectionFactory _factory;

        public DocumentReciver(string uri) {
            _factory = new ConnectionFactory();
            _factory.Uri = new Uri(uri);
        }

        public void Dispose() {
        }
    }
}
