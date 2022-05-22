using ReactiveUI;
using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows.Forms;
using SimpleYoutubeDownloader.Services;

namespace SimpleYoutubeDownloader.ViewModels
{
    public class MainViewModel : ReactiveObject
    {
        private readonly Logger _logger = Logger.Instance;

        public MainViewModel()
        {
            DownloadCommand = ReactiveCommand.Create(DownloadVideo);
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

        private async void DownloadVideo()
        {
            if (DownloadEnable) return;
            DownloadEnable = true;

            var saveDialog = new SaveFileDialog();
            saveDialog.DefaultExt = ".mp4";
            saveDialog.Filter = "MPEG-4 Video|*.mp4";

            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                StatusText = "다운로드 시작";
                _logger.WriteLine("Start Download");

                var downloader = new VideoDownloader(_logger);
                downloader.Status
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(x => StatusText = x);

                downloader.Progress
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(x => Progress = x);
                try
                {
                    await downloader.DownloadVideoAsync(SearchText, saveDialog.FileName);
                }
                catch (Exception ex)
                {
                    StatusText = "다운로드 실패";
                    _logger.WriteLine(ex.ToString());
                    MessageBox.Show("다운로드 실패", "에러", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                StatusText = "다운로드 취소";
                _logger.WriteLine("Cancel Download");
            }

            DownloadEnable = false;
        }
    }
}