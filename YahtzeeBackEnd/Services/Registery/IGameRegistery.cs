using YahtzeeBackEnd.Entites;

namespace YahtzeeBackEnd.Services.Registery
{
    public interface IGameRegistery
    {
        void AddGameInstance(GameInstance instance);
        GameInstance GetRoom(string roomCode);
        void RemoveGameInstance(GameInstance instance);
        bool RoomExists(string roomCode);
    }
}