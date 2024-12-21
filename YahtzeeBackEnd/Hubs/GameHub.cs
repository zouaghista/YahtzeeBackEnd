using Microsoft.AspNetCore.SignalR;
using YahtzeeBackEnd.Entites;
using YahtzeeBackEnd.Enums;
using YahtzeeBackEnd.Services.Registery;

namespace YahtzeeBackEnd.Hubs
{
    public class GameHub(IGameRegistery gameRegistery) : Hub
    {
        private readonly IGameRegistery _gameRegistery = gameRegistery;
        //Room Logic

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
                GameInstanceGuard gameInstance = new(roomName, [Context.ConnectionId, ""]);
                _gameRegistery.AddGameInstance(gameInstance);
                Clients.Caller.SendAsync("RoomCreation", "1");
            }
        }

        public void JoinRoom(string roomName) {
            var game = _gameRegistery.GetRoom(roomName);
            if (game!= null)
            {
                game.ConnectionIds[1] = Context.ConnectionId;
                Clients.Clients(game.ConnectionIds).SendAsync("RoomJoining", "1");
            }
            else
            {
                Clients.Caller.SendAsync("RoomCreation", "0");
            }
        }

        public void QuitRoom(string roomCode)
        {
            var potentialRoom = _gameRegistery.GetRoom(roomCode);
            if (potentialRoom != null) {
                if (!potentialRoom.ConnectionIds.Contains(Context.ConnectionId)) { return; }
                Clients.Clients(potentialRoom.ConnectionIds).SendAsync("RoomQuitting", "1");
                _gameRegistery.RemoveGameInstance(potentialRoom);
            }
        }
        //Dice Logic
        public void KeepDice(string roomCode, int dice)
        {
            var potentialRoom = _gameRegistery.GetRoom(roomCode);
            if (potentialRoom != null) {
                if (!potentialRoom.ConnectionIds.Contains(Context.ConnectionId)) { return; }
                if (potentialRoom.VerifyPlayerTurn(Context.ConnectionId))
                {
                    potentialRoom.GameInstance.KeepDice(dice);
                }
            }
        }
        public void DiscardDice(string roomCode, int dice)
        {
            var potentialRoom = _gameRegistery.GetRoom(roomCode);
            if (potentialRoom != null)
            {
                if (!potentialRoom.ConnectionIds.Contains(Context.ConnectionId)) { return; }
                if (potentialRoom.VerifyPlayerTurn(Context.ConnectionId))
                {
                    potentialRoom.GameInstance.DiscardDice(dice);
                }
            }
        }
        public void RollDice(string roomCode)
        {
            var potentialRoom = _gameRegistery.GetRoom(roomCode);
            if (potentialRoom != null)
            {
                if (!potentialRoom.ConnectionIds.Contains(Context.ConnectionId)) { return; }
                if (potentialRoom.VerifyPlayerTurn(Context.ConnectionId))
                {
                    potentialRoom.GameInstance.RollDice();
                    var vals = "";
                    foreach(byte dice in potentialRoom.GameInstance.Dice)
                    {
                        vals += dice.ToString();
                    }
                    Clients.Clients(potentialRoom.ConnectionIds).SendAsync("DiceValues", vals);
                }
            }
        }
        //Selection Logic
        public void SelectField(string roomCode, int move)
        {
            var potentialRoom = _gameRegistery.GetRoom(roomCode);
            if (potentialRoom != null)
            {
                if (!potentialRoom.ConnectionIds.Contains(Context.ConnectionId)) { return; }
                try
                {

                    if (potentialRoom.VerifyPlayerTurn(Context.ConnectionId) && potentialRoom.VerifyLegalMove((YathzeeMove)move))
                    {
                        if (potentialRoom.GameInstance.SelectPlayerField((YathzeeMove)move))
                        {
                            var result = potentialRoom.GameInstance.Playerscores;
                            Clients.Clients(potentialRoom.ConnectionIds).SendAsync("GameSummery", result);
                            _gameRegistery.RemoveGameInstance(potentialRoom);//immidate delition, Rematch logic will be added later
                        }
                        else
                        {
                            Clients.Clients(potentialRoom.ConnectionIds).SendAsync("FieldSelection", !potentialRoom.GameInstance.Player1Turn ,move, potentialRoom.GameInstance.GetLastPlayerScore());
                        }
                    }
                }catch
                {

                }
            }
        }
        public override Task OnDisconnectedAsync(Exception? exception)
        {
            var potentialRoom = _gameRegistery.GetConnectIdsRoom(Context.ConnectionId);
            if (potentialRoom != null)
            {
                Clients.Clients(potentialRoom.ConnectionIds).SendAsync("RoomQuitting", "1");
                _gameRegistery.RemoveGameInstance(potentialRoom);
            }
            return base.OnDisconnectedAsync(exception);
        }
    }

}
