namespace Asdf.Clients.JDownloader
{
	public class JDownloaderPackagesResponse
	{
		public string activeTask { get; set; }
		public long bytesLoaded { get; set; }
		public long bytesTotal { get; set; }
		public int childCount { get; set; }
		public string comment { get; set; }
		public string downloadPassword { get; set; }
		public bool enabled { get; set; }
		public long eta { get; set; }
		public bool finished { get; set; }
		public string[] hosts { get; set; }
		public string name { get; set; }
		public string priority { get; set; }
		public bool running { get; set; }
		public string saveTo { get; set; }
		public long speed { get; set; }
		public string status { get; set; }
		public string statusIconKey { get; set; }
		public long uuid { get; set; }
	}
}