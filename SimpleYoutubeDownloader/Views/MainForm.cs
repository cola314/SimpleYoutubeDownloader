using System;
using System.Windows.Forms;
using ReactiveUI;
using SimpleYoutubeDownloader.ViewModels;

namespace SimpleYoutubeDownloader.Views
{
    public partial class MainForm : Form, IViewFor<MainViewModel>
    {
        private readonly Logger _logger = Logger.Instance;

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
            _logger.OnWrite += (line) =>
            {
                Invoke(new Action(() =>
                {
                    logTextBox.AppendText(line);
                }));
            };

            _logger.WriteLine("Start Program");
        }
    }
}
