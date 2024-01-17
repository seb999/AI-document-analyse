using System.Text.Json;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.SignalR;

namespace EwrsDocAnalyses.Misc
{
    public class FolderWatcherService
    {
        private readonly IHubContext<SignalRHub> _hub;
        private readonly FileSystemWatcher watcher;
        private readonly string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Downloads";
       
        public FolderWatcherService(IHubContext<SignalRHub> hub)
        {
            _hub = hub;
            watcher = new FileSystemWatcher(folderPath)
            {
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.CreationTime | NotifyFilters.Size | NotifyFilters.Attributes,
                Filter = "*.docx"
            };

            watcher.Created += OnFileCreated;
            watcher.Changed += OnFileCreated;
            watcher.Renamed += OnFileCreated;
            watcher.EnableRaisingEvents = true;
        }

        private void OnFileCreated(object sender, FileSystemEventArgs e)
        {
            string first10Lines = Helper.ExtractDocx(e.FullPath);
            _hub.Clients.All.SendAsync("newFile", JsonSerializer.Serialize(first10Lines));
        }

        public void StartWatching()
        {
            watcher.EnableRaisingEvents = true;
        }

        public void StopWatching()
        {
            watcher.EnableRaisingEvents = false;
        }
    }
}