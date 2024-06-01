using RDNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RandomUtilsCenter.Services
{
    public class DebridService
    {
        private readonly ConfigService _config;
        private readonly TimerSync _sync;
        private readonly SemaphoreSlim _clientSync = new(1, 1);
        private RdNetClient _client;

        public DebridService(ConfigService config)
        {
            _config = config;
            _sync = new(TimeSpan.FromSeconds(5), ReloadAsync);
        }

        private bool InitClient()
        {
            if (_client == null && string.IsNullOrWhiteSpace(_config.RealDebridApiKey))
            {
                return false;
            }
            _client = new RdNetClient();
            _client.UseApiAuthentication(_config.RealDebridApiKey);
            return true;
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
            if (InitClient())
            {
                await _sync.RunAsync(async () =>
                {
                    Items = await _client.Torrents.GetAsync(null, 100);
                    foreach (var torrent in Items)
                    {
                        await SelectFilesAsync(torrent);
                    }

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
        }

        public async Task AddMagnetAsync(string hash, string magnet)
        {
            if (InitClient())
            {
                await ReloadAsync();
                if (!GetTorrents(hash).Any())
                {
                    var result = await _client.Torrents.AddMagnetAsync(magnet);
                    var torrent = await _client.Torrents.GetInfoAsync(result.Id);
                    await SelectFilesAsync(torrent);
                }
                await ReloadAsync();
            }
        }

        public async Task DeleteAsync(string hash)
        {
            if (InitClient())
            {
                foreach (var torrent in GetTorrents(hash).ToArray())
                {
                    await _client.Torrents.DeleteAsync(torrent.Id);
                }
                await ReloadAsync();
            }
        }

        public async Task<UnrestrictLink> UnrestrictAsync(string link)
        {
            if (InitClient())
            {
                await _clientSync.WaitAsync();
                try { return await _client.Unrestrict.LinkAsync(link); }
                finally { _clientSync.Release(); }
            }
            return new UnrestrictLink();
        }

        private async Task SelectFilesAsync(Torrent torrent)
        {
            if (torrent.Status == "waiting_files_selection")
            {
                if (torrent.Files is null)
                {
                    torrent = await _client.Torrents.GetInfoAsync(torrent.Id);
                }
                var files = torrent.Files
                    ?.Where(x =>
                        x.Bytes > 1024 &&
                       !x.Path.EndsWith(".txt", StringComparison.OrdinalIgnoreCase) &&
                       !x.Path.EndsWith(".nfo", StringComparison.OrdinalIgnoreCase) &&
                       !x.Path.EndsWith(".srt", StringComparison.OrdinalIgnoreCase) &&
                       !x.Path.EndsWith(".sub", StringComparison.OrdinalIgnoreCase) &&
                       !x.Path.EndsWith(".ass", StringComparison.OrdinalIgnoreCase)
                    )
                    ?.Select(x => x?.Id.ToString())
                    ?.ToArray();
                if (files?.Length > 0)
                {
                    await _client.Torrents.SelectFilesAsync(torrent.Id, files);
                }
            }
        }
    }
}