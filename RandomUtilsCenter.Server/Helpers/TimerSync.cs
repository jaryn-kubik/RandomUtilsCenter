using System;
using System.Threading;
using System.Threading.Tasks;

namespace RandomUtilsCenter.Server.Helpers
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

		public void Run(Action action, CancellationToken cancellationToken = default)
		{
			_sync.Wait(cancellationToken);
			try { action(); }
			finally { _sync.Release(); }
		}

		public T Run<T>(Func<T> action, CancellationToken cancellationToken = default)
		{
			_sync.Wait(cancellationToken);
			try { return action(); }
			finally { _sync.Release(); }
		}

		public async Task RunAsync(Action action, CancellationToken cancellationToken = default)
		{
			await _sync.WaitAsync(cancellationToken);
			try { action(); }
			finally { _sync.Release(); }
		}

		public async Task<T> RunAsync<T>(Func<T> action, CancellationToken cancellationToken = default)
		{
			await _sync.WaitAsync(cancellationToken);
			try { return action(); }
			finally { _sync.Release(); }
		}

		public async Task RunAsync(Func<Task> action, CancellationToken cancellationToken = default)
		{
			await _sync.WaitAsync(cancellationToken);
			try { await action(); }
			finally { _sync.Release(); }
		}

		public async Task<T> RunAsync<T>(Func<Task<T>> action, CancellationToken cancellationToken = default)
		{
			await _sync.WaitAsync(cancellationToken);
			try { return await action(); }
			finally { _sync.Release(); }
		}
	}
}