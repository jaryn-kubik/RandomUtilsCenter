using System.Collections.Generic;

namespace RandomUtilsCenter.Clients.Simkl
{
	public class SimklWatchRequest
	{
		public IEnumerable<SimklWatchRequestItem> shows { get; set; }
	}

	public class SimklWatchRequestItem
	{
		public string title { get; set; }
		public SimklWatchRequestItemIds ids { get; set; }
		public IEnumerable<SimklWatchRequestItemSeasons> seasons { get; set; }
	}

	public class SimklWatchRequestItemIds
	{
		public int simkl { get; set; }
	}

	public class SimklWatchRequestItemSeasons
	{
		public int number { get; set; }
		public IEnumerable<SimklWatchRequestItemEpisode> episodes { get; set; }
	}

	public class SimklWatchRequestItemEpisode
	{
		public int number { get; set; }
	}
}