using EnFoco_new.Models;

namespace EnFoco_new.Services
{
    public interface IUserService
    {
        Task<User?> GetUserByNameAsync(string name);
        Task<bool> VerifyPasswordAsync(User user, string password);
        Task<User?> CreateUserAsync(string name, string password);
    }
}