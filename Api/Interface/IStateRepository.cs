using Api.Models;

namespace Api.Interface
{
    public interface IStateRepository
    {
        Task<bool> StateHasSomeAsync();

        Task<User_state> GetStateAsync(string state);

        void CreateState(User_state state);

        Task SaveStateAsync();
    }
}
