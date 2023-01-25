using _211933M_Assn.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using _211933M_Assn.Models;
using Microsoft.Extensions.Logging;

namespace _211933M_Assn
{
    public class MyDbContext : IdentityDbContext<User>
    {
        private readonly IConfiguration _configuration;
        //constructor
        public MyDbContext(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string connectionString = _configuration.GetConnectionString("MyConnection");
            optionsBuilder.UseSqlServer(connectionString);
        }
        //DbSet = table in db
        public DbSet<User> AspNetUsers { get; set; }
    }
}
