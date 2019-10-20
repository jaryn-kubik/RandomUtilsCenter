using System;
using System.Collections.Generic;

namespace Asdf.Clients
{
	public class SimklShowsResponse : SimklResponse
	{
		public IEnumerable<SimklShowsResponseItem> shows { get; set; }
	}

	public class SimklShowsResponseItem
	{
		public DateTime last_watched_at { get; set; }

		public string status { get; set; }
		public string user_rating { get; set; }
		public string last_watched { get; set; }
		public string next_to_watch { get; set; }

		public int watched_episodes_count { get; set; }
		public int total_episodes_count { get; set; }
		public int not_aired_episodes_count { get; set; }

		public SimklShowsResponseItemDetail show { get; set; }
	}

	public class SimklShowsResponseItemDetail
	{
		public string title { get; set; }
		public string poster { get; set; }
		public int year { get; set; }

		public SimklShowsResponseItemIds ids { get; set; }
	}

	public class SimklShowsResponseItemIds
	{
		public int simkl { get; set; }
		public string slug { get; set; }
		public string imdb { get; set; }
		public string zap2it { get; set; }
		public string tmdb { get; set; }
		public string offen { get; set; }
		public string tvdbslug { get; set; }
		public string fb { get; set; }
		public string instagram { get; set; }
		public string tw { get; set; }
		public string tvdb { get; set; }
	}
}