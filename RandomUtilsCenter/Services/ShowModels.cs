using System;

namespace RandomUtilsCenter.Services
{
	public class ShowModel
	{
		public int Id { get; set; }

		public string ShowTitle { get; set; }
		public string EpisodeTitle { get; set; }

		public int Season { get; set; }
		public int Episode { get; set; }

		public DateTime Date { get; set; }
		public bool IsWatchable { get; set; }

		public string SeasonEpisode => $"S{Season:D2}E{Episode:D2}";
	}

	public class EpisodeModel
	{
		public string EpisodeTitle { get; set; }

		public int Season { get; set; }
		public int Episode { get; set; }

		public DateTime Date { get; set; }
	}
}