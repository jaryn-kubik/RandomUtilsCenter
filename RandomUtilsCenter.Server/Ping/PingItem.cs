using System;

namespace RandomUtilsCenter.Server.Ping
{
	public class PingItem
	{
		public DateTime Start { get; set; }
		public DateTime End { get; set; }

		public bool Success { get; set; }
		public long Count { get; set; }
		public long Time { get; set; }
	}
}