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
				if (check.token != null)
				{
					_config.AllDebridToken = check.token;
					_config.Save();
					return null;
				}
			}

			var result = await GetAsync<AllDebridLoginResponse>($"pin/get?agent={Agent}");
			_check_url = result.check_url;
			return result.user_url;
		}

		public Task<AllDebridTorrentsResponse> GetTorrentsAsync()
		{
			return GetAsync<AllDebridTorrentsResponse>($"magnet/status?agent={Agent}&token={_config.AllDebridToken}&apiVersion=2", 55);
		}

		public Task DeleteAsync(long id)
		{
			return GetAsync<AllDebridResponse>($"magnet/delete?agent={Agent}&token={_config.AllDebridToken}&id={id}");
		}

		private async Task<TResponse> GetAsync<TResponse>(string url, int? ignoreError = null) where TResponse : AllDebridResponse
		{
			var response = await GetAsync(url);
			var result = await ReadAsJsonAsync<TResponse>(response);
			if (result.error == null || result.errorCode == ignoreError)
			{
				return result;
			}
			throw new Exception($"{result.errorCode}: {result.error}");
		}
	}
}