using System;
using System.Management;
using System.Security.Principal;
using JetBrains.Annotations;
using Microsoft.Win32;

namespace Aurora.Utils;

public class RegistryChangedEventArgs : EventArgs
{
    public readonly object? Data;

    public RegistryChangedEventArgs(object data)
    {
        Data = data;
    }
}

public enum RegistryHiveOpt
{
    CurrentUser,
    LocalMachine,
}

public sealed class RegistryWatcher : IDisposable
{
    public event EventHandler<RegistryChangedEventArgs>? RegistryChanged;

    private readonly RegistryHiveOpt _registryHive;
    private readonly string _key;
    private readonly string _value;
    private ManagementEventWatcher? _eventWatcher;

    public RegistryWatcher(RegistryHiveOpt registryHive, string key, string value)
    {
        _registryHive = registryHive;
        _key = key;
        _value = value;
    }

    public void StartWatching()
    {
        var currentUser = WindowsIdentity.GetCurrent();
        var scope = new ManagementScope(@"\\.\root\default");

        var queryString = _registryHive switch
        {
            RegistryHiveOpt.LocalMachine => string.Format(
                "SELECT * FROM RegistryValueChangeEvent WHERE Hive='HKEY_LOCAL_MACHINE' AND KeyPath='{0}' AND ValueName='{1}'",
                _key.Replace(@"\", @"\\"), _value),
            RegistryHiveOpt.CurrentUser => string.Format(
                @"SELECT * FROM RegistryValueChangeEvent WHERE Hive='HKEY_USERS' AND KeyPath='{0}\\{1}' AND ValueName='{2}'",
                currentUser.User!.Value, _key.Replace(@"\", @"\\"), _value),
        };
        var query = new WqlEventQuery(queryString);
        _eventWatcher = new ManagementEventWatcher(scope, query);
        _eventWatcher.EventArrived += KeyWatcherOnEventArrived;
        try
        {
            _eventWatcher.Start();

            SendData();
        }
        catch (Exception)
        {
            Global.logger.Error("Registry not available, Query: {Query}", queryString);
        }
    }

    public void StopWatching()
    {
        if (_eventWatcher == null)
        {
            return;
        }

        _eventWatcher.EventArrived -= KeyWatcherOnEventArrived;
        _eventWatcher.Stop();
        _eventWatcher.Dispose();
        _eventWatcher = null;
    }

    private void KeyWatcherOnEventArrived(object? sender, EventArrivedEventArgs e)
    {
        SendData();
    }

    private void SendData()
    {
        var localMachine = _registryHive switch
        {
            RegistryHiveOpt.LocalMachine => Registry.LocalMachine,
            RegistryHiveOpt.CurrentUser => Registry.CurrentUser,
        };
        using var key = localMachine.OpenSubKey(_key);
        var data = key?.GetValue(_value);
        if (data == null)
        {
            return;
        }

        RegistryChanged?.Invoke(this, new RegistryChangedEventArgs(data));
    }

    public void Dispose()
    {
        StopWatching();
    }
}