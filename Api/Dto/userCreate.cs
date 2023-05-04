namespace Api.Dto
{
    public class UserCreate
    {
        public string Login { get; set; } = null!;

        public string Password { get; set; } = null!;

        public int User_group_id { get; set; }
    }
}
