﻿using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Asdf.Services
{
	public class PingService : IHostedService
	{
		private static readonly IPAddress _ip = new(new byte[] { 8, 8, 8, 8 });
		private readonly string _path = Path.Combine(Path.GetDirectoryName(typeof(Startup).Assembly.Location), ".ping.bin");
		private readonly Ping _ping = new();
		private readonly CancellationTokenSource _cancellationTokenSource = new();
		private readonly ConcurrentDictionary<DateTime, PingEntry> _items = new();
		private readonly ILogger<PingService> _logger;
		private readonly ConfigService _config;
		private BinaryWriter _writer;
		private DateTime _nextFlush = DateTime.Now.AddMinutes(1);

		public PingService(ILogger<PingService> logger, ConfigService config)
		{
			_logger = logger;
			_config = config;
		}

		public Task StartAsync(CancellationToken cancellationToken)
		{
			var stream = File.Open(_path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
			using var reader = new BinaryReader(stream, Encoding.ASCII, true);
			while (reader.PeekChar() != -1)
			{
				try
				{
					var timestamp = DateTime.FromBinary(reader.ReadInt64());
					var status = (IPStatus)reader.ReadInt32();
					var roundtripTime = reader.ReadInt64();
					_items.TryAdd(timestamp, new PingEntry(status, roundtripTime));
				}
				catch { }
			}
			_writer = new BinaryWriter(stream, Encoding.ASCII, false);
			_ = Task.Factory.StartNew(WorkerAsync, _cancellationTokenSource.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
			return Task.CompletedTask;
		}

		public async Task StopAsync(CancellationToken cancellationToken)
		{
			try { _cancellationTokenSource.Cancel(); }
			catch { }
			try { _writer.Flush(); }
			catch { }
			try { await _writer.DisposeAsync(); }
			catch { }
		}

		private async Task WorkerAsync()
		{
			while (!_cancellationTokenSource.IsCancellationRequested)
			{
				var interval = _config.PingInterval * 1000;
				if (interval > 0)
				{
					try
					{
						await SendPingAsync();
					}
					catch (Exception ex)
					{
						_logger.LogError(ex, "Error when sending ping");
					}
					await Task.Delay(interval);
				}
				else
				{
					await Task.Delay(60 * 1000);
				}
			}
		}

		private async Task SendPingAsync()
		{
			var timestamp = DateTime.Now;
			var reply = await _ping.SendPingAsync(_ip, 2000);
			_items.TryAdd(timestamp, new PingEntry(reply.Status, reply.RoundtripTime));
			_writer.Write(timestamp.ToBinary());
			_writer.Write((int)reply.Status);
			_writer.Write(reply.RoundtripTime);
			Updated?.Invoke();

			if (_nextFlush < timestamp)
			{
				_writer.Flush();
				_nextFlush = DateTime.Now.AddMinutes(1);
			}
		}

		public event Action Updated;
		public IEnumerable<KeyValuePair<DateTime, PingEntry>> Entries => _items;

		public record PingEntry(IPStatus Status, long RoundtripTime);
	}
}