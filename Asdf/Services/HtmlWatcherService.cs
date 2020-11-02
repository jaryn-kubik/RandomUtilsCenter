using Microsoft.Extensions.Hosting;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Asdf.Services
{
	public class HtmlWatcherService : IHostedService
	{
		private readonly ConfigService _config;
		private Timer _timer;

		public HtmlWatcherService(ConfigService config)
		{
			_config = config;
		}

		public Task StartAsync(CancellationToken cancellationToken)
		{
			_timer = new Timer(OnTimer, null, TimeSpan.Zero, TimeSpan.FromSeconds(10));
			return Task.CompletedTask;
		}

		public async Task StopAsync(CancellationToken cancellationToken)
		{
			try { await _timer.DisposeAsync(); }
			catch { }
		}

		private void OnTimer(object state)
		{
			foreach (var watcher in _config.HtmlWatcher.Where(x => x.Enabled))
			{

			}
		}
	}
}