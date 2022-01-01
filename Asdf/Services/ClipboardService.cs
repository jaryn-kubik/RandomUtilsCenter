using Microsoft.Extensions.Hosting;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Asdf.Services
{
	public class ClipboardService : IHostedService
	{
		private MessageHandler _handler;

		public event Action<string> Changed;

		public void SetText(string text)
		{
			var thread = new Thread(() => Clipboard.SetText(text));
			thread.SetApartmentState(ApartmentState.STA);
			thread.Start();
		}

		public Task StartAsync(CancellationToken cancellationToken)
		{
			var thread = new Thread(() =>
			{
				_handler = new(this);
				AddClipboardFormatListener(_handler.Handle);
				Application.ThreadException += Application_ThreadException;
				Application.Run();
			});
			thread.SetApartmentState(ApartmentState.STA);
			thread.Start();
			return Task.CompletedTask;
		}

		private void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
		{
			Utils.ShowMessage("Error - ClipboardService", $"{e.Exception}");
		}

		public Task StopAsync(CancellationToken cancellationToken)
		{
			RemoveClipboardFormatListener(_handler.Handle);
			_handler = null;
			Application.Exit();
			return Task.CompletedTask;
		}

		private class MessageHandler : NativeWindow
		{
			private readonly ClipboardService _clipboard;
			private string _last;

			public MessageHandler(ClipboardService clipboard)
			{
				CreateHandle(new CreateParams());
				_clipboard = clipboard;
			}

			protected override void WndProc(ref Message msg)
			{
				if (msg.Msg == WM_CLIPBOARDUPDATE)
				{
					var text = Clipboard.GetText();
					if (!string.IsNullOrWhiteSpace(text) && text != _last)
					{
						_last = text;
						try
						{
							_clipboard.Changed?.Invoke(text);
						}
						catch (Exception ex)
						{
							Utils.ShowMessage("Error - ClipboardService", $"Text: {text}{Environment.NewLine}{ex}");
						}
					}
				}
				base.WndProc(ref msg);
			}
		}

		private const int WM_CLIPBOARDUPDATE = 0x031D;

		[DllImport("user32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool AddClipboardFormatListener(IntPtr hwnd);

		[DllImport("user32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool RemoveClipboardFormatListener(IntPtr hwnd);
	}
}