using System;
using System.Threading.Tasks;

namespace SimpleYoutubeDownloader.Services
{
    public interface VideoDownloader
    {
        IObservable<int> Progress { get; }
        IObservable<string> Status { get; }

        Task DownloadVideoAsync(string url, string downloadFile);
    }
}