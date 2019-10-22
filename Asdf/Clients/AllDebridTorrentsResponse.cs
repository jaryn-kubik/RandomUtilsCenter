using System.Collections.Generic;

namespace Asdf.Clients
{
	public class AllDebridTorrentsResponse : AllDebridResponse
	{
		public Dictionary<string, AllDebridTorrentsResponseItem> torrents { get; set; }
	}

	public class AllDebridTorrentsResponseItem
	{
		public long id { get; set; }
		public string filename { get; set; }

		public long size { get; set; }
		public long downloaded { get; set; }
		public long uploaded { get; set; }

		public string status { get; set; }
		public int statusCode { get; set; }

		public IEnumerable<AllDebridTorrentsResponseLink> links { get; set; }
	}

	public class AllDebridTorrentsResponseLink
	{
		public string link { get; set; }
		public string filename { get; set; }
		public long size { get; set; }
	}
}