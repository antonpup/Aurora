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
        UpdateType update_type;

        public MainForm()
        {
            InitializeComponent();
        }

        public MainForm(UpdateType update_type)
        {
            InitializeComponent();
            this.update_type = update_type;
            StaticStorage.manager.clearLog();
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            progressTimer = new System.Windows.Forms.Timer();
            progressTimer.Interval = 1000;
            progressTimer.Tick += this.UpdateProgressTick;
            progressTimer.Enabled = true;
            progressTimer.Start();
            updaterThread = new Thread(() => StaticStorage.manager.retrieveUpdate(this.update_type));
            updaterThread.IsBackground = true;
            updaterThread.Start();
        }


        private void UpdateProgressTick(object sender, EventArgs args)
        {
            this.update_progress.Value = StaticStorage.manager.getTotalProgress();

            LogEntry[] logs = StaticStorage.manager.getLog();

            if (logs.Length > logTracker)
            {
                for (; logTracker < logs.Length; logTracker++)
                {
                    this.update_log_richtext.SelectionStart = this.update_log_richtext.TextLength;
                    this.update_log_richtext.SelectionLength = 0;

                    this.update_log_richtext.SelectionColor = logs[logTracker].getColor();
                    this.update_log_richtext.AppendText(logs[logTracker] + "\r\n");
                    this.update_log_richtext.SelectionColor = this.update_log_richtext.ForeColor;
                }

                this.update_log_richtext.ScrollToCaret();
            }
            

            /*
            if (StaticStorage.manager.updatestate != UpdateStatus.InProgress)
            {
                this.progressTimer.Enabled = false;
                this.progressTimer.Stop();
                
                return;
            }
            */
        }
    }
}
