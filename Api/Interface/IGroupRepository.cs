using Api.Models;
using System.Text.RegularExpressions;

namespace Api.Interface
{
    public interface IGroupRepository
    {
        Task<bool> GroupHasSomeAsync();

        Task<User_group> GetGroupAsync(int id);

        void CreateGroup(User_group group);

        Task SaveGroupAsync();
    }
}
