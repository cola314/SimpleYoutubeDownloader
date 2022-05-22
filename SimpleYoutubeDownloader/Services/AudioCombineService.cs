using System.Diagnostics;

namespace SimpleYoutubeDownloader.Services
{
    public class AudioCombineService
    {
        private readonly Logger _logger;

        public AudioCombineService(Logger logger)
        {
            _logger = logger;
        }

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

            _logger.WriteLine("Finish combine audio and video");
        }

        private void ProcessDataReceived(object sender, DataReceivedEventArgs e)
        {
            _logger.WriteLine(e.Data);
        }
    }
}