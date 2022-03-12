using Microsoft.Extensions.Hosting;
using RandomUtilsCenter.Services;
using System.Threading;
using System.Threading.Tasks;

namespace RandomUtilsCenter
{
	public class InitHostedService : IHostedService
	{
		private readonly YtDlpService _ytDlpService;

		public InitHostedService(YtDlpService ytDlpService)
		{
			_ytDlpService = ytDlpService;
		}

		public async Task StartAsync(CancellationToken cancellationToken)
		{
			await _ytDlpService.UpdateAsync(cancellationToken);
			_ytDlpService.Initialize();
		}

		public Task StopAsync(CancellationToken cancellationToken)
		{
			_ytDlpService.Shutdown();
			return Task.CompletedTask;
		}
	}
}