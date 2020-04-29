using Asdf.Services;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Asdf.Clients.AllDebrid
{
	public class AllDebridClient : BaseClient
	{
		public const string ApiUrl = "https://api.alldebrid.com";
		private const string Agent = "notificationCenter";

		private static string _check_url;
		private readonly ConfigService _config;

		public AllDebridClient(HttpClient client, ConfigService config) : base(client)
		{
			_config = config;
		}

		public async Task<string> LoginAsync()
		{
			if (_config.AllDebridToken != null)
			{
				return null;
			}

			if (_check_url != null)
			{
				var check = await GetAsync<AllDebridLoginCheckResponse>(_check_url);
				if (check.apikey != null)
				{
					_config.AllDebridToken = check.apikey;
					_config.Save();
					return null;
				}
			}

			var result = await GetAsync<AllDebridLoginResponse>($"v4/pin/get?agent={Agent}");
			_check_url = result.check_url;
			return result.user_url;
		}

		public Task<AllDebridTorrentsResponse> GetTorrentsAsync()
		{
			return GetAsync<AllDebridTorrentsResponse>($"v4/magnet/status?agent={Agent}&apikey={_config.AllDebridToken}");
		}

		public Task DeleteAsync(long id)
		{
			return GetAsync<object>($"v4/magnet/delete?agent={Agent}&apikey={_config.AllDebridToken}&id={id}");
		}

		private async Task<TResponse> GetAsync<TResponse>(string url)
		{
			var response = await GetAsync(url);
			var result = await ReadAsJsonAsync<AllDebridResponse<TResponse>>(response);
			if (result.status == "success" || result.data != null)
			{
				return result.data;
			}
			throw new Exception($"{result.error.code}: {result.error.message}");
		}
	}
}