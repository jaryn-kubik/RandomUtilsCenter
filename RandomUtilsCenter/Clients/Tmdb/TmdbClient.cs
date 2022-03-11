﻿using Microsoft.Extensions.Logging;
using RandomUtilsCenter.Clients;
using RandomUtilsCenter.Services;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace RandomUtilsCenter.Clients.Tmdb
{
	public class TmdbClient : BaseClient
	{
		public const string ApiUrl = "https://api.themoviedb.org";
		private static int _limitRemaining;
		private static long _limitReset;

		private readonly ConfigService _config;
		private readonly ILogger<TmdbClient> _log;

		public TmdbClient(HttpClient client, ConfigService config, ILogger<TmdbClient> log) : base(client)
		{
			_config = config;
			_log = log;
		}

		public Task<TmdbEpisodeResponse> GetEpisodeAsync(string id, string season, string episode)
		{
			return GetAsync<TmdbEpisodeResponse>($"3/tv/{id}/season/{season}/episode/{episode}");
		}

		public Task<TmdbShowResponse> GetShowAsync(string id)
		{
			return GetAsync<TmdbShowResponse>($"3/tv/{id}");
		}

		private async Task<TResponse> GetAsync<TResponse>(string url) where TResponse : TmdbResponse
		{
			if (_limitRemaining <= 0)
			{
				var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
				var wait = (_limitReset - timestamp) * 1000;
				if (wait > 0)
				{
					_log.LogWarning("Waiting for {0}s", wait);
					await Task.Delay((int)wait);
				}
			}

			var response = await GetAsync($"{url}?api_key={_config.TmdbApiKey}");
			if (!response.Headers.TryGetValues("X-RateLimit-Remaining", out var valuesRemaining) || !int.TryParse(valuesRemaining.FirstOrDefault(), out _limitRemaining))
			{
				_limitRemaining = 0;
			}
			if (!response.Headers.TryGetValues("X-RateLimit-Reset", out var valuesReset) || !long.TryParse(valuesReset.FirstOrDefault(), out _limitReset))
			{
				_limitReset = 0;
			}

			var result = await ReadAsJsonAsync<TResponse>(response);
			if (result.status_message == null)
			{
				return result;
			}
			throw new Exception($"{result.status_code}: {result.status_message}");
		}
	}
}