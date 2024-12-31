using System;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SimpleYoutubeDownloader.Properties;

namespace SimpleYoutubeDownloader.Services
{
    public class YtdlVideoDownloader : VideoDownloader
    {
        private readonly BehaviorSubject<int> _progressSubject;

        private readonly BehaviorSubject<string> _statusSubject;

        private readonly Logger _logger;

        public YtdlVideoDownloader(Logger logger)
        {
            _logger = logger;
            _statusSubject = new BehaviorSubject<string>(string.Empty);
            _progressSubject = new BehaviorSubject<int>(0);
        }

        public IObservable<string> Status => _statusSubject.AsObservable();
        public IObservable<int> Progress => _progressSubject.AsObservable();

        public Task DownloadVideoAsync(string url, string downloadFile)
        {
            SetStatus(Resources.DOWNLOAD_START);
            _logger.WriteLine("Start Download");

            _logger.WriteLine($"Download Url : {url}");
            _logger.WriteLine($"Download Path : {downloadFile}");

            // search video
            SetStatus(Resources.SEARCHING_VIDEO);

            return Task.Run(() =>
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    FileName = "yt-dlp.exe",
                    Arguments = $"-f \"bv+ba\" -o {downloadFile} {url}",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };
                _logger.WriteLine($"{startInfo.FileName} {startInfo.Arguments}");

                using (Process process = new Process { StartInfo = startInfo })
                {
                    process.OutputDataReceived += ProcessDataReceived;
                    process.ErrorDataReceived += ProcessDataReceived;
                    process.Start();
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();
                    process.WaitForExit();
                    process.OutputDataReceived -= ProcessDataReceived;
                    process.ErrorDataReceived -= ProcessDataReceived;
                }
                SetStatus(Resources.DOWNLOAD_COMPLATE);
            });
        }

        private static readonly Regex ProgressLineRegex = new Regex(@"(\d+(\.\d+)?)%");


        private void ProcessDataReceived(object sender, DataReceivedEventArgs e)
        {
            string line = e.Data ?? string.Empty;
            _logger.WriteLine(line);

            Match match = ProgressLineRegex.Match(line);
            if (match.Success)
            {
                if (float.TryParse(match.Groups[1].Value, out var progress))
                {
                    SetProgress((int)progress);
                    SetStatus(string.Format(Resources.DOWNLOAD_PROGRESS_MESSAGE, (int)progress));
                }
            }
        }

        private void SetStatus(string message)
        {
            _statusSubject.OnNext(message);
        }

        private void SetProgress(int value)
        {
            _progressSubject.OnNext(value);
        }
    }
}