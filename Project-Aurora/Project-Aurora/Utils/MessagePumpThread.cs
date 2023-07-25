using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Aurora.Utils;

internal sealed class MessagePumpThread : IDisposable
{
	private Thread? _thread;
	private readonly TaskCompletionSource<Exception> _initResult = new();
	private ApplicationContext? _applicationContext;

	public void Start(Action init)
	{
		_thread = new Thread(() =>
		{
			try
			{
				using (_applicationContext = new ApplicationContext())
				{
					init();
				}
			}
			catch (Exception e)
			{
				if (!_initResult.TrySetResult(e))
				{
					Global.logger.Error("Exception in dedicated message pump thread. Exception: " + e);
				}
			}
		});
		_thread.SetApartmentState(ApartmentState.STA);
		_thread.Start();

		if (_initResult.Task.Result != null)
			throw _initResult.Task.Result;
	}

	public void EnterMessageLoop()
	{
		_initResult.SetResult(null);
		Application.Run(_applicationContext);
	}

	private bool _disposed;

	public void Dispose()
	{
		if (_disposed) return;
		_disposed = true;
		if (_applicationContext == null) return;
		_applicationContext.ExitThread();
		_thread.Join(TimeSpan.Zero);
		_applicationContext.Dispose();
	}
}