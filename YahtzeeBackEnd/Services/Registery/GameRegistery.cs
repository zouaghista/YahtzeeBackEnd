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
            foreach(var connectionId in instance.ConnectionIds.Where(e => e != "")) {
                RegisterPlayer(connectionId, instance.RoomCode);
                //_connectionMappings.Add(connectionId, instance.RoomCode); 
            }
        }
        public void RegisterPlayer(string connectionId, string roomCode)
        {
            if (_connectionMappings.ContainsKey(connectionId)) {
                //player has a room thats active
                var room = GetRoom(_connectionMappings[connectionId]);
                if(room != null)
                    RemoveGameInstance(room);
            }
            _connectionMappings[connectionId] = roomCode;
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
