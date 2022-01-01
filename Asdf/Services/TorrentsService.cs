using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Asdf.Services
{
	public class TorrentsService
	{
		private readonly JsonHelper<ConcurrentDictionary<string, TorrentItem>> _items;
		private readonly DebridService _debrid;

		public TorrentsService(ClipboardService clipboard, DebridService debrid)
		{
			_items = JsonHelper<ConcurrentDictionary<string, TorrentItem>>.Load("torrents", x => new ConcurrentDictionary<string, TorrentItem>(x, StringComparer.OrdinalIgnoreCase));
			clipboard.Changed += Clipboard_ChangedAsync;
			_debrid = debrid;
		}

		public IReadOnlyDictionary<string, TorrentItem> Items => _items.Instance;
		public event Action Changed;

		public async Task DeleteAllAsync()
		{
			foreach (var item in Items.Keys.ToArray())
			{
				await DeleteAsync(item);
			}
		}

		public async Task DeleteAsync(string hash)
		{
			_items.Instance.TryRemove(hash, out _);
			await _debrid.DeleteAsync(hash);
			_items.Save();
		}

		private async void Clipboard_ChangedAsync(string text)
		{
			text = text?.Trim() ?? string.Empty;
			if (text.StartsWith("magnet:?"))
			{
				var parts = text.Substring("magnet:?".Length).Split('&', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
				var name = HttpUtility.UrlDecode(parts.FirstOrDefault(x => x.StartsWith("dn="))?.Substring("dn=".Length));
				var hash = parts.FirstOrDefault(x => x.StartsWith("xt="));
				hash = hash?.Substring(hash.LastIndexOf(':') + 1);
				Add(name, hash, text);
				await _debrid.AddMagnetAsync(hash, text);
				Changed?.Invoke();
			}
		}

		private TorrentItem Add(string name, string hash, string magnet)
		{
			name = name?.Trim();
			hash = hash?.Trim();
			magnet = magnet?.Trim();

			var item = _items.Instance.GetOrAdd(hash, new TorrentItem { Hash = hash });
			item.Name = name ?? item.Name;
			item.Magnet = magnet ?? item.Magnet;
			_items.Save();
			return item;
		}
	}
}