using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace Asdf.Services
{
	public class ConfigService
	{
		private string FilePath { get; set; }

		public string AllDebridToken { get; set; }
		public string SimklClientID { get; set; }
		public string SimklClientSecret { get; set; }
		public string SimklToken { get; set; }
		public string TmdbApiKey { get; set; }

		public string JDownloaderDir { get; set; }
		public string JDownloaderUserName { get; set; }
		public string JDownloaderPassword { get; set; }

		public List<HtmlWatcherConfig> HtmlWatcher { get; set; } = new List<HtmlWatcherConfig>();
		public double HtmlWatcherInterval { get; set; } = 5;

		public int PingInterval { get; set; } = 1;
		public int PingLatency { get; set; } = 50;

		public void Save()
		{
			var data = JsonSerializer.SerializeToUtf8Bytes(this, new JsonSerializerOptions { WriteIndented = true });
			File.WriteAllBytes(FilePath, data);
		}

		public static ConfigService Load()
		{
			var path = Path.Combine(Path.GetDirectoryName(typeof(Startup).Assembly.Location), ".config.json");
			try
			{
				var data = File.ReadAllBytes(path);
				var result = JsonSerializer.Deserialize<ConfigService>(data);
				result.FilePath = path;
				return result;
			}
			catch
			{
				var result = new ConfigService { FilePath = path };
				result.Save();
				return result;
			}
		}

		public class HtmlWatcherConfig
		{
			public bool Enabled { get; set; }
			public string Name { get; set; }
			public string Url { get; set; }
			public string Selector { get; set; }
		}
	}
}