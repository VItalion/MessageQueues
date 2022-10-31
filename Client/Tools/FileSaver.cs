using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Tools {
    internal class FileSaver {
        public void Save(string path, byte[] bytes) {
            using (var f = File.OpenWrite(path)) {
                var length = f.Length;
                f.Write(bytes);
            }
        }
    }
}
