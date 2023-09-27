using Common.Utils;

namespace Common.Data;

public class SignaledMemoryObject : IDisposable
{
    public event EventHandler? Updated;
    public event EventHandler? UpdateRequested;

    internal EventWaitHandle ObjectUpdatedHandle { get; }
    internal EventWaitHandle UpdateRequestedHandle { get; }

    protected SignaledMemoryObject(string fileName)
    {
        ObjectUpdatedHandle = EventWaitHandleAcl.Create(false, EventResetMode.AutoReset, fileName + "-updated", out var updated, null);
        UpdateRequestedHandle = EventWaitHandleAcl.Create(false, EventResetMode.AutoReset, fileName + "-request", out var requested, null);

        if (updated)
        {
            ObjectUpdatedHandle.Reset();
        }

        if (requested)
        {
            UpdateRequestedHandle.Reset();
        }
        
        MemorySharedEventThread.AddObject(this);
    }

    public void RequestUpdate()
    {
        UpdateRequestedHandle.Set();
    }

    protected void SignalUpdated()
    {
        ObjectUpdatedHandle.Set();
    }

    public virtual void Dispose()
    {
        MemorySharedEventThread.RemoveObject(this);

        ObjectUpdatedHandle.Dispose();
        UpdateRequestedHandle.Dispose();
    }

    internal void OnUpdated()
    {
        Updated?.Invoke(this, EventArgs.Empty);
    }

    internal void OnUpdateRequested()
    {
        UpdateRequested?.Invoke(this, EventArgs.Empty);
    }

    protected void WaitForUpdate()
    {
        ObjectUpdatedHandle.WaitOne();
    }
}