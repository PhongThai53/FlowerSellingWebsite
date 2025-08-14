using FlowerSelling.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace FlowerSellingWebsite.Infrastructure.DbContext
{
    public class FlowerSellingDbContextFactory : IDesignTimeDbContextFactory<FlowerSellingDbContext>
    {
        public FlowerSellingDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<FlowerSellingDbContext>();
            optionsBuilder.UseSqlServer("Server=localhost;Database=FlowerSellingDB;User Id=Long;Password=123;Trusted_Connection=True;;Encrypt=False");

            return new FlowerSellingDbContext(optionsBuilder.Options);
        }
    }
}
