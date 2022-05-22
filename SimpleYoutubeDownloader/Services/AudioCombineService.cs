using System;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text.RegularExpressions;

namespace SimpleYoutubeDownloader.Services
{
    public class AudioCombineService
    {
        private readonly Logger _logger;

        public AudioCombineService(Logger logger)
        {
            _logger = logger;
            _progressSubject = new BehaviorSubject<int>(0);
        }

        private readonly BehaviorSubject<int> _progressSubject;
        public IObservable<int> Progress => _progressSubject.AsObservable();

        private TimeSpan _totalTime;

        public void CombineAudioToVideo(string audioFile, string videoFile, string targetFile)
        {
            _logger.WriteLine("Start combine audio and video");

            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.UseShellExecute = false;
            startInfo.CreateNoWindow = true;
            startInfo.FileName = "ffmpeg.exe";
            startInfo.Arguments = $"-y -i {videoFile} -i {audioFile} -c:v copy -c:a aac \"{targetFile}\"";
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
            _logger.WriteLine($"{startInfo.FileName} {startInfo.Arguments}");

            using (Process process = new Process())
            {
                process.StartInfo = startInfo;
                process.OutputDataReceived += ProcessDataReceived;
                process.ErrorDataReceived += ProcessDataReceived;
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.WaitForExit();
            }

            _progressSubject.Dispose();
            _logger.WriteLine("Finish combine audio and video");
        }

        private static readonly Regex ProgressLineRegex = new Regex(
            "(frame=[ ]*[0-9]+ fps=[0-9.]+ q=[0-9-.]+ size=[ ]*[0-9A-Za-z]+ time=[0-9]{2}:[0-9]{2}:[0-9]{2}.[0-9]{2} bitrate=[0-9]+)");

        private static readonly Regex DurationLineRegex =
            new Regex("(Duration: [0-9]{2}:[0-9]{2}:[0-9]{2}.[0-9]{2}, start: [0-9.]+, bitrate: [0-9]+)");

        private static readonly Regex TimeSpanRegex = new Regex("([0-9]{2}:[0-9]{2}:[0-9]{2}.[0-9]{2})");

        private void ProcessDataReceived(object sender, DataReceivedEventArgs e)
        {
            string line = e.Data ?? string.Empty;
            _logger.WriteLine(line);


            if (DurationLineRegex.IsMatch(line) && TimeSpanRegex.IsMatch(line))
            {
                var match = TimeSpanRegex.Match(line).Value;
                if (TimeSpan.TryParse(match, out var duration))
                {
                    _totalTime = duration;
                }
            }

            if (ProgressLineRegex.IsMatch(line) && TimeSpanRegex.IsMatch(line))
            {
                var match = TimeSpanRegex.Match(line).Value;
                if (TimeSpan.TryParse(match, out var current))
                {
                    if (_totalTime.Ticks == 0)
                        return;

                    var progress = 100 * current.Ticks / _totalTime.Ticks;
                    if (progress < 0)
                        progress = 0;
                    if (progress > 100)
                        progress = 100;

                    _progressSubject.OnNext((int) progress);
                }
            }
        }
    }
}