using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Test_Core_API.Model;

namespace Test_Core_API.Entity
{
    public class Context: IdentityDbContext<ApplicationUser> 
    {

        public DbSet<Product> products { get; set; }

        public DbSet<User> users { get; set; }



        public Context(DbContextOptions options):base(options) { }
    }
}
