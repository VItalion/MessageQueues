using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Models {
    internal class FileMessage {
        public Guid Id { get; set; }
        
        public string Name { get; set; }

        public int Size { get; set; }

        public int Count { get; set; }

        public int TotalCount { get; set; }
     
        public byte[] Bytes { get; set; }
    }
}
