using Api.Data;
using Api.Interface;
using Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Api.Repository
{
    public class StateRepository : IStateRepository
    {
        private readonly DataContext dataContext;

        public StateRepository(DataContext datacontext)
        {
            this.dataContext = datacontext;
        }

        public async Task<bool> StateHasSomeAsync()
        {
            return await dataContext.Statements.AnyAsync();
        }

        public async Task<User_state> GetStateAsync(string state)
        {
            return await dataContext.Statements.FirstOrDefaultAsync(x => x.Code.ToLower() == state); ;
        }

        public void CreateState(User_state state)
        {
            dataContext.Statements.Add(state);
        }

        public async Task SaveStateAsync()
        {
            await dataContext.SaveChangesAsync();
        }
    }
}
