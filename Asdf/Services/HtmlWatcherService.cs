using AngleSharp;
using AngleSharp.Dom;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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
		private readonly ILogger<HtmlWatcherService> _logger;
		private readonly ConfigService _config;
		private IBrowsingContext _context;
		private Timer _timer;

		public HtmlWatcherService(ILogger<HtmlWatcherService> logger, ConfigService config)
		{
			_logger = logger;
			_config = config;
		}

		public Task StartAsync(CancellationToken cancellationToken)
		{
			_context = BrowsingContext.New(Configuration.Default.WithDefaultLoader());
			_timer = new Timer(OnTimer, null, TimeSpan.Zero, TimeSpan.FromMinutes(5));
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
				await GetAsync(watcher, false);
			}
		}

		public async Task GetAsync(ConfigService.HtmlWatcherConfig watcher, bool force)
		{
			try
			{
				if (force)
				{
					_cache[watcher] = null;
				}

				var document = await _context.OpenAsync(watcher.Url);
				var element = document.QuerySelector(watcher.Selector);
				var text = element.Text();
				var lines = text.Split('\n').Select(x => x?.Trim()).Where(x => !string.IsNullOrWhiteSpace(x));
				var result = string.Join(Environment.NewLine, lines);

				if (!_cache.TryGetValue(watcher, out var prev) || prev != result)
				{
					Utils.ShowMessage(watcher.Name, result);
				}
				_cache[watcher] = result;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"HtmlWatcherService: {watcher.Url}");
				Utils.ShowMessage("Error", ex.ToString());
			}
		}
	}
}