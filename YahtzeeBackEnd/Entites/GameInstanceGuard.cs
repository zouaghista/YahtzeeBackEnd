using YahtzeeBackEnd.Enums;

namespace YahtzeeBackEnd.Entites
{
    public class GameInstanceGuard(string roomCode, string[] connectionIds)
    {
        private GameInstance _gameInstance = new();
        public readonly string RoomCode = roomCode;
        public string[] ConnectionIds = connectionIds;
        public string[] PlayerNames = new string[2];
        public GameInstance GameInstance { get => _gameInstance; }
        
        public bool VerifyPlayerTurn(string player)
        {
            return ((_gameInstance.Player1Turn) == (player == ConnectionIds[0])&&_gameInstance.Started); 
        }

        public bool VerifyLegalMove(YathzeeMove move)
        {
            return _gameInstance.GetCurrentPlayerScore()[(int)move] == -1 && _gameInstance.RollCount != 0;
        }
        public bool VerifyRoll()
        {
            return _gameInstance.RollCount < 3;
        }

        public void RegisterPlayerName(int id, string name)
        {
            if(id < 2)
            {
                PlayerNames[id] = name;
            }
        }

        public string GetPlayerNames()
        {
            return string.Join(":", PlayerNames);
        }
        public int PlayerId(string player) {
            if(player == ConnectionIds[0]) return 0;
            if (player == ConnectionIds[1]) return 1;
            return -1;
        }
    }
}
