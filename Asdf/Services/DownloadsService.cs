using Asdf.Clients.JDownloader;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Asdf.Services
{
	public class DownloadsService
	{
		private readonly ConfigService _config;
		private readonly JDownloaderClient _client;

		public DownloadsService(ConfigService config, JDownloaderClient client)
		{
			_config = config;
			_client = client;
		}

		public async Task<bool> LoginAsync(string userName = null, string password = null)
		{
			if (userName != null && password != null)
			{
				_config.JDownloaderUserName = userName.ToLowerInvariant();
				_config.JDownloaderPassword = password;
				_config.Save();
			}
			if (_config.JDownloaderUserName == null || _config.JDownloaderPassword == null)
			{
				return false;
			}
			await _client.ConnectAsync();
			return true;
		}

		public IEnumerable<DownloadModel> GetItems()
		{
			yield break;
		}
	}
}