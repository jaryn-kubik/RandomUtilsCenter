using System;

namespace Asdf
{
	public static class Utils
	{
		public static string BytesToString(long value)
		{
			var sizes = new[]
			{
				(1000 * 1000 * 1000L, "GB"),
				(1000 * 1000L, "MB"),
				(1000L, "KB"),
				(1L, "B")
			};

			foreach (var (size, suffix) in sizes)
			{
				if (value > size)
				{
					return $"{(decimal)value / size:F2} {suffix}";
				}
			}
			throw new NotSupportedException();
		}
	}
}