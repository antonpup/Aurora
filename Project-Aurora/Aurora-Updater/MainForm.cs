using System;
using System.Threading;
using System.Windows.Forms;

namespace Aurora_Updater;

public partial class MainForm : Form
{
    private readonly System.Windows.Forms.Timer _progressTimer = new();
    private readonly Thread _updaterThread;
    private int _logTracker;

    public MainForm()
    {
        _updaterThread = new Thread(() => StaticStorage.Manager.RetrieveUpdate());
        InitializeComponent();
        StaticStorage.Manager.ClearLog();
    }

    private void Form1_Shown(object? sender, EventArgs e)
    {
        _progressTimer.Interval = 1000;
        _progressTimer.Tick += UpdateProgressTick;
        _progressTimer.Enabled = true;
        _progressTimer.Start();
        _updaterThread.IsBackground = true;
        _updaterThread.Start();
    }

    private void UpdateProgressTick(object? sender, EventArgs args)
    {
        update_progress.Value = StaticStorage.Manager.GetTotalProgress();

        var logs = StaticStorage.Manager.GetLog();

        if (logs.Length <= _logTracker) return;
        for (; _logTracker < logs.Length; _logTracker++)
        {
            richtextUpdateLog.SelectionStart = richtextUpdateLog.TextLength;
            richtextUpdateLog.SelectionLength = 0;

            richtextUpdateLog.SelectionColor = logs[_logTracker].GetColor();
            richtextUpdateLog.AppendText(logs[_logTracker] + "\r\n");
            richtextUpdateLog.SelectionColor = richtextUpdateLog.ForeColor;
        }

        richtextUpdateLog.ScrollToCaret();
    }
}