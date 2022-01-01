using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Asdf.Services
{
	public class TorrentsService
	{
		private readonly JsonHelper<List<TorrentItem>> _list;
		private readonly DebridService _debrid;

		public TorrentsService(ClipboardService clipboard, DebridService debrid)
		{
			_list = JsonHelper<List<TorrentItem>>.Load("torrents");
			clipboard.Changed += Clipboard_ChangedAsync;
			_debrid = debrid;
		}

		public IReadOnlyList<TorrentItem> Items => _list.Instance;
		public event Action Changed;

		public async Task DeleteAsync(TorrentItem item)
		{
			await _debrid.DeleteAsync(item.Hash);
			_list.Instance.Remove(item);
			_list.Save();
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

			var item = _list.Instance.FirstOrDefault(x =>
				(!string.IsNullOrWhiteSpace(x.Name) && x.Name == name) ||
				(!string.IsNullOrWhiteSpace(x.Hash) && x.Hash == hash) ||
				(!string.IsNullOrWhiteSpace(x.Magnet) && x.Magnet == magnet)
			);
			if (item == null)
			{
				item = new TorrentItem();
				_list.Instance.Add(item);
			}

			item.Name = name ?? item.Name;
			item.Hash = hash ?? item.Hash;
			item.Magnet = magnet ?? item.Magnet;
			_list.Save();
			return item;
		}
	}
}