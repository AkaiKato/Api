using Api.Data;
using Api.Dto;
using Api.Interface;
using Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Api.Repository
{
    public class GroupRepository : IGroupRepository
    {
        private readonly DataContext dataContext;

        public GroupRepository(DataContext datacontext)
        {
            this.dataContext = datacontext;
        }

        public async Task<bool> GroupHasSomeAsync()
        {
            return await dataContext.Groups.AnyAsync();
        }

        public async Task<User_group> GetGroupAsync(int id)
        {
            return await dataContext.Groups.FirstOrDefaultAsync(x => x.Id == id);
        }

        public void CreateGroup(User_group group)
        {
            dataContext.Groups.Add(group);
        }

        public async Task SaveGroupAsync()
        {
            await dataContext.SaveChangesAsync();
        }
    }
}
