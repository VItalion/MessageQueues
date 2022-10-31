namespace Client.Tools {
    internal class FileSaver {
        public void Save(string path, byte[] bytes) {
            try {
                var mode = File.Exists(path) ? FileMode.Append : FileMode.Create;
                using (var f = File.Open(path, mode)) {
                    f.Write(bytes);
                }
            } catch (Exception ex) {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}
