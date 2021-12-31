using System;
using System.Net.NetworkInformation;

namespace Asdf.Services
{
	public class PingItem
	{
		public DateTime Timestamp { get; set; }
		public IPStatus Status { get; set; }
		public long RoundtripTime { get; set; }
	}
}