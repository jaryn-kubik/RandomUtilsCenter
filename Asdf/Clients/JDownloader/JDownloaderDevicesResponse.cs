using System.Collections.Generic;

namespace Asdf.Clients.JDownloader
{
	public class JDownloaderDevicesResponse
	{
		public long rid { get; set; }
		public IEnumerable<JDownloaderDeviceResponse> list { get; set; }
	}

	public class JDownloaderDeviceResponse
	{
		public string id { get; set; }
		public string type { get; set; }
		public string name { get; set; }
	}
}