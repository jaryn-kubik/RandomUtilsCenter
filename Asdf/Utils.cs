using System.Windows.Forms;

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
				(1000L, "KB")
			};

			foreach (var (size, suffix) in sizes)
			{
				if (value > size)
				{
					return $"{(decimal)value / size:F2} {suffix}";
				}
			}
			return $"{value:F2} B";
		}

		public static void ShowMessage(string title, string msg)
		{
			MessageBox.Show(msg, title);
		}
	}
}