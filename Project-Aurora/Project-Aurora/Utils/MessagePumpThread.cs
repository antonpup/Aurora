using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Aurora.Utils
{
	internal sealed class MessagePumpThread : IDisposable
	{
		private Thread thread;
		private readonly TaskCompletionSource<Exception> initResult = new();
		private ApplicationContext applicationContext;

		public void Start(Action init)
		{
			thread = new Thread(() =>
			{
				try
				{
					using (applicationContext = new ApplicationContext())
					{
						init();
					}
				}
				catch (Exception e)
				{
					if (!initResult.TrySetResult(e))
					{
						Global.logger.Error("Exception in dedicated message pump thread. Exception: " + e);
					}
				}
			});
			thread.SetApartmentState(ApartmentState.STA);
			thread.Start();

			if (initResult.Task.Result != null)
				throw initResult.Task.Result;
		}

		public void EnterMessageLoop()
		{
			initResult.SetResult(null);
			Application.Run(applicationContext);
		}

		private bool disposed;

		public void Dispose()
		{
			if (!disposed)
			{
				disposed = true;
				if (applicationContext != null)
				{
					applicationContext.ExitThread();
					thread.Join(TimeSpan.Zero);
					applicationContext.Dispose();
				}
			}
		}
	}
}