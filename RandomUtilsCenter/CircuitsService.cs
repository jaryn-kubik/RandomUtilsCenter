using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RandomUtilsCenter
{
	public class CircuitsService : CircuitHandler
	{
		private readonly MemoryCache _memoryCache = new(new MemoryCacheOptions());

		public override Task OnCircuitOpenedAsync(Circuit circuit, CancellationToken cancellationToken)
		{
			_memoryCache.Set(circuit.Id, circuit, TimeSpan.FromHours(1));
			return Task.CompletedTask;
		}

		public override Task OnCircuitClosedAsync(Circuit circuit, CancellationToken cancellationToken)
		{
			_memoryCache.Remove(circuit.Id);
			return Task.CompletedTask;
		}

		public bool Any => _memoryCache.Count > 0;
	}
}