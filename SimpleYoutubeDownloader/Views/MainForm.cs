using ReactiveUI;
using SimpleYoutubeDownloader.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;

namespace SimpleYoutubeDownloader
{
    public partial class MainForm : Form, IViewFor<MainViewModel>
    {
        const string VIDEO_FILE = "video.mp4";
        const string AUDIO_FILE = "audio.mp4";

        private Logger logger = Logger.Instance;

        public MainViewModel ViewModel { get; set; }

        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = value as MainViewModel;
        }

        public MainForm()
        {
            InitializeComponent();

            this.WhenActivated(d =>
            {
                d(this.Bind(ViewModel, vm => vm.SearchText, v => v.downloadPathTextBox.Text));
                d(this.OneWayBind(ViewModel, vm => vm.StatusText, v => v.progressLabel.Text));
                d(this.OneWayBind(ViewModel, vm => vm.Progress, v => v.downloadProgressBar.Value));
                d(this.BindCommand(ViewModel, vm => vm.DownloadCommand, v => v.downloadButton, nameof(downloadButton.Click)));
            });

            ViewModel = new MainViewModel();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            logger.OnWrite += (line) =>
            {
                Invoke(new Action(() =>
                {
                    logTextBox.AppendText(line);
                }));
            };

            logger.WriteLine("Start Program");
        }
    }
}
