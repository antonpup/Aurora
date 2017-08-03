using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Aurora.Utils
{
	public sealed class ThreadPriorityChanger : IDisposable
	{
		private const int THREAD_PRIORITY_ERROR_RETURN = int.MaxValue;

		public enum ThreadPriority
		{
			THREAD_MODE_BACKGROUND_BEGIN = 0x00010000,
			THREAD_MODE_BACKGROUND_END = 0x00020000,
			THREAD_PRIORITY_ABOVE_NORMAL = 1,
			THREAD_PRIORITY_BELOW_NORMAL = -1,
			THREAD_PRIORITY_HIGHEST = 2,
			THREAD_PRIORITY_IDLE = -15,
			THREAD_PRIORITY_LOWEST = -2,
			THREAD_PRIORITY_NORMAL = 0,
			THREAD_PRIORITY_TIME_CRITICAL = 15
		}

		private const uint THREAD_SET_INFORMATION = 0x0020;
		private const uint THREAD_SET_LIMITED_INFORMATION = 0x0400;
		private const uint THREAD_QUERY_INFORMATION = 0x0040;
		private const uint THREAD_QUERY_LIMITED_INFORMATION = 0x0800;

		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern uint GetCurrentThreadId();

		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern IntPtr OpenThread(uint desiredAccess, bool inheritHandle, uint threadId);

		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern bool CloseHandle(IntPtr handle);

		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern bool SetThreadPriority(IntPtr hThread, ThreadPriority nPriority);

		[DllImport("kernel32.dll", SetLastError = true)]
		static extern ThreadPriority GetThreadPriority(IntPtr hThread);

		public static void SetCurrentThreadPriority(ThreadPriority priority)
		{
			IntPtr threadHandle = IntPtr.Zero;
			try
			{
				threadHandle = OpenThread(THREAD_SET_INFORMATION | THREAD_SET_LIMITED_INFORMATION, false, GetCurrentThreadId());
				if (!SetThreadPriority(threadHandle, priority))
				{
					var errorCode = Marshal.GetLastWin32Error();
					throw new Win32Exception(errorCode);
				}
			}
			finally
			{
				if (threadHandle != IntPtr.Zero)
				{
					CloseHandle(threadHandle);
				}
			}
		}

		private readonly System.Threading.ThreadPriority clrPriority;
		private readonly ThreadPriority windowsPriority;
		private readonly IntPtr threadHandle;

		public ThreadPriorityChanger(System.Threading.ThreadPriority clrPriority, ThreadPriority windowsPriority)
		{
			if (!Enum.IsDefined(typeof(ThreadPriority), windowsPriority))
				throw new ArgumentException(nameof(windowsPriority));

			try
			{
				Thread.BeginThreadAffinity();
				this.clrPriority = clrPriority;
				this.windowsPriority = windowsPriority;

				threadHandle = OpenThread(
					THREAD_SET_INFORMATION | 
					THREAD_SET_LIMITED_INFORMATION | 
					THREAD_QUERY_INFORMATION | 
					THREAD_QUERY_LIMITED_INFORMATION, false, GetCurrentThreadId());

				if (threadHandle == IntPtr.Zero)
					throw new Win32Exception(Marshal.GetLastWin32Error());
				
				this.windowsPriority = GetThreadPriority(threadHandle);
				if (this.windowsPriority == (ThreadPriority)THREAD_PRIORITY_ERROR_RETURN)
					throw new Win32Exception(Marshal.GetLastWin32Error());

				if (!SetThreadPriority(threadHandle, windowsPriority))
					throw new Win32Exception(Marshal.GetLastWin32Error());
			}
			catch
			{
				Dispose();
				throw;
			}
		}

		private bool disposed;

		private void ReleaseUnmanagedResources()
		{
			if (threadHandle != IntPtr.Zero)
			{
				CloseHandle(threadHandle);
			}
		}

		public void Dispose()
		{
			if (!disposed)
			{
				disposed = true;

				if (threadHandle != IntPtr.Zero)
				{
					SetThreadPriority(threadHandle, windowsPriority);
				}

				Thread.CurrentThread.Priority = clrPriority;
				Thread.EndThreadAffinity();
				GC.SuppressFinalize(this);
			}
		}

		~ThreadPriorityChanger()
		{
			ReleaseUnmanagedResources();
		}
	}
}
