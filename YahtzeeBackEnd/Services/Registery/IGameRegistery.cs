using YahtzeeBackEnd.Entites;

namespace YahtzeeBackEnd.Services.Registery
{
    public interface IGameRegistery
    {
        void AddGameInstance(GameInstanceGuard instance);
        GameInstanceGuard? GetConnectIdsRoom(string connectionId);
        string? GetConnectIdsRoomCode(string connectionId);
        GameInstanceGuard? GetRoom(string roomCode);
        void RegisterPlayer(string connectionId, string roomCode);
        void RemoveGameInstance(GameInstanceGuard instance);
        bool RoomExists(string roomCode);
    }
}