using Api.Models.Base;

namespace Api.Models
{
    public class User : BaseInfo
    {
        public string Login { get; set; } = null!;

        public string Password { get; set; } = null!;

        public DateOnly Created_date { get; set; } = DateOnly.FromDateTime(DateTime.Now);

        public User_group User_group_id { get; set; } = null!;

        public User_state User_state_id { get; set; } = null!;
    }
}
