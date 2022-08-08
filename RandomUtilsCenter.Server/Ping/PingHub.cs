using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace RandomUtilsCenter.Server.Ping
{
	public class PingHub : Hub<IPingClient>
	{
		private readonly PingService _service;

		public PingHub(PingService service)
		{
			_service = service;
		}

		public async Task GetItemsAsync()
		{
			await Clients.Caller.ReceiveItemsAsync(_service.Items);
		}
	}
}