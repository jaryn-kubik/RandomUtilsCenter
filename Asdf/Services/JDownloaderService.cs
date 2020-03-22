using Microsoft.Extensions.Hosting;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Asdf.Services
{
	public class JDownloaderService : IHostedService
	{
		private readonly ConfigService _config;
		private Process _process;

		public JDownloaderService(ConfigService config)
		{
			_config = config;
		}

		public Task StartAsync(CancellationToken cancellationToken)
		{
			var pathJava = Path.Combine(_config.JDownloaderDir, "jre\\bin\\java.exe");
			var pathJar = Path.Combine(_config.JDownloaderDir, "JDownloader.jar");

			_process = Process.Start(new ProcessStartInfo(pathJava, $"-Djava.awt.headless=true -jar \"{pathJar}\"")
			{
				CreateNoWindow = true
			});
			return Task.CompletedTask;
		}

		public Task StopAsync(CancellationToken cancellationToken)
		{
			_process.Kill();
			return Task.CompletedTask;
		}
	}
}