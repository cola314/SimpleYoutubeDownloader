using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleYoutubeDownloader.ViewModels
{
    public class MainViewModel : ReactiveObject
    {
        public MainViewModel()
        {
            StatusText = "진행상황";
        }

        private string statusText_;
        public string StatusText
        {
            get => statusText_;
            set => this.RaiseAndSetIfChanged(ref statusText_, value);
        }

        private string searchText_;
        public string SearchText
        {
            get => searchText_;
            set => this.RaiseAndSetIfChanged(ref searchText_, value);
        }

        private int progress_;
        public int Progress
        {
            get => progress_;
            set => this.RaiseAndSetIfChanged(ref progress_, value);
        }

        public bool downloadEnable_;
        public bool DownloadEnable
        {
            get => downloadEnable_;
            set => this.RaiseAndSetIfChanged(ref downloadEnable_, value);
        }
    }
}
