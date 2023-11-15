using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using Aurora.Utils;
using Aurora.Utils.IpApi;
using Microsoft.Win32.TaskScheduler;
using Xceed.Wpf.Toolkit;

namespace Aurora.Settings.Controls;

public partial class Control_SettingsGeneral
{
    private const string StartupTaskId = "AuroraStartup";
    
    public Control_SettingsGeneral()
    {
        InitializeComponent();

        TransparencyCheckbox.IsEnabled = TransparencyComponent.UseMica;
        
        DataContext = Global.Configuration;

        SetAutostartTask();
    }

    private void SetAutostartTask()
    {
        try
        {
            using var service = new TaskService();
            var task = service.FindTask(StartupTaskId);
            var exePath = Assembly.GetExecutingAssembly().Location.Replace(".dll", ".exe");

            var taskDefinition = task != null ? task.Definition : service.NewTask();

            //Update path of startup task
            taskDefinition.RegistrationInfo.Description = "Start Aurora on Startup";
            taskDefinition.Actions.Clear();
            taskDefinition.Actions.Add(new ExecAction(exePath, "-silent", Path.GetDirectoryName(exePath)));
            if (task != null)
            {
                startDelayAmount.Value = task.Definition.Triggers.FirstOrDefault(t =>
                    t.TriggerType == TaskTriggerType.Logon
                ) is LogonTrigger trigger
                    ? (int)trigger.Delay.TotalSeconds
                    : 0;
            }
            else
            {
                taskDefinition.Triggers.Add(new LogonTrigger { Enabled = true });

                taskDefinition.Principal.RunLevel = TaskRunLevel.Highest;
                taskDefinition.Settings.DisallowStartIfOnBatteries = false;
                taskDefinition.Settings.DisallowStartOnRemoteAppSession = false;
                taskDefinition.Settings.ExecutionTimeLimit = TimeSpan.Zero;
            }

            task = service.RootFolder.RegisterTaskDefinition(StartupTaskId, taskDefinition);
            RunAtWinStartup.IsChecked = task.Enabled;
        }
        catch (Exception exc)
        {
            Global.logger.Error(exc, "Error caught when updating startup task");
        }
    }

    /// <summary>The excluded program the user has selected in the excluded list.</summary>
    public string SelectedExcludedProgram { get; set; }

    private void ExcludedAdd_Click(object? sender, RoutedEventArgs e)
    {
        var dialog = new Window_ProcessSelection { ButtonLabel = "Exclude Process" };
        if (dialog.ShowDialog() == true &&
            !string.IsNullOrWhiteSpace(dialog.ChosenExecutableName) &&
            !Global.Configuration.ExcludedPrograms.Contains(dialog.ChosenExecutableName)
           )
            Global.Configuration.ExcludedPrograms.Add(dialog.ChosenExecutableName);
    }

    private void ExcludedRemove_Click(object? sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(SelectedExcludedProgram))
            Global.Configuration.ExcludedPrograms.Remove(SelectedExcludedProgram);
    }

    private void RunAtWinStartup_Checked(object? sender, RoutedEventArgs e)
    {
        if (!IsLoaded || sender is not CheckBox checkBox) return;
        try
        {
            using var ts = new TaskService();
            //Find existing task
            var task = ts.FindTask(StartupTaskId);
            task.Enabled = checkBox.IsChecked.Value;
        }
        catch (Exception exc)
        {
            Global.logger.Error(exc, "RunAtWinStartup_Checked Exception: ");
        }
    }

    private void HighPriorityCheckbox_Checked(object? sender, RoutedEventArgs e)
    {
        Process.GetCurrentProcess().PriorityClass = Global.Configuration.HighPriority ? ProcessPriorityClass.High : ProcessPriorityClass.Normal;
    }

    private void StartDelayAmount_ValueChanged(object? sender, RoutedPropertyChangedEventArgs<object> e)
    {
        using var service = new TaskService();
        var task = service.FindTask(StartupTaskId);
        if (task?.Definition.Triggers.FirstOrDefault(t => t.TriggerType == TaskTriggerType.Logon) is not
            LogonTrigger trigger) return;
        trigger.Delay = new TimeSpan(0, 0, ((IntegerUpDown)sender).Value ?? 0);
        task.RegisterChanges();
    }

    private async void ResetLocation_Clicked(object sender, RoutedEventArgs e)
    {
        try
        {
            var ipData = await IpApiClient.GetIpData();
            Global.Configuration.Lat = ipData.Lat;
            Global.Configuration.Lon = ipData.Lon;
        }
        catch (Exception exc)
        {
            Global.logger.Error(exc, "Failed getting geographic data");
        }
    }
}