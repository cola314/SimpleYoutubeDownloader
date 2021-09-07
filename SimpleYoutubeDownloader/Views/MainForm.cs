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

        private void downloadButton_Click(object sender, EventArgs e)
        {
            if (ViewModel.DownloadEnable)
            {
                return;
            }

            ViewModel.DownloadEnable = true;
            ViewModel.StatusText = "다운로드 시작";
            logger.WriteLine("Start Download");
            var saveDialog = new SaveFileDialog();
            saveDialog.DefaultExt = ".mp4";
            saveDialog.Filter = "MPEG-4 Video|*.mp4";

            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                string downloadPath = downloadPathTextBox.Text;
                string targetFileName = saveDialog.FileName;

                logger.WriteLine($"Download Path : {downloadPath}");
                logger.WriteLine($"Target File : {targetFileName}");

                Task.Run(async () =>
                {
                    try
                    {
                        // search video
                        ViewModel.StatusText = "비디오 검색 중";
                        logger.WriteLine($"Search Video Info");
                        logger.WriteLine($"URL : \"{downloadPath}\"\nSaveFile : \"{targetFileName}\"");

                        var youtube = new YoutubeClient();
                        var video = await youtube.Videos.GetAsync(downloadPath);
                        logger.WriteLine($"id : {video.Id}\ntitle : {video.Title}\nAuthor : {video.Author}");

                        var streamManifest = await youtube.Videos.Streams.GetManifestAsync(video.Id);
                        logger.WriteLine($"{streamManifest.Streams.Count} numbers of stream found");

                        var audioStreamInfo = streamManifest.GetAudioOnlyStreams()
                            .Where(x => x.Container.Name == "mp4")
                            .OrderByDescending(x => x.Bitrate.BitsPerSecond)
                            .FirstOrDefault();

                        var videoStreamInfo = streamManifest.GetVideoOnlyStreams()
                            .Where(x => x.Container.Name == "mp4")
                            .OrderByDescending(x => x.VideoQuality.MaxHeight)
                            .FirstOrDefault();

                        if(audioStreamInfo == null)
                        {
                            logger.WriteLine("Audio stream info not found");
                            throw new Exception("다운로드 가능한 오디오가 없습니다");
                        }

                        if(videoStreamInfo == null)
                        {
                            logger.WriteLine("Video stream info not found");
                            throw new Exception("다운로드 가능한 비디오가 없습니다");
                        }

                        // download video
                        logger.WriteLine("Start download audio and video from server");
                        int audioProgress = 0, videoProgress = 0;

                        var videoDownload = youtube.Videos.Streams.DownloadAsync(videoStreamInfo, VIDEO_FILE,
                            new ProgressAction(val =>
                            {
                                int newProgress = (int)(val * 50);
                                if(newProgress / 5 > videoProgress / 5)
                                {
                                    logger.WriteLine($"Video download progress : {newProgress / 5 * 5}%");
                                }
                                videoProgress = newProgress;

                                if (audioProgress + videoProgress != 100)
                                {
                                    ViewModel.Progress = audioProgress + videoProgress;
                                    ViewModel.StatusText = $"다운로드 중 {audioProgress + videoProgress}% 완료";
                                }
                            }));

                        var audioDownload = youtube.Videos.Streams.DownloadAsync(audioStreamInfo, AUDIO_FILE,
                            new ProgressAction(val =>
                            {
                                int newProgress = (int)(val * 50);
                                if (newProgress / 5 > audioProgress / 5)
                                {
                                    logger.WriteLine($"Audio download progress : {2 * newProgress / 5 * 5}%");
                                }
                                audioProgress = newProgress;

                                if (audioProgress + videoProgress != 100)
                                {
                                    ViewModel.Progress = audioProgress + videoProgress;
                                    ViewModel.StatusText = $"다운로드 중 {audioProgress + videoProgress}% 완료";
                                }
                            }));


                        Task.WaitAll(videoDownload.AsTask(), audioDownload.AsTask());
                        logger.WriteLine("Finish download audio and video from server");

                        // combine audio and video
                        logger.WriteLine("Start combine audio and video");
                        ViewModel.StatusText = "mp4 변환 작업 중";

                        ProcessStartInfo startInfo = new ProcessStartInfo();
                        startInfo.UseShellExecute = false;
                        startInfo.CreateNoWindow = true;
                        startInfo.FileName = "ffmpeg.exe";
                        startInfo.Arguments = $"-i {VIDEO_FILE} -i {AUDIO_FILE} -c:v copy -c:a aac \"{targetFileName}\"";
                        startInfo.RedirectStandardOutput = true;
                        startInfo.RedirectStandardError = true;
                        logger.WriteLine($"{startInfo.FileName} {startInfo.Arguments}");

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

                        logger.WriteLine("Finish combine audio and video");

                        ViewModel.Progress = 100;
                        ViewModel.StatusText = "다운로드 완료";
                        MessageBox.Show("다운로드 완료", "정보", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        Process.Start("explorer.exe", Path.GetDirectoryName(targetFileName));

                        // clean up
                        logger.WriteLine("Clean temp audio and video file");
                        File.Delete(VIDEO_FILE);
                        File.Delete(AUDIO_FILE);
                        logger.WriteLine("Finish Clean temp audio and video file");
                    }
                    catch(Exception ex)
                    {
                        ViewModel.StatusText = "다운로드 실패";
                        logger.WriteLine(ex.ToString());
                        MessageBox.Show("다운로드 실패", "에러", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    ViewModel.DownloadEnable = false;
                });
            }
            else
            {
                ViewModel.StatusText = "다운로드 취소";
                logger.WriteLine("Cancel Download");
                ViewModel.DownloadEnable = false;
            }
        }

        private void Process_DataReceived(object sender, DataReceivedEventArgs e)
        {
            logger.WriteLine(e.Data);
        }
    }
}
