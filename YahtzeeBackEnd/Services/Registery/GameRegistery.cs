using YahtzeeBackEnd.Entites;

namespace YahtzeeBackEnd.Services.Registery
{
    public class GameRegistery : IGameRegistery
    {
        private Dictionary<string, GameInstance> _currentGames;
        public GameRegistery()
        {
            _currentGames = [];
        }
        public void AddGameInstance(GameInstance instance)
        {
            _currentGames.Add(instance.RoomCode, instance);
        }
        public void RemoveGameInstance(GameInstance instance)
        {
            _currentGames.Remove(instance.RoomCode);
        }
        public GameInstance GetRoom(string roomCode)
        {
            return _currentGames[roomCode];
        }
        public bool RoomExists(string roomCode)
        {
            return _currentGames.ContainsKey(roomCode);
        }
    }
}
