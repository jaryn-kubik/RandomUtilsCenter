using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RandomUtilsCenter.Server.Helpers
{
	public static class Utils
	{
		public static string BytesToString(long? value)
		{
			if (value == null)
			{
				return string.Empty;
			}

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
			Task.Run(() => MessageBox.Show(msg, title, MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly));
		}

		public static void ShowToast(string title, string msg)
		{
			new ToastContentBuilder()
				.AddText(title)
				.AddText(msg)
				.Show(x => x.ExpirationTime = DateTimeOffset.Now);
		}
	}
}