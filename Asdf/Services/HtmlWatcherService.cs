using AngleSharp;
using AngleSharp.Dom;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Asdf.Services
{
	public class HtmlWatcherService : IHostedService
	{
		private readonly Dictionary<ConfigService.HtmlWatcherConfig, string> _cache = new();
		private readonly ConfigService _config;
		private IBrowsingContext _context;
		private Timer _timer;

		public HtmlWatcherService(ConfigService config)
		{
			_config = config;
		}

		public Task StartAsync(CancellationToken cancellationToken)
		{
			_context = BrowsingContext.New(Configuration.Default.WithDefaultLoader());
			_timer = new Timer(OnTimer, null, TimeSpan.Zero, TimeSpan.FromSeconds(10 * 60));
			return Task.CompletedTask;
		}

		public async Task StopAsync(CancellationToken cancellationToken)
		{
			try { await _timer.DisposeAsync(); }
			catch { }
		}

		private async void OnTimer(object state)
		{
			foreach (var watcher in _config.HtmlWatcher.Where(x => x.Enabled))
			{
				try
				{
					var document = await _context.OpenAsync(watcher.Url);
					var element = document.QuerySelector(watcher.Selector);
					var result = element.Text().Trim();

					if (_cache.TryGetValue(watcher, out var prev) && prev != result)
					{
						ShowMessage(result);
					}
					_cache[watcher] = result;
				}
				catch (Exception ex)
				{
					ShowMessage(ex.ToString());
				}
			}
		}

		private void ShowMessage(string msg)
		{
		}
	}
}