using ReactiveUI;
using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows.Forms;
using SimpleYoutubeDownloader.Properties;
using SimpleYoutubeDownloader.Services;

namespace SimpleYoutubeDownloader.ViewModels
{
    public class MainViewModel : ReactiveObject
    {
        private readonly Logger _logger = Logger.Instance;

        public MainViewModel()
        {
            DownloadCommand = ReactiveCommand.Create(DownloadVideo, this.WhenAnyValue(x => x.DownloadEnable));
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

        private bool _downloadEnable = true;

        public bool DownloadEnable
        {
            get => _downloadEnable;
            set => this.RaiseAndSetIfChanged(ref _downloadEnable, value);
        }

        public ReactiveCommand<Unit, Unit> DownloadCommand { get; }

        private async void DownloadVideo()
        {
            DownloadEnable = false;

            var saveDialog = new SaveFileDialog();
            saveDialog.DefaultExt = ".mp4";
            saveDialog.Filter = "MPEG-4 Video|*.mp4";

            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                var downloader = CreateVideoDownloader();
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
                    StatusText = Resources.DOWNLOAD_FAIL;
                    _logger.WriteLine(ex.ToString());
                    MessageBox.Show(Resources.DOWNLOAD_FAIL, Resources.ERROR, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                StatusText = Resources.DOWNLOAD_CANCEL;
                _logger.WriteLine("Cancel Download");
            }

            DownloadEnable = true;
        }

        private VideoDownloader CreateVideoDownloader()
        {
            return new YtdlVideoDownloader(_logger);
            //return new YoutubeExplodeVideoDownloader(_logger);
        }
    }
}