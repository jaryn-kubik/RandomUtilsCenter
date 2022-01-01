using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Asdf.Services
{
	public class DownloadsService
	{
		private const string _dir = "D:\\Downloads";
		private readonly DebridService _debrid;
		private readonly TorrentsService _torrents;
		private readonly HttpClient _client = new();
		private readonly ConcurrentDictionary<string, DownloadItem> _items = new(StringComparer.OrdinalIgnoreCase);
		private readonly ConcurrentDictionary<string, CancellationTokenSource> _tasks = new();

		public DownloadsService(DebridService debrid, TorrentsService torrents)
		{
			_debrid = debrid;
			_debrid.Changed += Update;
			_torrents = torrents;
		}

		public event Action Changed;

		public DownloadItem GetDownload(string link)
		{
			return _items.GetValueOrDefault(link);
		}

		public void Update()
		{
			foreach (var torrent in _torrents.Items.Values)
			{
				var token = new CancellationTokenSource();
				if (_tasks.TryAdd(torrent.Hash, token))
				{
					Task.Run(async () =>
					{
						try { await DownloadAsync(torrent.Hash, token.Token); }
						catch (Exception ex) { Utils.ShowMessage("Error - DownloadsService", ex.ToString()); }
						finally
						{
							_tasks.TryRemove(torrent.Hash, out _);
							try { token.Dispose(); }
							catch { }
						}
					}, token.Token);
				}
				else
				{
					token.Dispose();
				}
			}
			ClearTasks();
		}

		public void Clear()
		{
			Stop();
			_items.Clear();
		}

		public void Stop()
		{
			foreach (var task in _tasks.Values)
			{
				try { task.Cancel(); }
				catch { }
			}
			ClearTasks();
			foreach (var item in _items.Where(x => !x.Value.Done).Select(x => x.Key).ToArray())
			{
				_items.TryRemove(item, out _);
			}
		}

		private void ClearTasks()
		{
			foreach (var task in _tasks)
			{
				if (!_torrents.Items.TryGetValue(task.Key, out _) || task.Value.IsCancellationRequested)
				{
					try
					{
						if (_tasks.TryRemove(task.Key, out var token))
						{
							token.Cancel();
							token.Dispose();
						}
					}
					catch { }
				}
			}
		}

		public async Task DownloadAsync(string hash, CancellationToken cancellationToken = default)
		{
			var torrent = _debrid.GetTorrent(hash);
			if (torrent?.Links != null)
			{
				foreach (var link in torrent.Links)
				{
					await DownloadLinkAsync(link, cancellationToken);
				}
			}
		}

		private async Task DownloadLinkAsync(string link, CancellationToken cancellationToken)
		{
			var item = new DownloadItem();
			if (cancellationToken.IsCancellationRequested || !_items.TryAdd(link, item))
			{
				return;
			}

			try
			{
				var unresctrict = await _debrid.UnrestrictAsync(link);
				item.FileName = unresctrict.Filename;
				item.Size = unresctrict.Filesize;

				var fileInfo = new FileInfo(Path.Combine(_dir, item.FileName));
				if (fileInfo.Exists && fileInfo.Length == item.Size)
				{
					item.Done = true;
					item.Position = item.Size;
					Changed?.Invoke();
					return;
				}

				var request = new HttpRequestMessage(HttpMethod.Get, unresctrict.Download);
				var response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
				response.EnsureSuccessStatusCode();

				using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
				using var file = fileInfo.Create();

				var timestampStart = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
				var timestampPrev = timestampStart;
				var timestampPrevSpeed = timestampStart;
				var positionPrev = 0L;

				var buffer = new byte[81920];
				int bytesRead;
				while ((bytesRead = await stream.ReadAsync(buffer, cancellationToken)) > 0)
				{
					await file.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
					item.Position = file.Position;

					var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
					var diffTotal = Math.Max(timestamp - timestampStart, 1);
					item.SpeedTotal = item.Position / diffTotal;

					var diff = Math.Max(timestamp - timestampPrevSpeed, 1);
					item.SpeedCurrent = (item.Position - positionPrev) / diff;

					if (timestamp - timestampPrev >= 1)
					{
						timestampPrev = timestamp;
						Changed?.Invoke();
					}

					if (timestamp - timestampPrevSpeed >= 5)
					{
						timestampPrevSpeed = timestamp;
						positionPrev = item.Position;
					}
				}

				item.Done = true;
				Changed?.Invoke();
			}
			catch
			{
				_items.TryRemove(link, out _);
				if (!cancellationToken.IsCancellationRequested)
				{
					throw;
				}
			}
		}
	}
}