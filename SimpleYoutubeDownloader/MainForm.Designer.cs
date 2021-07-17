﻿
namespace SimpleYoutubeDownloader
{
    partial class MainForm
    {
        /// <summary>
        /// 필수 디자이너 변수입니다.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 사용 중인 모든 리소스를 정리합니다.
        /// </summary>
        /// <param name="disposing">관리되는 리소스를 삭제해야 하면 true이고, 그렇지 않으면 false입니다.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form 디자이너에서 생성한 코드

        /// <summary>
        /// 디자이너 지원에 필요한 메서드입니다. 
        /// 이 메서드의 내용을 코드 편집기로 수정하지 마세요.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.downloadPathTextBox = new System.Windows.Forms.TextBox();
            this.downloadButton = new System.Windows.Forms.Button();
            this.downloadProgressBar = new System.Windows.Forms.ProgressBar();
            this.progressLabel = new System.Windows.Forms.Label();
            this.logTextBox = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // downloadPathTextBox
            // 
            this.downloadPathTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.downloadPathTextBox.Location = new System.Drawing.Point(12, 21);
            this.downloadPathTextBox.Name = "downloadPathTextBox";
            this.downloadPathTextBox.Size = new System.Drawing.Size(749, 26);
            this.downloadPathTextBox.TabIndex = 0;
            // 
            // downloadButton
            // 
            this.downloadButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.downloadButton.Location = new System.Drawing.Point(767, 21);
            this.downloadButton.Name = "downloadButton";
            this.downloadButton.Size = new System.Drawing.Size(106, 26);
            this.downloadButton.TabIndex = 1;
            this.downloadButton.Text = "다운로드";
            this.downloadButton.UseVisualStyleBackColor = true;
            this.downloadButton.Click += new System.EventHandler(this.downloadButton_Click);
            // 
            // downloadProgressBar
            // 
            this.downloadProgressBar.Location = new System.Drawing.Point(15, 91);
            this.downloadProgressBar.Name = "downloadProgressBar";
            this.downloadProgressBar.Size = new System.Drawing.Size(858, 23);
            this.downloadProgressBar.TabIndex = 2;
            // 
            // progressLabel
            // 
            this.progressLabel.AutoSize = true;
            this.progressLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.progressLabel.Location = new System.Drawing.Point(12, 57);
            this.progressLabel.Name = "progressLabel";
            this.progressLabel.Size = new System.Drawing.Size(57, 20);
            this.progressLabel.TabIndex = 3;
            this.progressLabel.Text = "진행상황";
            // 
            // logTextBox
            // 
            this.logTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.logTextBox.Location = new System.Drawing.Point(12, 133);
            this.logTextBox.Multiline = true;
            this.logTextBox.Name = "logTextBox";
            this.logTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.logTextBox.Size = new System.Drawing.Size(861, 389);
            this.logTextBox.TabIndex = 4;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(888, 534);
            this.Controls.Add(this.logTextBox);
            this.Controls.Add(this.progressLabel);
            this.Controls.Add(this.downloadProgressBar);
            this.Controls.Add(this.downloadButton);
            this.Controls.Add(this.downloadPathTextBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.Text = "SimpleYoutubeDownloader";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox downloadPathTextBox;
        private System.Windows.Forms.Button downloadButton;
        private System.Windows.Forms.ProgressBar downloadProgressBar;
        private System.Windows.Forms.Label progressLabel;
        private System.Windows.Forms.TextBox logTextBox;
    }
}

