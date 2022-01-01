using RDNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Asdf.Services
{
	public class DebridService
	{
		private readonly RdNetClient _client;
		private readonly TimerSync _sync;
		private readonly SemaphoreSlim _clientSync = new(1, 1);

		public DebridService()
		{
			_client = new RdNetClient();
			_client.UseApiAuthentication("H6OPNEATJZCRJVGCBMVNTUZUSEGQ3JEYLW4JFGSR4Q6LMC7F5LLQ");
			_sync = new(TimeSpan.FromSeconds(5), ReloadAsync);
		}

		public IList<Torrent> Items { get; private set; } = Array.Empty<Torrent>();
		public event Action Changed;

		public IEnumerable<Torrent> GetTorrents(string hash)
		{
			return Items.Where(x => string.Equals(x.Hash, hash, StringComparison.OrdinalIgnoreCase));
		}

		public Torrent GetTorrent(string hash)
		{
			return GetTorrents(hash).FirstOrDefault();
		}

		public async Task ReloadAsync()
		{
			await _sync.Run(async () =>
			{
				Items = (await _client.Torrents.GetAsync(null, 100));
				if (Items.Count == 0 || Items.All(x => x.Status == "downloaded"))
				{
					_sync.Stop();
				}
				else
				{
					_sync.Start();
				}
			});
			Changed?.Invoke();
		}

		public async Task AddMagnetAsync(string hash, string magnet)
		{
			await ReloadAsync();
			if (!GetTorrents(hash).Any())
			{
				var result = await _client.Torrents.AddMagnetAsync(magnet);
				var torrent = await _client.Torrents.GetInfoAsync(result.Id);
				if (torrent.Status == "waiting_files_selection")
				{
					var files = torrent.Files.Where(x =>
						x.Bytes > 1024 &&
						!x.Path.EndsWith(".txt", StringComparison.OrdinalIgnoreCase) &&
						!x.Path.EndsWith(".nfo", StringComparison.OrdinalIgnoreCase) &&
						!x.Path.EndsWith(".srt", StringComparison.OrdinalIgnoreCase) &&
						!x.Path.EndsWith(".sub", StringComparison.OrdinalIgnoreCase) &&
						!x.Path.EndsWith(".ass", StringComparison.OrdinalIgnoreCase)
					);
					await _client.Torrents.SelectFilesAsync(result.Id, files.Select(x => x.Id.ToString()).ToArray());
				}
			}
			await ReloadAsync();
		}

		public async Task DeleteAsync(string hash)
		{
			foreach (var torrent in GetTorrents(hash).ToArray())
			{
				await _client.Torrents.DeleteAsync(torrent.Id);
			}
			await ReloadAsync();
		}

		public async Task<UnrestrictLink> UnrestrictAsync(string link)
		{
			await _clientSync.WaitAsync();
			try { return await _client.Unrestrict.LinkAsync(link); }
			finally { _clientSync.Release(); }
		}
	}
}