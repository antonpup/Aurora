using System;
using System.IO;
using System.IO.Pipes;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading.Tasks;
using Aurora.Profiles;

namespace Aurora.Utils;

public delegate void WrapperConnectionClosedHandler(string process);

public class IpcListener
{
    private bool _isRunning;

    /// <summary>
    ///  Event for handing a newly received game state
    /// </summary>
    public event NewGameStateHandler NewGameState = delegate { };

    public event WrapperConnectionClosedHandler WrapperConnectionClosed = delegate { };

    /// <summary>
    /// Returns whether or not the wrapper is connected through IPC
    /// </summary>
    public bool IsWrapperConnected { get; private set; }

    /// <summary>
    /// Returns the process of the wrapped connection
    /// </summary>
    public string WrappedProcess { get; private set; } = "";

    private NamedPipeServerStream _ipcPipeStream;

    private static NamedPipeServerStream CreatePipe()
    {
        var securityIdentifier = new SecurityIdentifier(WellKnownSidType.WorldSid, null);

        var pipeSecurity = new PipeSecurity();
        pipeSecurity.AddAccessRule(new PipeAccessRule(securityIdentifier,
            PipeAccessRights.ReadWrite | PipeAccessRights.CreateNewInstance,
            AccessControlType.Allow));

        return NamedPipeServerStreamAcl.Create(
            "Aurora\\server", PipeDirection.In,
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
        await _ipcPipeStream.DisposeAsync();
        _ipcPipeStream = null;
    }

    private void BeginIpcServer()
    {
        IsWrapperConnected = false;
        WrappedProcess = "";
        Global.logger.Info("[IPCServer] Pipe created {}", _ipcPipeStream?.GetHashCode() ?? -1);

        _ipcPipeStream = CreatePipe();
        _ipcPipeStream.BeginWaitForConnection(ReceiveGameState, null);
    }

    private void ReceiveGameState(IAsyncResult result)
    {
        if (!_isRunning)
        {
            return;
        }
        Global.logger.Info("[IPCServer] Pipe connection established");

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
                    NewGameState?.Invoke(newState);
                }
                catch (Exception exc)
                {
                    Global.logger.Error("[IPCServer] HandleNewIPCGameState Exception, " + exc);
                    Global.logger.Info("Recieved data that caused error:\n\r" + temp);
                }
            }
        }
        finally
        {
            WrapperConnectionClosed?.Invoke(WrappedProcess);
            IsWrapperConnected = false;
            WrappedProcess = "";
        }
        if (!_isRunning)
        {
            return;
        }
        _ipcPipeStream = CreatePipe();
        _ipcPipeStream.BeginWaitForConnection(ReceiveGameState, null);
    }
}