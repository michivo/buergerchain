using System.Threading.Tasks;

namespace FreieWahl.UserData.Store
{
    public interface IUserDataStore
    {
        Task SaveUserImage(string userId, string imageData);

        Task<string> GetUserImage(string userId);
    }
}
