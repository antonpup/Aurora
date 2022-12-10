namespace Aurora_Updater
{
    partial class UpdateInfoForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UpdateInfoForm));
            this.labelCurrentVersion = new System.Windows.Forms.Label();
            this.labelUpdateVersion = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.richTextBoxChangelog = new System.Windows.Forms.RichTextBox();
            this.labelChangelogTitle = new System.Windows.Forms.Label();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonInstall = new System.Windows.Forms.Button();
            this.pictureBoxApplicationLogo = new System.Windows.Forms.PictureBox();
            this.lblUpdateTitle = new System.Windows.Forms.Label();
            this.labelUpdateDescription = new System.Windows.Forms.Label();
            this.linkLabelViewHistory = new System.Windows.Forms.LinkLabel();
            this.labelUpdateSize = new System.Windows.Forms.Label();
            this.skipButton = new System.Windows.Forms.Button();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxApplicationLogo)).BeginInit();
            this.SuspendLayout();
            // 
            // labelCurrentVersion
            // 
            this.labelCurrentVersion.AutoSize = true;
            this.labelCurrentVersion.Location = new System.Drawing.Point(14, 73);
            this.labelCurrentVersion.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelCurrentVersion.Name = "labelCurrentVersion";
            this.labelCurrentVersion.Size = new System.Drawing.Size(131, 15);
            this.labelCurrentVersion.TabIndex = 0;
            this.labelCurrentVersion.Text = "Installed Version: #.#.#x";
            // 
            // labelUpdateVersion
            // 
            this.labelUpdateVersion.AutoSize = true;
            this.labelUpdateVersion.Location = new System.Drawing.Point(14, 93);
            this.labelUpdateVersion.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelUpdateVersion.Name = "labelUpdateVersion";
            this.labelUpdateVersion.Size = new System.Drawing.Size(125, 15);
            this.labelUpdateVersion.TabIndex = 1;
            this.labelUpdateVersion.Text = "Update Version: #.#.#x";
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.Controls.Add(this.richTextBoxChangelog);
            this.panel1.Controls.Add(this.labelChangelogTitle);
            this.panel1.Location = new System.Drawing.Point(14, 133);
            this.panel1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(653, 352);
            this.panel1.TabIndex = 2;
            // 
            // richTextBoxChangelog
            // 
            this.richTextBoxChangelog.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.richTextBoxChangelog.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.richTextBoxChangelog.Location = new System.Drawing.Point(0, 18);
            this.richTextBoxChangelog.Margin = new System.Windows.Forms.Padding(0, 3, 0, 0);
            this.richTextBoxChangelog.Name = "richTextBoxChangelog";
            this.richTextBoxChangelog.ReadOnly = true;
            this.richTextBoxChangelog.Size = new System.Drawing.Size(653, 333);
            this.richTextBoxChangelog.TabIndex = 1;
            this.richTextBoxChangelog.Text = "";
            // 
            // labelChangelogTitle
            // 
            this.labelChangelogTitle.AutoSize = true;
            this.labelChangelogTitle.Location = new System.Drawing.Point(0, 0);
            this.labelChangelogTitle.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelChangelogTitle.Name = "labelChangelogTitle";
            this.labelChangelogTitle.Size = new System.Drawing.Size(68, 15);
            this.labelChangelogTitle.TabIndex = 0;
            this.labelChangelogTitle.Text = "Change log";
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.Location = new System.Drawing.Point(580, 492);
            this.buttonCancel.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(88, 27);
            this.buttonCancel.TabIndex = 3;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonInstall
            // 
            this.buttonInstall.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonInstall.Location = new System.Drawing.Point(388, 492);
            this.buttonInstall.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.buttonInstall.Name = "buttonInstall";
            this.buttonInstall.Size = new System.Drawing.Size(88, 27);
            this.buttonInstall.TabIndex = 4;
            this.buttonInstall.Text = "Install";
            this.buttonInstall.UseVisualStyleBackColor = true;
            this.buttonInstall.Click += new System.EventHandler(this.buttonInstall_Click);
            // 
            // pictureBoxApplicationLogo
            // 
            this.pictureBoxApplicationLogo.Image = global::Aurora_Updater.Properties.Resources.Aurora_updater_logo;
            this.pictureBoxApplicationLogo.Location = new System.Drawing.Point(14, 14);
            this.pictureBoxApplicationLogo.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.pictureBoxApplicationLogo.Name = "pictureBoxApplicationLogo";
            this.pictureBoxApplicationLogo.Size = new System.Drawing.Size(48, 48);
            this.pictureBoxApplicationLogo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pictureBoxApplicationLogo.TabIndex = 5;
            this.pictureBoxApplicationLogo.TabStop = false;
            // 
            // lblUpdateTitle
            // 
            this.lblUpdateTitle.AutoSize = true;
            this.lblUpdateTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblUpdateTitle.Location = new System.Drawing.Point(77, 14);
            this.lblUpdateTitle.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblUpdateTitle.Name = "lblUpdateTitle";
            this.lblUpdateTitle.Size = new System.Drawing.Size(261, 20);
            this.lblUpdateTitle.TabIndex = 6;
            this.lblUpdateTitle.Text = "New Aurora update is available!";
            // 
            // labelUpdateDescription
            // 
            this.labelUpdateDescription.AutoSize = true;
            this.labelUpdateDescription.Location = new System.Drawing.Point(78, 37);
            this.labelUpdateDescription.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelUpdateDescription.Name = "labelUpdateDescription";
            this.labelUpdateDescription.Size = new System.Drawing.Size(117, 15);
            this.labelUpdateDescription.TabIndex = 7;
            this.labelUpdateDescription.Text = "$UpdateDescription$";
            // 
            // linkLabelViewHistory
            // 
            this.linkLabelViewHistory.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.linkLabelViewHistory.AutoSize = true;
            this.linkLabelViewHistory.LinkArea = new System.Windows.Forms.LinkArea(0, 21);
            this.linkLabelViewHistory.Location = new System.Drawing.Point(14, 497);
            this.linkLabelViewHistory.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.linkLabelViewHistory.Name = "linkLabelViewHistory";
            this.linkLabelViewHistory.Size = new System.Drawing.Size(125, 15);
            this.linkLabelViewHistory.TabIndex = 8;
            this.linkLabelViewHistory.TabStop = true;
            this.linkLabelViewHistory.Text = "View previous updates";
            this.linkLabelViewHistory.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelViewHistory_LinkClicked);
            // 
            // labelUpdateSize
            // 
            this.labelUpdateSize.AutoSize = true;
            this.labelUpdateSize.Location = new System.Drawing.Point(14, 114);
            this.labelUpdateSize.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelUpdateSize.Name = "labelUpdateSize";
            this.labelUpdateSize.Size = new System.Drawing.Size(166, 15);
            this.labelUpdateSize.TabIndex = 9;
            this.labelUpdateSize.Text = "Update Download Size: ## MB";
            // 
            // skipButton
            // 
            this.skipButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.skipButton.Location = new System.Drawing.Point(484, 492);
            this.skipButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.skipButton.Name = "skipButton";
            this.skipButton.Size = new System.Drawing.Size(88, 27);
            this.skipButton.TabIndex = 10;
            this.skipButton.Text = "Skip Version";
            this.skipButton.UseVisualStyleBackColor = true;
            this.skipButton.Click += new System.EventHandler(this.skipButton_Click);
            // 
            // UpdateInfoForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(681, 532);
            this.Controls.Add(this.skipButton);
            this.Controls.Add(this.labelUpdateSize);
            this.Controls.Add(this.linkLabelViewHistory);
            this.Controls.Add(this.labelUpdateDescription);
            this.Controls.Add(this.lblUpdateTitle);
            this.Controls.Add(this.pictureBoxApplicationLogo);
            this.Controls.Add(this.buttonInstall);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.labelUpdateVersion);
            this.Controls.Add(this.labelCurrentVersion);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(697, 571);
            this.MinimumSize = new System.Drawing.Size(697, 571);
            this.Name = "UpdateInfoForm";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Aurora Updater";
            this.Shown += new System.EventHandler(this.UpdateInfoForm_Shown);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxApplicationLogo)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelCurrentVersion;
        private System.Windows.Forms.Label labelUpdateVersion;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.RichTextBox richTextBoxChangelog;
        private System.Windows.Forms.Label labelChangelogTitle;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonInstall;
        private System.Windows.Forms.PictureBox pictureBoxApplicationLogo;
        private System.Windows.Forms.Label lblUpdateTitle;
        private System.Windows.Forms.Label labelUpdateDescription;
        private System.Windows.Forms.LinkLabel linkLabelViewHistory;
        private System.Windows.Forms.Label labelUpdateSize;
        private System.Windows.Forms.Button skipButton;
    }
}