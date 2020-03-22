using System.Collections.Generic;

namespace Asdf.Clients.JDownloader
{
	public class JDownloaderLinksRequest
	{
		public bool addedDate { get; set; } = true;
		public bool bytesLoaded { get; set; } = true;
		public bool bytesTotal { get; set; } = true;
		public bool comment { get; set; } = true;
		public bool enabled { get; set; } = true;
		public bool eta { get; set; } = true;
		public bool extractionStatus { get; set; } = true;
		public bool finished { get; set; } = true;
		public bool finishedDate { get; set; } = true;
		public bool host { get; set; } = true;
		public IEnumerable<long> jobUUIDs { get; set; }
		public int maxResults { get; set; } = int.MaxValue;
		public IEnumerable<long> packageUUIDs { get; set; }
		public bool password { get; set; } = true;
		public bool priority { get; set; } = true;
		public bool running { get; set; } = true;
		public bool skipped { get; set; } = true;
		public bool speed { get; set; } = true;
		public int startAt { get; set; } = int.MinValue;
		public bool status { get; set; } = true;
		public bool url { get; set; } = true;
	}
}