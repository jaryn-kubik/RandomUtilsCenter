using System.Collections.Generic;
using System.Threading.Tasks;

namespace RandomUtilsCenter.Server.Ping
{
	public interface IPingClient
	{
		Task ReceiveItemsAsync(IReadOnlyList<PingItem> item);
	}
}