using System;
using System.IO;
using System.IO.Pipes;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading.Tasks;
using Aurora.Profiles;
using Common;

namespace Aurora.Modules.GameStateListen;

public class IpcListener
{
    private bool _isRunning;

    /// <summary>
    ///  Event for handing a newly received game state
    /// </summary>
    public event EventHandler<IGameState>? NewGameState;

    public event EventHandler<string>? WrapperConnectionClosed;

    public event EventHandler<string>? AuroraCommandReceived; 

    /// <summary>
    /// Returns whether or not the wrapper is connected through IPC
    /// </summary>
    public bool IsWrapperConnected { get; private set; }

    /// <summary>
    /// Returns the process of the wrapped connection
    /// </summary>
    public string WrappedProcess { get; private set; } = "";

    private NamedPipeServerStream? _ipcPipeStream;
    private NamedPipeServerStream? _auroraInterfacePipeStream;

    private static NamedPipeServerStream CreatePipe(string pipeName)
    {
        var securityIdentifier = new SecurityIdentifier(WellKnownSidType.WorldSid, null);

        var pipeSecurity = new PipeSecurity();
        pipeSecurity.AddAccessRule(new PipeAccessRule(securityIdentifier,
            PipeAccessRights.ReadWrite | PipeAccessRights.CreateNewInstance,
            AccessControlType.Allow));

        return NamedPipeServerStreamAcl.Create(
            pipeName, PipeDirection.In,
            NamedPipeServerStream.MaxAllowedServerInstances,
            PipeTransmissionMode.Message, PipeOptions.Asynchronous, 5 * 1024, 5 * 1024, pipeSecurity);
    }

    /// <summary>
    /// Starts listening for GameState requests
    /// </summary>
    public bool Start()
    {
        if (_isRunning) return false;
        _isRunning = true;

        BeginIpcServer();
        return true;
    }

    /// <summary>
    /// Stops listening for GameState requests
    /// </summary>
    public async Task Stop()
    {
        _isRunning = false;

        _ipcPipeStream.Close();
        _auroraInterfacePipeStream.Close();

        var ipcTask = _ipcPipeStream.DisposeAsync().AsTask()
            .ContinueWith(_ => _ipcPipeStream = null);
        var auroraTask = _auroraInterfacePipeStream.DisposeAsync().AsTask()
            .ContinueWith(_ => _auroraInterfacePipeStream = null);
        await Task.WhenAll(ipcTask, auroraTask);
    }

    private void BeginIpcServer()
    {
        IsWrapperConnected = false;
        WrappedProcess = "";

        _ipcPipeStream = CreatePipe("Aurora\\server");
        _ipcPipeStream.BeginWaitForConnection(ReceiveGameState, null);
        Global.logger.Information("[IPCServer] Pipe created {}", _ipcPipeStream?.GetHashCode() ?? -1);

        _auroraInterfacePipeStream = CreatePipe(Constants.AuroraInterfacePipe);
        _auroraInterfacePipeStream.BeginWaitForConnection(ReceiveAuroraCommand, null);
        Global.logger.Information("[AuroraCommandsServerIPC] Pipe created {}", _auroraInterfacePipeStream?.GetHashCode() ?? -1);
    }

    private void ReceiveGameState(IAsyncResult result)
    {
        if (!_isRunning)
        {
            return;
        }
        Global.logger.Information("[IPCServer] Pipe connection established");

        try
        {
            using var sr = new StreamReader(_ipcPipeStream);
            while (sr.ReadLine() is { } temp)
            {
                try
                {
                    var newState = new GameState_Wrapper(temp); //GameState_Wrapper 

                    IsWrapperConnected = true;
                    WrappedProcess = newState.Provider.Name.ToLowerInvariant();
                    NewGameState?.Invoke(this, newState);
                }
                catch (Exception exc)
                {
                    Global.logger.Error(exc, "[IPCServer] ReceiveGameState Exception, ");
                    Global.logger.Information("Received data that caused error:\n\r{Data}", temp);
                }
            }
        }
        finally
        {
            WrapperConnectionClosed?.Invoke(this, WrappedProcess);
            IsWrapperConnected = false;
            WrappedProcess = "";
        }
        if (!_isRunning)
        {
            return;
        }
        //run in another thread to reset stack
        Task.Run(() =>
        {
            _ipcPipeStream = CreatePipe("Aurora\\server");
            _ipcPipeStream.BeginWaitForConnection(ReceiveGameState, null);
        });
    }

    private void ReceiveAuroraCommand(IAsyncResult result)
    {
        if (!_isRunning)
        {
            return;
        }
        Global.logger.Information("[AuroraCommandsServerIPC] Pipe connection established");

        using var sr = new StreamReader(_auroraInterfacePipeStream);
        while (sr.ReadLine() is { } command)
        {
            AuroraCommandReceived?.Invoke(this, command);
        }
        if (!_isRunning)
        {
            return;
        }
        _auroraInterfacePipeStream = CreatePipe(Constants.AuroraInterfacePipe);
        _auroraInterfacePipeStream.BeginWaitForConnection(ReceiveAuroraCommand, null);
    }
}