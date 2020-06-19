using System;
using System.Windows.Forms;

namespace Aurora_Updater
{
    public partial class UpdateInfoForm : Form
    {
        public string changelog = "";
        public string currentVersion = "";
        public string updateVersion = "";
        public string updateDescription = "";
        public bool preRelease = false;
        public long updateSize = 0;

        public UpdateInfoForm()
        {
            InitializeComponent();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void buttonInstall_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void UpdateInfoForm_Shown(object sender, EventArgs e)
        {
            this.lblUpdateTitle.Text = preRelease ? "New Aurora Pre-release update is available!" : "New Aurora update is available!";
            this.labelUpdateDescription.Text = updateDescription;
            this.labelCurrentVersion.Text = $"Installed Version: {currentVersion}";
            this.labelUpdateVersion.Text = $"Update Version: {updateVersion}";
            this.richTextBoxChangelog.Text = changelog;
            this.labelUpdateSize.Text = $"Update Download Size: {SizeSuffix(updateSize, 2)}";
        }

        private void linkLabelViewHistory_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if(sender is LinkLabel)
            {
                (sender as LinkLabel).LinkVisited = true;
                System.Diagnostics.Process.Start("https://github.com/antonpup/Aurora/releases");
            }
        }

        // Source: http://stackoverflow.com/questions/14488796/does-net-provide-an-easy-way-convert-bytes-to-kb-mb-gb-etc
        static readonly string[] SizeSuffixes =
                   { "bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };
        static string SizeSuffix(Int64 value, int decimalPlaces = 1)
        {
            if (value < 0) { return "-" + SizeSuffix(-value); }
            if (value == 0) { return "0.0 bytes"; }

            int mag = (int)Math.Log(value, 1024);

            decimal adjustedSize = (decimal)value / (1L << (mag * 10));

            if (Math.Round(adjustedSize, decimalPlaces) >= 1000)
            {
                mag += 1;
                adjustedSize /= 1024;
            }

            return string.Format("{0:n" + decimalPlaces + "} {1}",
                adjustedSize,
                SizeSuffixes[mag]);
        }
    }
}
