using System.Collections.Generic;

namespace RandomUtilsCenter.Services
{
	public class ConfigService
	{
		private JsonHelper<ConfigService> Json { get; set; }

		public string RealDebridApiKey { get; set; }

		public List<HtmlWatcherConfig> HtmlWatcher { get; set; } = new List<HtmlWatcherConfig>();
		public double HtmlWatcherInterval { get; set; } = 5;

		public int PingInterval { get; set; } = 1;
		public int PingLatency { get; set; } = 20;

		public static ConfigService Load() => JsonHelper<ConfigService>.Load("config", x => x.Instance.Json = x);
		public void Save() => Json.Save();

		public class HtmlWatcherConfig
		{
			public bool Enabled { get; set; }
			public string Name { get; set; }
			public string Url { get; set; }
			public string Selector { get; set; }
		}
	}
}