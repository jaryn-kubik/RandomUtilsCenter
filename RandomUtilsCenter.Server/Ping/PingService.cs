using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using RandomUtilsCenter.Server.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;

namespace RandomUtilsCenter.Server.Ping
{
	public class PingService : IHostedService
	{
		private static readonly IPAddress _ip = new(new byte[] { 8, 8, 8, 8 });
		private readonly System.Net.NetworkInformation.Ping _ping = new();
		private readonly JsonHelper<List<PingItem>> _items;
		private readonly TimerSync _sync;
		private readonly IHubContext<PingHub, IPingClient> _hub;
		private DateTime _lastSave;

		public PingService(IHubContext<PingHub, IPingClient> hub)
		{
			_items = JsonHelper<List<PingItem>>.Load("ping");
			_sync = new TimerSync(TimeSpan.FromSeconds(2), OnTimerAsync);
			_hub = hub;
		}

		public IReadOnlyList<PingItem> Items => _items.Instance;

		public Task StartAsync(CancellationToken cancellationToken)
		{
			_sync.Start();
			return Task.CompletedTask;
		}

		public async Task StopAsync(CancellationToken cancellationToken)
		{
			_sync.Stop();
			await _sync.RunAsync(_items.Save, cancellationToken);
		}

		private async Task OnTimerAsync()
		{
			var item = await SendPingAsync();

			var last = _items.Instance.LastOrDefault();
			if (last == null || last.Success != item.Success || (item.End - last.End).TotalMinutes > 10)
			{
				last = item;
				_items.Instance.Add(last);
			}
			last.End = item.End;
			last.Count++;
			last.Time += item.Time;

			await _hub.Clients.All.ReceiveItemsAsync(_items.Instance);
			if ((item.End - _lastSave).TotalMinutes > 1)
			{
				_items.Save();
				_lastSave = item.End;
			}
		}

		private async Task<PingItem> SendPingAsync()
		{
			try
			{
				var reply = await _ping.SendPingAsync(_ip, 10000);
				var timestamp = DateTime.Now;
				return new PingItem
				{
					Success = reply.Status == IPStatus.Success,
					Start = timestamp,
					End = timestamp,
					Time = reply.RoundtripTime
				};
			}
			catch
			{
				var timestamp = DateTime.Now;
				return new PingItem
				{
					Success = false,
					Start = timestamp,
					End = timestamp,
				};
			}
		}
	}
}