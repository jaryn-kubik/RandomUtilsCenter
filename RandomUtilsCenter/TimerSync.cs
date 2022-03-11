using System;
using System.Threading;
using System.Threading.Tasks;

namespace RandomUtilsCenter
{
	public class TimerSync
	{
		private readonly SemaphoreSlim _sync = new(1, 1);
		private readonly TimeSpan _interval;
		private readonly Func<Task> _action;
		private PeriodicTimer _timer;

		public TimerSync(TimeSpan interval, Func<Task> action)
		{
			_interval = interval;
			_action = action;
		}

		public void Start()
		{
			Stop();
			var timer = new PeriodicTimer(_interval);
			_timer = timer;
			Task.Run(async void () =>
			{
				while (await timer.WaitForNextTickAsync())
				{
					try
					{
						await _action();
					}
					catch (Exception ex)
					{
						Utils.ShowMessage("Error - TimerSync", ex.ToString());
					}
				}
			});
		}

		public void Stop()
		{
			try
			{
				_timer?.Dispose();
				_timer = null;
			}
			catch { }
		}

		public void Run(Action action)
		{
			_sync.Wait();
			try { action(); }
			finally { _sync.Release(); }
		}

		public T Run<T>(Func<T> action)
		{
			_sync.Wait();
			try { return action(); }
			finally { _sync.Release(); }
		}

		public async Task Run(Func<Task> action)
		{
			await _sync.WaitAsync();
			try { await action(); }
			finally { _sync.Release(); }
		}

		public async Task<T> Run<T>(Func<Task<T>> action)
		{
			await _sync.WaitAsync();
			try { return await action(); }
			finally { _sync.Release(); }
		}
	}
}