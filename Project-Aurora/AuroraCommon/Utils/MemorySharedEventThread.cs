using Common.Data;
using Microsoft.Scripting.Utils;

namespace Common.Utils;

internal static class MemorySharedEventThread
{
    private static readonly List<HandlesAndThread> HandleThreads = new();

    internal static void AddObject(SignaledMemoryObject o)
    {
        var handleThread = HandleThreads.Find(ht => ht.HasSpace(2));
        if (handleThread == null)
        {
            handleThread = new HandlesAndThread();
            HandleThreads.Add(handleThread);
        }
        
        handleThread.AddToThread(o);
    }

    internal static void RemoveObject(SignaledMemoryObject o)
    {
        foreach (var handlesAndThread in HandleThreads)
        {
            handlesAndThread.RemoveIfExists(o);
        }
    }

    private class HandlesAndThread
    {
        private const int MaxHandles = 64;
    
        private CancellationTokenSource _cancellation = new();
        private Thread _thread = new(() => { });
        
        private Action[] _actions = { () => { } };
        private WaitHandle[] _handles;
        
        private readonly SemaphoreSlim _semaphore = new(1);

        internal HandlesAndThread()
        {
            _handles = new[] { _cancellation.Token.WaitHandle };
            _thread.Start();
        }

        private Thread CreateThread()
        {
            return new Thread(() =>
            {
                if (_handles.Length <= 1)
                {
                    // stop thread if only handle is cancel token
                    return;
                }
                while (true)
                {
                    var i = WaitHandle.WaitAny(_handles);
                    switch (i)
                    {
                        case 0:
                            return;
                        default:
                            Task.Run(() =>
                            {
                                _actions[i].Invoke();
                            });
                            break;
                    }
                }
            })
            {
                Name = "Memory Share Event Thread",
                IsBackground = true,
            };
        }

        internal void AddToThread(SignaledMemoryObject o)
        {
            _semaphore.Wait();

            try
            {
                _actions = _actions.Concat(new[]
                {
                    o.OnUpdated,
                    o.OnUpdateRequested
                }).ToArray();
                _handles = _handles.Concat(new[]
                {
                    o.ObjectUpdatedHandle,
                    o.UpdateRequestedHandle
                }).ToArray();

                _cancellation.Cancel();
                _thread.Join();

                _cancellation = new CancellationTokenSource();
                _handles[0] = _cancellation.Token.WaitHandle;
                _thread = CreateThread();
                _thread.Start();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        internal void RemoveIfExists(SignaledMemoryObject o)
        {
            _semaphore.Wait();

            try
            {
                var updatedHandle = _handles.FindIndex(h => o.ObjectUpdatedHandle == h);
                var requestedHandle = _handles.FindIndex(h => o.UpdateRequestedHandle == h);
                _actions = _actions.Where((_, i) => i != updatedHandle && i != requestedHandle).ToArray();
                _handles = _handles.Where((_, i) => i != updatedHandle && i != requestedHandle).ToArray();

                _cancellation.Cancel();
                _thread.Join();

                _cancellation = new CancellationTokenSource();
                _handles[0] = _cancellation.Token.WaitHandle;
                _thread = CreateThread();
                _thread.Start();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        internal bool HasSpace(int handleCount)
        {
            return _handles.Length + handleCount < MaxHandles;
        }
    }
}