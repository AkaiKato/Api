using Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Api.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options)
            : base(options) {}

        public DbSet<User> Users { get; set; }

        public DbSet<User_group> Groups { get; set; }

        public DbSet<User_state> Statements { get; set; }  


    }
}
