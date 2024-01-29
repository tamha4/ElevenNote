using ElevenNote.Models.User;

namespace ElevenNote.Services.User
{
    public interface IUserServices
    {
        Task<bool> RegisterUserAsync(UserRegister model);
        Task<UserDetail?> GetUserByIdAsync(int userId);

    }
}