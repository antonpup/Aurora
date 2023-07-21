using System;
using System.Threading;
using System.Threading.Tasks;
using Amib.Threading;

namespace Aurora.Modules;

public abstract class IAuroraModule : IDisposable
{
    private static readonly SmartThreadPool ModuleThreadPool = new(new STPStartInfo
    {
        AreThreadsBackground = true,
        ThreadPriority = ThreadPriority.AboveNormal,
        IdleTimeout = 10000,
    })
    {
        Name = "Initialize Threads",
        Concurrency = 10,
        MaxThreads = 6,
    };
    private TaskCompletionSource? _taskSource;

    private async Task QueueInit(Action action)
    {
        _taskSource = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        object WorkItemCallback(object _)
        {
            action.Invoke();
            return null;
        }

        void PostExecuteWorkItemCallback(IWorkItemResult _)
        {
            if (!_taskSource.TrySetResult())
            {
                Global.logger.Fatal("WTFFF");
            }
        }

        ModuleThreadPool.QueueWorkItem(WorkItemCallback, null, PostExecuteWorkItemCallback);
        if (ModuleThreadPool.IsIdle)
        {
            ModuleThreadPool.Start();
        }

        await _taskSource.Task;
    }

    public virtual async Task InitializeAsync()
    {
        await QueueInit(Initialize);
    }

    public abstract void Initialize();
    public abstract Task DisposeAsync();
    public abstract void Dispose();
}