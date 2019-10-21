using Asdf.Clients;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Asdf.Services
{
	public class ShowsService
	{
		private static readonly MemoryCache _episodeInfo = new MemoryCache(new MemoryCacheOptions());
		private static readonly MemoryCache _showInfo = new MemoryCache(new MemoryCacheOptions());
		private static readonly List<ShowModel> _shows = new List<ShowModel>();

		private readonly SimklClient _simkl;
		private readonly TmdbClient _tmbd;

		public ShowsService(SimklClient simkl, TmdbClient tmbd)
		{
			_simkl = simkl;
			_tmbd = tmbd;
		}

		public string Login()
		{
			return _simkl.GetLoginUrl();
		}

		public Task LoginAsync(string code)
		{
			return _simkl.LoginAsync(code);
		}

		public async Task<IEnumerable<ShowModel>> GetShowsAsync(bool force = false)
		{
			if (force)
			{
				_shows.Clear();
			}

			if (_shows.Count > 0)
			{
				return _shows;
			}

			var items = await _simkl.GetShowsAsync();
			foreach (var item in items.shows)
			{
				if (item.next_to_watch != null)
				{
					var episode = Regex.Match(item.next_to_watch, @"S(\d+)E(\d+)", RegexOptions.IgnoreCase);
					var idSeason = episode.Groups[1].Value;
					var idEpisode = episode.Groups[2].Value;

					var episodeInfo = await _episodeInfo.GetOrCreateAsync($"{item.show.ids.simkl}S{idSeason:D2}E{idEpisode:D2}", _ => _tmbd.GetEpisodeAsync(item.show.ids.tmdb, idSeason, idEpisode));
					_shows.Add(new ShowModel
					{
						Id = item.show.ids.simkl,
						ShowTitle = item.show.title,
						EpisodeTitle = episodeInfo.name,

						Season = episodeInfo.season_number,
						Episode = episodeInfo.episode_number,

						Date = DateTime.Parse(episodeInfo.air_date).AddDays(1),
						IsWatchable = true
					});
				}
				else if (item.total_episodes_count > item.watched_episodes_count)
				{
					var showInfo = await _showInfo.GetOrCreateAsync(item.show.ids.simkl, x =>
					{
						x.SetAbsoluteExpiration(TimeSpan.FromHours(1));
						return _tmbd.GetShowAsync(item.show.ids.tmdb);
					});

					if (showInfo.next_episode_to_air != null)
					{
						_shows.Add(new ShowModel
						{
							Id = item.show.ids.simkl,
							ShowTitle = item.show.title,
							EpisodeTitle = showInfo.next_episode_to_air.name,

							Season = showInfo.next_episode_to_air.season_number,
							Episode = showInfo.next_episode_to_air.episode_number,

							Date = DateTime.Parse(showInfo.next_episode_to_air.air_date).AddDays(1)
						});
					}
				}
			}
			_shows.Sort((x, y) => x.Date.CompareTo(y.Date));
			return _shows;
		}

		public Task WatchAsync(ShowModel show)
		{
			return _simkl.WatchShowAsync(show.ShowTitle, show.Id, show.Season, show.Episode);
		}
	}
}