using Api.Models;

namespace Api.Interface
{
    public interface IUserRepository
    {
        Task<IList<User>> GetUsersAsync(Pagination pagination);

        Task<User> GetUserAsync(int id);

        Task<bool> UserExistsAsync(int id);

        Task<bool> UsersAlredyHaveAdminAsync();

        void CreateUser(User user);

        void UpdateUser(User user);

        Task SaveUserAsync();
    }
}
