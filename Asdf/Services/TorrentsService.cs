using AngleSharp;
using AngleSharp.Dom;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Asdf.Services
{
	public class TorrentsService
	{
		private readonly JsonHelper<List<TorrentItem>> _list;
		private readonly IBrowsingContext _context;

		public TorrentsService(ClipboardService clipboard)
		{
			_list = JsonHelper<List<TorrentItem>>.Load("torrents");
			_context = BrowsingContext.New(Configuration.Default.WithDefaultLoader());
			clipboard.Changed += Clipboard_ChangedAsync;
		}

		private TorrentItem Add(string name, string url, string magnet)
		{
			name = name?.Trim();
			url = url?.Trim();
			magnet = magnet?.Trim();

			var item = _list.Instance.FirstOrDefault(x =>
				(!string.IsNullOrWhiteSpace(x.Name) && x.Name == name) ||
				(!string.IsNullOrWhiteSpace(x.Url) && x.Url == url) ||
				(!string.IsNullOrWhiteSpace(x.Magnet) && x.Magnet == magnet)
			);
			if (item == null)
			{
				item = new TorrentItem();
				_list.Instance.Add(item);
				_list.Instance.Sort((x, y) => string.Compare(x.Name, y.Name));
			}

			item.Name = name ?? item.Name;
			item.Url = url ?? item.Url;
			item.Magnet = magnet ?? item.Magnet;
			_list.Save();
			return item;
		}

		private async void Clipboard_ChangedAsync(string text)
		{
			text = text?.Trim() ?? string.Empty;
			if (text.StartsWith("https://rarbgtorrents.org/torrent/"))
			{
				var document = await _context.OpenAsync(text);
				var elementName = document.QuerySelector("h1");
				var elementMagnet = document.QuerySelector("a[href^=m]");

				var name = elementName?.Text()?.Trim();
				var magnet = elementMagnet?.GetAttribute("href")?.Trim();

				Add(name, text, magnet);
			}
			else if (text.StartsWith("magnet:?"))
			{
				var parts = text.Split('&', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
				var name = parts.FirstOrDefault(x => x.StartsWith("dn="))?.Substring("dn=".Length);
				Add(name, null, text);
			}
		}
	}
}