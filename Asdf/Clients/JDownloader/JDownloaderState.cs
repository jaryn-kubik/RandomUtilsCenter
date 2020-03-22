using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Asdf.Clients.JDownloader
{
	public class JDownloaderState
	{
		private long _requestId = 0;
		public long RequestId => _requestId++;

		public byte[] LoginSecret { get; private set; }
		public byte[] DeviceSecret { get; private set; }
		public string SessionToken { get; private set; }
		public string RegainToken { get; private set; }
		public byte[] ServerEncryptionToken { get; private set; }
		public byte[] DeviceEncryptionToken { get; private set; }

		public string DeviceId { get; set; }
		public string ConnectionUrl { get; set; }

		public void SetSecrets(string email, string password)
		{
			LoginSecret = SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes($"{email}{password}server"));
			DeviceSecret = SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes($"{email}{password}device"));
		}

		public void SetTokens(string sessiontoken, string regaintoken)
		{
			SessionToken = sessiontoken;
			RegainToken = regaintoken;

			var data = new byte[sessiontoken.Length / 2];
			for (var i = 0; i < data.Length; i++)
			{
				var byteValue = sessiontoken.Substring(i * 2, 2);
				data[i] = Convert.ToByte(byteValue, 16);
			}
			ServerEncryptionToken = SHA256.Create().ComputeHash(LoginSecret.Concat(data).ToArray());
			DeviceEncryptionToken = SHA256.Create().ComputeHash(DeviceSecret.Concat(data).ToArray());
		}
	}
}