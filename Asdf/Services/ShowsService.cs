using Asdf.Clients;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Asdf.Services
{
	public class ShowsService
	{
		private readonly SimklClient _simkl;
		private readonly TmdbClient _tmbd;
		private static IEnumerable<ShowModel> _shows;

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

		public async Task<IEnumerable<ShowModel>> GetShowsAsync()
		{
			if (_shows == null)
			{
				var shows = new List<ShowModel>();
				var items = await _simkl.GetShows();
				foreach (var item in items.shows)
				{
					if (item.next_to_watch != null)
					{
						var episode = Regex.Match(item.next_to_watch, @"S(\d+)E(\d+)", RegexOptions.IgnoreCase);
						var idSeason = episode.Groups[1].Value;
						var idEpisode = episode.Groups[2].Value;
						var episodeInfo = await _tmbd.GetEpisodeAsync(item.show.ids.tmdb, idSeason, idEpisode);
						shows.Add(new ShowModel
						{
							ShowTitle = item.show.title,
							EpisodeNumber = item.next_to_watch,
							EpisodeTitle = episodeInfo.name,
							EpisodeDate = DateTime.Parse(episodeInfo.air_date).AddDays(1)
						});
					}
					else if (item.total_episodes_count > item.watched_episodes_count)
					{
						var showInfo = await _tmbd.GetShowAsync(item.show.ids.tmdb);
						if (showInfo.next_episode_to_air != null)
						{
							shows.Add(new ShowModel
							{
								ShowTitle = item.show.title,
								EpisodeNumber = $"S{showInfo.next_episode_to_air.season_number:D2}E{showInfo.next_episode_to_air.episode_number:D2}",
								EpisodeTitle = showInfo.next_episode_to_air.name,
								EpisodeDate = DateTime.Parse(showInfo.next_episode_to_air.air_date).AddDays(1)
							});
						}
					}
				}
				shows.Sort((x, y) => x.EpisodeDate.CompareTo(y.EpisodeDate));
				_shows = shows;
			}
			return _shows;
		}
	}
}