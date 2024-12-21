using Microsoft.AspNetCore.SignalR;
using YahtzeeBackEnd.Entites;
using YahtzeeBackEnd.Services.Registery;

namespace YahtzeeBackEnd.Hubs
{
    public class GameHub(IGameRegistery gameRegistery) : Hub
    {
        private readonly IGameRegistery _gameRegistery = gameRegistery;

        public void CheckIfRoomExists(string roomName)
        {
            Clients.Caller.SendAsync("RoomCodeStatus", roomName+":" + _gameRegistery.RoomExists(roomName));
        }

        public void CreateRoom(string roomName) {
            if (_gameRegistery.RoomExists(roomName))
            {
                Clients.Caller.SendAsync("RoomCreation", "0");
            }
            else
            {
                GameInstance gameInstance = new(roomName);
                _gameRegistery.AddGameInstance(gameInstance);
                Clients.Caller.SendAsync("RoomCreation", "1");
            }
        }
    }
}
