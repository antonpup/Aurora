namespace Aurora_Updater
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.update_progress = new System.Windows.Forms.ProgressBar();
            this.richtextUpdateLog = new System.Windows.Forms.RichTextBox();
            this.labelApplicationTitle = new System.Windows.Forms.Label();
            this.pictureBoxApplicationLogo = new System.Windows.Forms.PictureBox();
            this.labelUpdateLog = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxApplicationLogo)).BeginInit();
            this.SuspendLayout();
            // 
            // update_progress
            // 
            this.update_progress.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.update_progress.Location = new System.Drawing.Point(12, 352);
            this.update_progress.Name = "update_progress";
            this.update_progress.Size = new System.Drawing.Size(560, 22);
            this.update_progress.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.update_progress.TabIndex = 0;
            // 
            // richtextUpdateLog
            // 
            this.richtextUpdateLog.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.richtextUpdateLog.Location = new System.Drawing.Point(12, 82);
            this.richtextUpdateLog.Name = "richtextUpdateLog";
            this.richtextUpdateLog.ReadOnly = true;
            this.richtextUpdateLog.Size = new System.Drawing.Size(560, 264);
            this.richtextUpdateLog.TabIndex = 1;
            this.richtextUpdateLog.Text = "";
            // 
            // labelApplicationTitle
            // 
            this.labelApplicationTitle.AutoSize = true;
            this.labelApplicationTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelApplicationTitle.Location = new System.Drawing.Point(66, 12);
            this.labelApplicationTitle.Name = "labelApplicationTitle";
            this.labelApplicationTitle.Size = new System.Drawing.Size(156, 20);
            this.labelApplicationTitle.TabIndex = 9;
            this.labelApplicationTitle.Text = "Updating Aurora...";
            // 
            // pictureBoxApplicationLogo
            // 
            this.pictureBoxApplicationLogo.Image = global::Aurora_Updater.Properties.Resources.Aurora_updater_logo;
            this.pictureBoxApplicationLogo.Location = new System.Drawing.Point(12, 12);
            this.pictureBoxApplicationLogo.Name = "pictureBoxApplicationLogo";
            this.pictureBoxApplicationLogo.Size = new System.Drawing.Size(48, 48);
            this.pictureBoxApplicationLogo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pictureBoxApplicationLogo.TabIndex = 8;
            this.pictureBoxApplicationLogo.TabStop = false;
            // 
            // labelUpdateLog
            // 
            this.labelUpdateLog.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelUpdateLog.AutoSize = true;
            this.labelUpdateLog.Location = new System.Drawing.Point(9, 66);
            this.labelUpdateLog.Name = "labelUpdateLog";
            this.labelUpdateLog.Size = new System.Drawing.Size(75, 13);
            this.labelUpdateLog.TabIndex = 10;
            this.labelUpdateLog.Text = "Update details";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(584, 381);
            this.Controls.Add(this.richtextUpdateLog);
            this.Controls.Add(this.labelUpdateLog);
            this.Controls.Add(this.labelApplicationTitle);
            this.Controls.Add(this.pictureBoxApplicationLogo);
            this.Controls.Add(this.update_progress);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(600, 450);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(600, 350);
            this.Name = "MainForm";
            this.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Aurora Updater";
            this.Shown += new System.EventHandler(this.Form1_Shown);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxApplicationLogo)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ProgressBar update_progress;
        private System.Windows.Forms.RichTextBox richtextUpdateLog;
        private System.Windows.Forms.Label labelApplicationTitle;
        private System.Windows.Forms.PictureBox pictureBoxApplicationLogo;
        private System.Windows.Forms.Label labelUpdateLog;
    }
}

