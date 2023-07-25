using System;
using System.IO.Pipes;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;

namespace Aurora.Modules.Logitech;

public sealed class PipeListener : IDisposable
{
    private const int BufferSize = 5 * 1024;
    public event EventHandler? ClientConnected;
    public event EventHandler? ClientDisconnected;
    public event EventHandler<ReadOnlyMemory<byte>>? CommandReceived;
    
    private readonly string _pipeName;
    private readonly CancellationTokenSource _tokenSource;

    private NamedPipeServerStream? _pipeStream;
    
    private readonly byte[] _headerBuffer = new byte[4];
    private readonly byte[] _buffer = new byte[BufferSize];

    public PipeListener(string pipeName)
    {
        _pipeName = pipeName;
        _tokenSource = new();
    }

    public void StartListening()
    {
        _pipeStream = CreatePipe(_pipeName);
        _pipeStream.BeginWaitForConnection(ReceiveAuroraCommand, null);
    }
    
    private void ReceiveAuroraCommand(IAsyncResult result)
    {
        if (_tokenSource.IsCancellationRequested || _pipeStream == null)
        {
            return;
        }
        ClientConnected?.Invoke(this, EventArgs.Empty);

        while (_pipeStream.IsConnected && _pipeStream.Read(_headerBuffer) == sizeof(int))
        {
            var dataLength = BitConverter.ToInt32(_headerBuffer) - sizeof(int);

            var position = 0;
            do
            {
                var bytesRead = _pipeStream.Read(_buffer.AsSpan(position, dataLength - position));
                if (bytesRead == 0)
                    return;
                position += bytesRead;
                
            } while (position < dataLength);

            var actualData = _buffer.AsMemory(0, dataLength);
            CommandReceived?.Invoke(this, actualData);
        }

        ClientDisconnected?.Invoke(this, EventArgs.Empty);
        _pipeStream = CreatePipe(_pipeName);
        _pipeStream.BeginWaitForConnection(ReceiveAuroraCommand, null);
    }
    
    private static NamedPipeServerStream CreatePipe(string pipeName)
    {
        var securityIdentifier = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
        
        PipeAccessRule rule = new(
            securityIdentifier,
            PipeAccessRights.FullControl,
            AccessControlType.Allow
            );

        var pipeSecurity = new PipeSecurity();
        pipeSecurity.SetAccessRule(rule);

        return NamedPipeServerStreamAcl.Create(
            pipeName, PipeDirection.InOut,
            -1,
            PipeTransmissionMode.Message,
            PipeOptions.Asynchronous,
            BufferSize,
            BufferSize,
            pipeSecurity);
    }

    #region IDisposable

    private bool _disposedValue;

    private void Dispose(bool disposing)
    {
        if (_disposedValue) return;
        if (disposing)
        {
            _tokenSource.Cancel();
            try
            {
                //dummy client to unblock the server
                using NamedPipeClientStream pipeStream = new(_pipeName);
                pipeStream.Connect(100);
                pipeStream.Close();
            }
            catch { /* ignored */ }

            _tokenSource.Dispose();
        }

        // TODO: free unmanaged resources (unmanaged objects) and override finalizer
        // TODO: set large fields to null
        _disposedValue = true;
    }

    public void Dispose()
    {
        Dispose(disposing: true);
    }

    #endregion
}