using AngleSharp;
using AngleSharp.Dom;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Asdf.Services
{
	public class HtmlWatcherService : IHostedService
	{
		private readonly string _path;
		private readonly Dictionary<ConfigService.HtmlWatcherConfig, string> _cache = new();
		private readonly ILogger<HtmlWatcherService> _logger;
		private readonly ConfigService _config;
		private IBrowsingContext _context;
		private Timer _timer;

		public HtmlWatcherService(ILogger<HtmlWatcherService> logger, ConfigService config)
		{
			_path = Path.Combine(Path.GetDirectoryName(typeof(Startup).Assembly.Location), ".watcher.json");
			_logger = logger;
			_config = config;
		}

		public Task StartAsync(CancellationToken cancellationToken)
		{
			try
			{
				var json = File.ReadAllBytes(_path);
				var data = JsonSerializer.Deserialize<IEnumerable<WatcherCache>>(json);
				foreach (var item in data)
				{
					var watcher = _config.HtmlWatcher.FirstOrDefault(x => x.Url == item.Url);
					if (watcher != null)
					{
						_cache[watcher] = item.Value;
					}
				}
			}
			catch { }

			_context = BrowsingContext.New(Configuration.Default.WithDefaultLoader());
			_timer = new Timer(OnTimer, null, TimeSpan.Zero, TimeSpan.FromMinutes(_config.HtmlWatcherInterval));
			return Task.CompletedTask;
		}

		public async Task StopAsync(CancellationToken cancellationToken)
		{
			try { await _timer.DisposeAsync(); }
			catch { }
		}

		public void OnIntervalChanged()
		{
			var timeSpan = TimeSpan.FromMinutes(_config.HtmlWatcherInterval);
			_timer.Change(timeSpan, timeSpan);
		}

		private async void OnTimer(object state)
		{
			foreach (var watcher in _config.HtmlWatcher.Where(x => x.Enabled))
			{
				await GetAsync(watcher, false);
			}

			var data = _cache.Select(x => new WatcherCache(x.Key.Url, x.Value));
			var json = JsonSerializer.SerializeToUtf8Bytes(data, new JsonSerializerOptions { WriteIndented = true });
			File.WriteAllBytes(_path, json);
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
				Utils.ShowMessage("Error", $"Url: {watcher.Url}{Environment.NewLine}Exception: {ex}");
			}
		}

		private record WatcherCache(string Url, string Value);
	}
}