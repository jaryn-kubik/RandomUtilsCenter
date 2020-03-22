using System.Collections.Generic;

namespace Asdf.Clients.JDownloader
{
	public class JDownloaderPackagesRequest
	{
		public bool bytesLoaded { get; set; } = true;
		public bool bytesTotal { get; set; } = true;
		public bool childCount { get; set; } = true;
		public bool comment { get; set; } = true;
		public bool enabled { get; set; } = true;
		public bool eta { get; set; } = true;
		public bool finished { get; set; } = true;
		public bool hosts { get; set; } = true;
		public int maxResults { get; set; } = int.MaxValue;
		public IEnumerable<long> packageUUIDs { get; set; }
		public bool priority { get; set; } = true;
		public bool running { get; set; } = true;
		public bool saveTo { get; set; } = true;
		public bool speed { get; set; } = true;
		public int startAt { get; set; } = int.MinValue;
		public bool status { get; set; } = true;
	}
}