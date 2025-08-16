using FlowerSelling.Data.FlowerSellingWebsite.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace FlowerSellingWebsite.Infrastructure.DbContext
{
    public class FlowerSellingDbContextFactory : IDesignTimeDbContextFactory<FlowerSellingDbContext>
    {
        public FlowerSellingDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<FlowerSellingDbContext>();
            optionsBuilder.UseSqlServer("Server=localhost,1433;Database=FlowerSellingDB;User Id=sa;Password=123abc@123;Trusted_Connection=False;;Encrypt=False");

            return new FlowerSellingDbContext(optionsBuilder.Options);
        }
    }
}
