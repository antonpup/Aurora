using System;
using System.Threading;
using System.Windows.Forms;

namespace Aurora_Updater
{
    public partial class MainForm : Form
    {
        System.Windows.Forms.Timer progressTimer;
        Thread updaterThread;
        int logTracker = 0;
        UpdateType updatetype;

        public MainForm()
        {
            InitializeComponent();
        }

        public MainForm(UpdateType update_type)
        {
            InitializeComponent();
            this.updatetype = update_type;
            StaticStorage.Manager.ClearLog();
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            progressTimer = new System.Windows.Forms.Timer();
            progressTimer.Interval = 1000;
            progressTimer.Tick += this.UpdateProgressTick;
            progressTimer.Enabled = true;
            progressTimer.Start();
            updaterThread = new Thread(() => StaticStorage.Manager.RetrieveUpdate(this.updatetype));
            updaterThread.IsBackground = true;
            updaterThread.Start();
        }


        private void UpdateProgressTick(object sender, EventArgs args)
        {
            this.update_progress.Value = StaticStorage.Manager.GetTotalProgress();

            LogEntry[] logs = StaticStorage.Manager.GetLog();

            if (logs.Length > logTracker)
            {
                for (; logTracker < logs.Length; logTracker++)
                {
                    this.richtextUpdateLog.SelectionStart = this.richtextUpdateLog.TextLength;
                    this.richtextUpdateLog.SelectionLength = 0;

                    this.richtextUpdateLog.SelectionColor = logs[logTracker].GetColor();
                    this.richtextUpdateLog.AppendText(logs[logTracker] + "\r\n");
                    this.richtextUpdateLog.SelectionColor = this.richtextUpdateLog.ForeColor;
                }

                this.richtextUpdateLog.ScrollToCaret();
            }
        }
    }
}
