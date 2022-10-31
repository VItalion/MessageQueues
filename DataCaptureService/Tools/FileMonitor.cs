using DataCaptureService.Models;

namespace DataCaptureService.Tools {
    internal class FileMonitor : IDisposable {
        private readonly FileSystemWatcher _watcher;
        private readonly IEnumerable<string> _supportedExtensions;

        public event Action<FileModel> FileCreated;

        public FileMonitor(string sourceFolderPath, IEnumerable<string> supportedExtensions) {
            _supportedExtensions = supportedExtensions.Select(x => x.ToLowerInvariant());
            _watcher = new FileSystemWatcher();
            _watcher.Path = sourceFolderPath;
            _watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName;
            _watcher.EnableRaisingEvents = true;
            _watcher.Created += OnFileCreated;
        }

        private void OnFileCreated(object sender, FileSystemEventArgs e) {
            if (!_supportedExtensions.Contains(Path.GetExtension(e.FullPath).ToLowerInvariant()))       //not provide unsupported files
                return;

            FileCreated?.Invoke(new FileModel {
                Path = e.FullPath,
                Name = e.Name,
                Extension = Path.GetExtension(e.FullPath)
            });
        }

        public void Dispose() {
            _watcher.Created -= OnFileCreated;
            _watcher?.Dispose();
        }
    }
}
