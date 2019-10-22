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
	}
}