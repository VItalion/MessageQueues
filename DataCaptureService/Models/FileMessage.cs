namespace DataCaptureService.Models {
    internal class FileMessage {
        public Guid Id { get; set; }
        
        public string Name { get; set; }

        public int Count { get; set; }

        public int TotalCount { get; set; }

        public byte[] Bytes { get; set; }
    }
}
