using Microsoft.AspNetCore.SignalR;
using YahtzeeBackEnd.Entites;
using YahtzeeBackEnd.Enums;
using YahtzeeBackEnd.Mappers;
using YahtzeeBackEnd.Services.Registery;

namespace YahtzeeBackEnd.Hubs
{
    public class GameHub(IGameRegistery gameRegistery) : Hub
    {
        private readonly IGameRegistery _gameRegistery = gameRegistery;
        //Room Logic
        public override Task OnConnectedAsync()
        {
            Console.WriteLine("Wa jena chkoun");
            return base.OnConnectedAsync();
        }
        public void CheckIfRoomExists(string roomName)
        {
            Console.WriteLine("Someone checked", roomName);
            Clients.Caller.SendAsync("RoomCodeStatus", roomName+":" + _gameRegistery.RoomExists(roomName));
        }
        public void StartingPlayerRoll(string roomName)
        {
            var potentialRoom = _gameRegistery.GetRoom(roomName);
            if (potentialRoom != null)
            {
                if (!potentialRoom.ConnectionIds.Contains(Context.ConnectionId)) { return; }
                if(potentialRoom.GameInstance.Started) { return; }
                int playerId = potentialRoom.PlayerId(Context.ConnectionId);
                if (potentialRoom.GameInstance.TotalPlayerScores[playerId] == -1)
                {
                    do
                    {
                        potentialRoom.GameInstance.RollDice();
                        potentialRoom.GameInstance.TotalPlayerScores[playerId] = potentialRoom.GameInstance.Dice.Sum(score => score);
                    } while (potentialRoom.GameInstance.TotalPlayerScores[0] == potentialRoom.GameInstance.TotalPlayerScores[1]);
                    Clients.Clients(potentialRoom.ConnectionIds).SendAsync("StartingRoll", $"{playerId.ToString()}:{potentialRoom.GameInstance.TotalPlayerScores[playerId]}");
                    Clients.Clients(potentialRoom.ConnectionIds).SendAsync("SetDice", $"{playerId}:{EncodeDice(potentialRoom.GameInstance.Dice)}");
                }
                if (!potentialRoom.GameInstance.TotalPlayerScores.Any(score=>score < 0))
                {
                    potentialRoom.GameInstance.StartGame();
                    if (potentialRoom.GameInstance.Player1Turn)
                    {
                        Clients.Clients(potentialRoom.ConnectionIds).SendAsync("StartGame", "0");
                    }
                    else
                    {
                        Clients.Clients(potentialRoom.ConnectionIds).SendAsync("StartGame", "1");
                    }
                }
            }
        }
        public void CreateRoom(string playerName) {
            string roomName;
            do
            {
                roomName = RandomString();
            } while (_gameRegistery.RoomExists(roomName));
            Console.WriteLine("Someone Tried to create a room");
            GameInstanceGuard gameInstance = new(roomName, [Context.ConnectionId, ""]);
            gameInstance.RegisterPlayerName(0, playerName);
            _gameRegistery.AddGameInstance(gameInstance);
            Clients.Caller.SendAsync("RoomCreation", roomName);
            
        }

        public void JoinRoom(string roomName, string playerName) {
            var game = _gameRegistery.GetRoom(roomName);
            if (game!= null)
            {
                if (!game.ConnectionIds.Any(e => e == ""))
                {
                    Clients.Caller.SendAsync("RoomJoining", "0");
                    return;
                }
                game.ConnectionIds[1] = Context.ConnectionId;
                game.RegisterPlayerName(1, playerName);
                _gameRegistery.RegisterPlayer(Context.ConnectionId, roomName);
                Clients.Clients(game.ConnectionIds).SendAsync("RoomJoining", game.GetPlayerNames());
            }
            else
            {
                Clients.Caller.SendAsync("RoomJoining", "0");
            }
        }
        public void PlayAgain(string roomCode)
        {
            var potentialRoom = _gameRegistery.GetRoom(roomCode);
            if (potentialRoom != null)
            {
                if (!potentialRoom.ConnectionIds.Contains(Context.ConnectionId)) { return; }
                potentialRoom.SetWillingToPlay(potentialRoom.PlayerId(Context.ConnectionId));
                if (potentialRoom.WillRetry.All(e => e))
                {
                    potentialRoom.GameInstance.ResetGame();
                    potentialRoom.ResetWillingToPlay();
                    Clients.Clients(potentialRoom.ConnectionIds).SendAsync("RestartGame", "1");
                }
            }
        }
        public void QuitRoom(string roomCode)
        {
            var potentialRoom = _gameRegistery.GetRoom(roomCode);
            if (potentialRoom != null) {
                if (!potentialRoom.ConnectionIds.Contains(Context.ConnectionId)) { return; }
                Clients.Clients(potentialRoom.ConnectionIds).SendAsync("RoomClosure", "1");
                _gameRegistery.RemoveGameInstance(potentialRoom);
            }
        }
        //Dice Logic
        public void HoldDice(string roomCode, string dice)
        {
            if (dice.Length != 5) return;
            var potentialRoom = _gameRegistery.GetRoom(roomCode);
            if (potentialRoom != null&&potentialRoom.VerifyPlayerTurn(Context.ConnectionId))
            {
                if (potentialRoom.VerifyPlayerTurn(Context.ConnectionId))
                {
                    for(int i = 0; i < 5; i++)
                    {
                        if (dice[i] == '1')
                        {
                            potentialRoom.GameInstance.KeepDice(i);
                        }
                        else
                        {
                            potentialRoom.GameInstance.DiscardDice(i);
                        }
                    }
                    Clients.Clients(potentialRoom.ConnectionIds).SendAsync("HideDice", EncodeHeldDice(potentialRoom.GameInstance.RollingDice));
                }
            }
        }
        public void RollDice(string roomCode)
        {
            var potentialRoom = _gameRegistery.GetRoom(roomCode);
            if (potentialRoom != null)
            {
                if (!potentialRoom.ConnectionIds.Contains(Context.ConnectionId)) { return; }
                if (potentialRoom.VerifyPlayerTurn(Context.ConnectionId)&&potentialRoom.VerifyRoll())
                {
                    potentialRoom.GameInstance.RollDice();
                    var vals = "";
                    foreach(byte dice in potentialRoom.GameInstance.Dice)
                    {
                        vals += dice.ToString();
                    }
                    Clients.Clients(potentialRoom.ConnectionIds).SendAsync("HideDice", EncodeHeldDice(potentialRoom.GameInstance.RollingDice));
                    Clients.Clients(potentialRoom.ConnectionIds).SendAsync("SetDice", $"2:{EncodeDice(potentialRoom.GameInstance.Dice)}");
                }
            }
        }
        //Selection Logic
        public void SelectField(string roomCode, string field)
        {
            if (!YathzeeFieldNameMapper.FieldCodes.TryGetValue(field, out var move)) return;
            var potentialRoom = _gameRegistery.GetRoom(roomCode);
            if (potentialRoom != null)
            {
                if (!potentialRoom.GameInstance.Started) return;
                if (!potentialRoom.ConnectionIds.Contains(Context.ConnectionId)) { return; }
                try
                {

                    if (potentialRoom.VerifyPlayerTurn(Context.ConnectionId) && potentialRoom.VerifyLegalMove(move))
                    {
                        if (potentialRoom.GameInstance.SelectPlayerField(move))
                        {
                            var result = potentialRoom.GameInstance.Playerscores;
                            Clients.Clients(potentialRoom.ConnectionIds).SendAsync("GameSummery", result);
                        }
                        else
                        {
                            Clients.Clients(potentialRoom.ConnectionIds).SendAsync("FieldSelection", $"{potentialRoom.PlayerId(Context.ConnectionId)}:{field}:{potentialRoom.GameInstance.GetLastPlayerScore()[(int)move]}");
                            Console.WriteLine($"{potentialRoom.PlayerId(Context.ConnectionId)}:{field}:{potentialRoom.GameInstance.GetLastPlayerScore()[(int)move]}");
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
                Clients.Clients(potentialRoom.ConnectionIds).SendAsync("RoomClosure", "1");
                Clients.Clients(potentialRoom.ConnectionIds).SendAsync("GameSummery", "");
                _gameRegistery.RemoveGameInstance(potentialRoom);
            }
            return base.OnDisconnectedAsync(exception);
        }

        private string EncodeDice(byte[] dice)
        {
            string res = string.Empty;
            for (int i = 0; i < dice.Length; i++) {
                res += dice[i].ToString() + ',';
            }
            return res.Remove(res.Length-1);
        }
        private string EncodeHeldDice(bool[] dice)
        {

            string res = string.Empty;
            for (int i = 0; i < dice.Length; i++)
            {
                res += (dice[i]?"1":"0") + ",";
            }
            return res.Remove(res.Length - 1);
        }
        private static Random random = new Random();

        private static string RandomString()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, 8)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }

}
