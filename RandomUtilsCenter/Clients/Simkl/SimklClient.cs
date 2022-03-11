﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using RandomUtilsCenter;
using RandomUtilsCenter.Clients;
using RandomUtilsCenter.Services;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace RandomUtilsCenter.Clients.Simkl
{
	public class SimklClient : BaseClient
	{
		public const string ApiUrl = "https://api.simkl.com";

		private readonly LinkGenerator _linkGenerator;
		private readonly IHttpContextAccessor _httpContextAccessor;
		private readonly ConfigService _config;

		public SimklClient(HttpClient client, LinkGenerator linkGenerator, IHttpContextAccessor httpContextAccessor, ConfigService config) : base(client)
		{
			_linkGenerator = linkGenerator;
			_httpContextAccessor = httpContextAccessor;
			_config = config;
		}

		public async Task LoginAsync(string code)
		{
			var request = new SimklAuthRequest
			{
				code = code,
				client_id = _config.SimklClientID,
				client_secret = _config.SimklClientSecret,
				redirect_uri = GetRedirectUrl(),
				grant_type = "authorization_code"
			};

			var result = await PostAsync<SimklAuthResponse, SimklAuthRequest>("oauth/token", request);
			_config.SimklToken = result.access_token;
			_config.Save();
		}

		public Task<SimklShowsResponse> GetShowsAsync()
		{
			return PostAsync<SimklShowsResponse>("sync/all-items/shows/watching");
		}

		public Task WatchShowAsync(string title, int id, int season, int episode)
		{
			var request = new SimklWatchRequest
			{
				shows = new[]
				{
					new SimklWatchRequestItem
					{
						title = title,
						ids = new SimklWatchRequestItemIds { simkl = id },
						seasons = new []
						{
							new SimklWatchRequestItemSeasons
							{
								number = season,
								episodes = new [] { new SimklWatchRequestItemEpisode { number = episode} }
							}
						}
					}
				}
			};
			return PostAsync<SimklResponse, SimklWatchRequest>("sync/history", request);
		}

		public string GetLoginUrl()
		{
			return _config.SimklToken == null
				? $"https://simkl.com/oauth/authorize?response_type=code&client_id={_config.SimklClientID}&redirect_uri={GetRedirectUrl()}"
				: null;
		}

		private string GetRedirectUrl()
		{
			return _linkGenerator.GetUriByAction(_httpContextAccessor.HttpContext, nameof(ApiController.Simkl), ApiController.Name);
		}

		private async Task<TResponse> PostAsync<TResponse>(string url) where TResponse : SimklResponse
		{
			return await PostAsyncInt<TResponse>(() => PostAsync(url));
		}

		private async Task<TResponse> PostAsync<TResponse, TRequest>(string url, TRequest request) where TResponse : SimklResponse
		{
			return await PostAsyncInt<TResponse>(() => PostJsonAsync(url, request));
		}

		private async Task<TResponse> PostAsyncInt<TResponse>(Func<Task<HttpResponseMessage>> action) where TResponse : SimklResponse
		{
			if (_config.SimklToken != null)
			{
				Headers.Authorization = new AuthenticationHeaderValue("Bearer", _config.SimklToken);
				Headers.Remove("simkl-api-key");
				Headers.Add("simkl-api-key", _config.SimklClientID);
			}

			var response = await action();
			var result = await ReadAsJsonAsync<TResponse>(response);

			if (result.error == null)
			{
				return result;
			}
			throw new Exception($"{result.error}: {result.message}");
		}
	}
}