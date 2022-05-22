using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using System.Windows.Forms;
using ReactiveUI;
using YoutubeExplode;

namespace SimpleYoutubeDownloader.Services
{
    public class VideoDownloader
    {
        private const string VIDEO_FILE = "video.mp4";
        private const string AUDIO_FILE = "audio.mp4";

        private readonly BehaviorSubject<int> _progressSubject;

        private readonly BehaviorSubject<string> _statusSubject;

        private readonly Logger _logger;

        public VideoDownloader(Logger logger)
        {
            _logger = logger;
            _statusSubject = new BehaviorSubject<string>(string.Empty);
            _progressSubject = new BehaviorSubject<int>(0);
        }

        public IObservable<string> Status => _statusSubject.AsObservable();
        public IObservable<int> Progress => _progressSubject.AsObservable();

        public Task DownloadVideoAsync(string url, string downloadFile)
        {
            _logger.WriteLine($"Download Url : {url}");
            _logger.WriteLine($"Download Path : {downloadFile}");

            // search video
            SetStatus("비디오 검색 중");

            return Task.Run(async () =>
            {
                var youtube = new YoutubeClient();
                var video = await youtube.Videos.GetAsync(url);
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
                    .Select(x => (int) (x * 100))
                    .Distinct(progress => progress / 5)
                    .Publish();

                var audioDownload = Observable.Create<double>(async observer =>
                    {
                        await youtube.Videos.Streams.DownloadAsync(audioStreamInfo, AUDIO_FILE,
                            new Progress<double>(observer.OnNext));
                        observer.OnCompleted();
                        return Disposable.Empty;
                    })
                    .Select(x => (int) (x * 100))
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
                        SetProgress(totalProgress);
                        SetStatus($"다운로드 중 {totalProgress}% 완료");
                    });

                videoDownload.Connect();
                audioDownload.Connect();

                await Task.WhenAll(videoDownload.ToTask(), audioDownload.ToTask());
                _logger.WriteLine("Finish download audio and video from server");

                // combine audio and video
                SetStatus("mp4 변환 작업 시작");

                AudioCombineService audioCombineService = new AudioCombineService(_logger);
                audioCombineService.Progress
                    .Do(x =>
                    {
                        _logger.WriteLine($"Mp4 Converting progress {x}%");
                        SetStatus($"mp4 변환 작업 중 {x}% 완료");
                    })
                    .Subscribe(SetProgress);

                audioCombineService.CombineAudioToVideo(AUDIO_FILE, VIDEO_FILE, downloadFile);

                SetProgress(100);
                SetStatus("다운로드 완료");
                MessageBox.Show("다운로드 완료", "정보", MessageBoxButtons.OK, MessageBoxIcon.Information);

                Process.Start("explorer.exe", Path.GetDirectoryName(downloadFile));

                // clean up
                _logger.WriteLine("Clean temp audio and video file");
                File.Delete(VIDEO_FILE);
                File.Delete(AUDIO_FILE);
                _logger.WriteLine("Finish Clean temp audio and video file");
            });
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