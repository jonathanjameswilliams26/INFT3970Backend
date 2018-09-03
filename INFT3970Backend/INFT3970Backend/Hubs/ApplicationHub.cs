using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using INFT3970Backend.Models;
using INFT3970Backend.Data_Access_Layer;

namespace INFT3970Backend.Hubs
{
    public class ApplicationHub : Hub
    {
        public override Task OnConnectedAsync()
        {
            int playerID = int.Parse(Context.GetHttpContext().Request.Query["playerID"]);
            PlayerDAL playerDAL = new PlayerDAL();
            Response<object> response = playerDAL.UpdateConnectionID(playerID, Context.ConnectionId);
            return base.OnConnectedAsync();
        }
    }
}
