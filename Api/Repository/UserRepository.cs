using Api.Data;
using Api.Interface;
using Api.Models;
using Microsoft.EntityFrameworkCore;
using static Api.Enums;

namespace Api.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly DataContext dataContext;

        public UserRepository(DataContext datacontext)
        {
            this.dataContext = datacontext;
        }
        public async Task<IList<User>> GetUsersAsync(Pagination pagination)
        {
            return await dataContext.Users
                .Include(x => x.User_group_id)
                .Include(x => x.User_state_id)
                .Where(u => u.User_state_id.Code.ToLower() == States.active.ToString())
                .Skip((pagination.PageNumber - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .ToListAsync();
        }

        public async Task<User> GetUserAsync(int id)
        {
            return await dataContext.Users
                .Include(x => x.User_group_id)
                .Include(x => x.User_state_id)
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<bool> UserExistsAsync(int id)
        {
            return await dataContext.Users.AnyAsync(u => u.Id == id);
        }

        public async Task<bool> UsersAlredyHaveAdminAsync()
        {
            return await dataContext.Users.AnyAsync(x => x.User_group_id.Code.ToLower() == Groups.admin.ToString());
        }

        public void CreateUser(User user)
        {
            dataContext.Users.Add(user);
        }

        public void UpdateUser(User user)
        {
            dataContext.Users.Update(user);
        }

        public async Task SaveUserAsync()
        {
            await dataContext.SaveChangesAsync();
        }
    }
}
