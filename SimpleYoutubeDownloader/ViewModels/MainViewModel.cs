using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using YoutubeExplode;

namespace SimpleYoutubeDownloader.ViewModels
{
    public class MainViewModel : ReactiveObject
    {
        private const string VIDEO_FILE = "video.mp4";
        private const string AUDIO_FILE = "audio.mp4";
        private readonly Logger _logger = Logger.Instance;

        public MainViewModel()
        {
            StatusText = "진행상황";
            DownloadCommand = ReactiveCommand.Create(() => DownloadVideo());
        }

        private string _statusText;
        public string StatusText
        {
            get => _statusText;
            set => this.RaiseAndSetIfChanged(ref _statusText, value);
        }

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set => this.RaiseAndSetIfChanged(ref _searchText, value);
        }

        private int _progress;
        public int Progress
        {
            get => _progress;
            set => this.RaiseAndSetIfChanged(ref _progress, value);
        }

        private bool _downloadEnable;
        public bool DownloadEnable
        {
            get => _downloadEnable;
            set => this.RaiseAndSetIfChanged(ref _downloadEnable, value);
        }

        public ReactiveCommand<Unit, Unit> DownloadCommand { get; }

        private void DownloadVideo()
        {
            if (this.DownloadEnable)
            {
                return;
            }

            this.DownloadEnable = true;
            this.StatusText = "다운로드 시작";
            _logger.WriteLine("Start Download");
            var saveDialog = new SaveFileDialog();
            saveDialog.DefaultExt = ".mp4";
            saveDialog.Filter = "MPEG-4 Video|*.mp4";

            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                string downloadPath = SearchText;
                string targetFileName = saveDialog.FileName;

                _logger.WriteLine($"Download Path : {downloadPath}");
                _logger.WriteLine($"Target File : {targetFileName}");

                Task.Run(async () =>
                {
                    try
                    {
                        // search video
                        this.StatusText = "비디오 검색 중";
                        _logger.WriteLine($"Search Video Info");
                        _logger.WriteLine($"URL : \"{downloadPath}\"\nSaveFile : \"{targetFileName}\"");

                        var youtube = new YoutubeClient();
                        var video = await youtube.Videos.GetAsync(downloadPath);
                        _logger.WriteLine($"id : {video.Id}\ntitle : {video.Title}\nAuthor : {video.Author}");

                        var streamManifest = await youtube.Videos.Streams.GetManifestAsync(video.Id);
                        _logger.WriteLine($"{streamManifest.Streams.Count} numbers of stream found");

                        var audioStreamInfo = streamManifest.GetAudioOnlyStreams()
                            .Where(x => x.Container.Name == "mp4")
                            .OrderByDescending(x => x.Bitrate.BitsPerSecond)
                            .FirstOrDefault();

                        var videoStreamInfo = streamManifest.GetVideoOnlyStreams()
                            .Where(x => x.Container.Name == "mp4")
                            .OrderByDescending(x => x.VideoQuality.MaxHeight)
                            .FirstOrDefault();

                        if (audioStreamInfo == null)
                        {
                            _logger.WriteLine("Audio stream info not found");
                            throw new Exception("다운로드 가능한 오디오가 없습니다");
                        }

                        if (videoStreamInfo == null)
                        {
                            _logger.WriteLine("Video stream info not found");
                            throw new Exception("다운로드 가능한 비디오가 없습니다");
                        }

                        // download video
                        _logger.WriteLine("Start download audio and video from server");

                        var videoDownload = Observable.Create<double>(async observer =>
                            {
                                await youtube.Videos.Streams.DownloadAsync(videoStreamInfo, VIDEO_FILE,
                                    new Progress<double>(observer.OnNext));
                                observer.OnCompleted();
                                return Disposable.Empty;
                            })
                            .Select(x => (int)(x * 100))
                            .Distinct(progress => progress / 5)
                            .Publish();

                        var audioDownload = Observable.Create<double>(async observer =>
                            {
                                await youtube.Videos.Streams.DownloadAsync(audioStreamInfo, AUDIO_FILE,
                                    new Progress<double>(observer.OnNext));
                                observer.OnCompleted();
                                return Disposable.Empty;
                            })
                            .Select(x => (int)(x * 100))
                            .Distinct(progress => progress / 5)
                            .Publish();

                        videoDownload
                            .Subscribe(progress => _logger.WriteLine($"Video download progress : {progress}%"));

                        audioDownload
                            .Subscribe(progress => _logger.WriteLine($"Audio download progress : {progress}%"));

                        audioDownload.CombineLatest(videoDownload, (x, y) => (x + y) / 2)
                            .ObserveOn(RxApp.MainThreadScheduler)
                            .Subscribe(totalProgress =>
                            {
                                this.Progress = totalProgress;
                                this.StatusText = $"다운로드 중 {totalProgress}% 완료";
                            });

                        videoDownload.Connect();
                        audioDownload.Connect();
                        
                        await Task.WhenAll(videoDownload.ToTask(), audioDownload.ToTask());
                        _logger.WriteLine("Finish download audio and video from server");

                        // combine audio and video
                        _logger.WriteLine("Start combine audio and video");
                        this.StatusText = "mp4 변환 작업 중";

                        ProcessStartInfo startInfo = new ProcessStartInfo();
                        startInfo.UseShellExecute = false;
                        startInfo.CreateNoWindow = true;
                        startInfo.FileName = "ffmpeg.exe";
                        startInfo.Arguments = $"-i {VIDEO_FILE} -i {AUDIO_FILE} -c:v copy -c:a aac \"{targetFileName}\"";
                        startInfo.RedirectStandardOutput = true;
                        startInfo.RedirectStandardError = true;
                        _logger.WriteLine($"{startInfo.FileName} {startInfo.Arguments}");

                        using (Process process = new Process())
                        {
                            process.StartInfo = startInfo;
                            process.OutputDataReceived += Process_DataReceived;
                            process.ErrorDataReceived += Process_DataReceived;
                            process.Start();
                            process.BeginOutputReadLine();
                            process.BeginErrorReadLine();
                            process.WaitForExit();
                        }

                        _logger.WriteLine("Finish combine audio and video");

                        this.Progress = 100;
                        this.StatusText = "다운로드 완료";
                        MessageBox.Show("다운로드 완료", "정보", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        Process.Start("explorer.exe", Path.GetDirectoryName(targetFileName));

                        // clean up
                        _logger.WriteLine("Clean temp audio and video file");
                        File.Delete(VIDEO_FILE);
                        File.Delete(AUDIO_FILE);
                        _logger.WriteLine("Finish Clean temp audio and video file");
                    }
                    catch (Exception ex)
                    {
                        this.StatusText = "다운로드 실패";
                        _logger.WriteLine(ex.ToString());
                        MessageBox.Show("다운로드 실패", "에러", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    this.DownloadEnable = false;
                });
            }
            else
            {
                this.StatusText = "다운로드 취소";
                _logger.WriteLine("Cancel Download");
                this.DownloadEnable = false;
            }
        }

        private void Process_DataReceived(object sender, DataReceivedEventArgs e)
        {
            _logger.WriteLine(e.Data);
        }
    }
}
