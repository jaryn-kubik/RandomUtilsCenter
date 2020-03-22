using System.Collections.Generic;

namespace Asdf.Clients.JDownloader
{
	public class JDownloaderDeviceConnectionsResponse
	{
		public IEnumerable<JDownloaderDeviceConnectionsInfo> infos { get; set; }
	}

	public class JDownloaderDeviceConnectionsInfo
	{
		public int port { get; set; }
		public string ip { get; set; }
	}
}