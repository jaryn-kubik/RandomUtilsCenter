using System;
using System.Net.NetworkInformation;

namespace RandomUtilsCenter.Services
{
	public class PingItem
	{
		public DateTime Timestamp { get; set; }
		public IPStatus Status { get; set; }
		public long RoundtripTime { get; set; }
	}
}