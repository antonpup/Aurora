using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Amib.Threading;

namespace Aurora.Modules;

public abstract class AuroraModule : IDisposable
{
    private static readonly SmartThreadPool ModuleThreadPool = new(new STPStartInfo
    {
        AreThreadsBackground = true,
        ThreadPriority = ThreadPriority.AboveNormal,
    })
    {
        Name = "Initialize Threads",
        Concurrency = 10,
        MaxThreads = 6,
    };

    private TaskCompletionSource? _taskSource;

    private async Task QueueInit(Func<Task> action)
    {
        _taskSource = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        ModuleThreadPool.QueueWorkItem(WorkItemCallback, null, PostExecuteWorkItemCallback);
        if (ModuleThreadPool.IsIdle)
        {
            ModuleThreadPool.Start();
        }

        await _taskSource.Task;
        return;

        async Task WorkItemCallback(object _)
        {
            await action();
        }

        void PostExecuteWorkItemCallback(IWorkItemResult _)
        {
            Application.Current.Dispatcher.Invoke(() => { _taskSource.SetResult(); });
        }
    }

    public virtual async Task InitializeAsync()
    {
        await QueueInit(InitButWait);
    }

    private async Task InitButWait()
    {
        await Initialize();
    }

    protected abstract Task Initialize();
    public abstract Task DisposeAsync();
    public abstract void Dispose();
}