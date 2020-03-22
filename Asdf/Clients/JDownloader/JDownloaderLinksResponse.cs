using System.Collections.Generic;

namespace Asdf.Clients.JDownloader
{

	public class JDownloaderLinksResponse
	{
		public long addedDate { get; set; }
		public long bytesLoaded { get; set; }
		public long bytesTotal { get; set; }
		public string comment { get; set; }
		public string downloadPassword { get; set; }
		public bool enabled { get; set; }
		public long eta { get; set; }
		public string extractionStatus { get; set; }
		public bool finished { get; set; }
		public long finishedDate { get; set; }
		public string host { get; set; }
		public string name { get; set; }
		public long packageUUID { get; set; }
		public string priority { get; set; }
		public bool running { get; set; }
		public bool skipped { get; set; }
		public long speed { get; set; }
		public string status { get; set; }
		public string statusIconKey { get; set; }
		public string url { get; set; }
		public long uuid { get; set; }
	}
}