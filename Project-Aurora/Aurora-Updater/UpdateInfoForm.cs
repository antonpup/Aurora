using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace Aurora_Updater;

public partial class UpdateInfoForm : Form
{
    public string Changelog = "";
    public string CurrentVersion = "";
    public string UpdateVersion = "";
    public string UpdateDescription = "";
    public bool PreRelease = false;
    public long UpdateSize = 0;

    public UpdateInfoForm()
    {
        InitializeComponent();
    }

    private void buttonCancel_Click(object? sender, EventArgs e)
    {
        DialogResult = DialogResult.Cancel;
        Close();
    }

    private void buttonInstall_Click(object? sender, EventArgs e)
    {
        DialogResult = DialogResult.OK;
        Close();
    }

    private void UpdateInfoForm_Shown(object? sender, EventArgs e)
    {
        lblUpdateTitle.Text = PreRelease ? "New Aurora Pre-release update is available!" : "New Aurora update is available!";
        labelUpdateDescription.Text = UpdateDescription;
        labelCurrentVersion.Text = $"Installed Version: {CurrentVersion}";
        labelUpdateVersion.Text = $"Update Version: {UpdateVersion}";
        richTextBoxChangelog.Text = Changelog;
        labelUpdateSize.Text = $"Update Download Size: {SizeSuffix(UpdateSize, 2)}";
    }

    private void linkLabelViewHistory_LinkClicked(object? sender, LinkLabelLinkClickedEventArgs e)
    {
        if (sender is not LinkLabel label) return;
        label.LinkVisited = true;
        Process.Start("explorer", "https://github.com/Aurora-RGB/Aurora/releases");
    }

    // Source: http://stackoverflow.com/questions/14488796/does-net-provide-an-easy-way-convert-bytes-to-kb-mb-gb-etc
    private static readonly string[] SizeSuffixes = { "bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };

    private static string SizeSuffix(long value, int decimalPlaces = 1)
    {
        if (value < 0) { return "-" + SizeSuffix(-value); }
        if (value == 0) { return "0.0 bytes"; }

        var mag = (int)Math.Log(value, 1024);

        var adjustedSize = (decimal)value / (1L << (mag * 10));

        if (Math.Round(adjustedSize, decimalPlaces) >= 1000)
        {
            mag += 1;
            adjustedSize /= 1024;
        }

        return string.Format("{0:n" + decimalPlaces + "} {1}",
            adjustedSize,
            SizeSuffixes[mag]);
    }

    private void skipButton_Click(object? sender, EventArgs e)
    {
        File.WriteAllText("skipversion.txt", UpdateVersion);
        DialogResult = DialogResult.Cancel;
        Close();
    }
}