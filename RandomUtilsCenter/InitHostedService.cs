using Microsoft.Extensions.Hosting;
using Microsoft.Toolkit.Uwp.Notifications;
using RandomUtilsCenter.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RandomUtilsCenter
{
	public class InitHostedService : IHostedService
	{
		private readonly YtDlpService _ytDlpService;
		private readonly IHostApplicationLifetime _lifetime;

		public InitHostedService(YtDlpService ytDlpService, IHostApplicationLifetime lifetime)
		{
			_ytDlpService = ytDlpService;
			_lifetime = lifetime;
		}

		public async Task StartAsync(CancellationToken cancellationToken)
		{
			await _ytDlpService.UpdateAsync(cancellationToken);
			_ytDlpService.Initialize();
			AppDomain.CurrentDomain.ProcessExit += (_, _) => Shutdown();
		}

		public Task StopAsync(CancellationToken cancellationToken)
		{
			return Task.CompletedTask;
		}

		private void Shutdown()
		{
			ToastNotificationManagerCompat.History.Clear();
			ToastNotificationManagerCompat.Uninstall();
			_ytDlpService.Shutdown();
		}
	}
}