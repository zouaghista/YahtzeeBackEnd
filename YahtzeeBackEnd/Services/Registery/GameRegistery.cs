using YahtzeeBackEnd.Entites;

namespace YahtzeeBackEnd.Services.Registery
{
    public class GameRegistery : IGameRegistery
    {
        private readonly Dictionary<string, GameInstanceGuard> _currentGames;
        private readonly Dictionary<string, string> _connectionMappings;
        public GameRegistery()
        {
            _currentGames = [];
            _connectionMappings = [];
        }
        public void AddGameInstance(GameInstanceGuard instance)
        {
            _currentGames.Add(instance.RoomCode, instance);
            _connectionMappings.Add(instance.ConnectionIds[0], instance.RoomCode);
            _connectionMappings.Add(instance.ConnectionIds[1], instance.RoomCode);
        }
        public void RemoveGameInstance(GameInstanceGuard instance)
        {
            _currentGames.Remove(instance.RoomCode);
            _connectionMappings.Remove(instance.ConnectionIds[0]);
            _connectionMappings.Remove(instance.ConnectionIds[1]);
        }
        public GameInstanceGuard? GetRoom(string roomCode)
        {
            _currentGames.TryGetValue(roomCode, out var instance);
            return instance;
        }
        public bool RoomExists(string roomCode)
        {
            return _currentGames.ContainsKey(roomCode);
        }
        public GameInstanceGuard? GetConnectIdsRoom(string connectionId)
        {
            _connectionMappings.TryGetValue(connectionId, out var result);
            if(result == null) { return null; }
            _currentGames.TryGetValue(result, out var instance);
            return instance;
        }
        public string? GetConnectIdsRoomCode(string connectionId)
        {
            _connectionMappings.TryGetValue(connectionId, out var result);
            return result;
        }
    }
}
