using System.Threading.Tasks;

namespace FreieWahl.UserData.Store
{
    /// <summary>
    /// Stores additional user data (main user data like user name, mail, password, ... is handled by firebase auth).
    /// Currently, this additional user data only consists of the user image.
    /// </summary>
    public interface IUserDataStore
    {
        /// <summary>
        /// Saves the image for a user
        /// </summary>
        /// <param name="userId">the user's id</param>
        /// <param name="imageData">a string representation of the user image</param>
        /// <returns>the future of this operation</returns>
        Task SaveUserImage(string userId, string imageData);

        /// <summary>
        /// gets the image for a user
        /// </summary>
        /// <param name="userId">the user's id</param>
        /// <returns>the string representation of the user image</returns>
        Task<string> GetUserImage(string userId);
    }
}
