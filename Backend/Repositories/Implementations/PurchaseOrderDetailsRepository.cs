using FlowerSelling.Data.FlowerSellingWebsite.Data;
using FlowerSellingWebsite.Models.Entities;
using FlowerSellingWebsite.Repositories.Interfaces;

namespace FlowerSellingWebsite.Repositories.Implementations
{
    public class PurchaseOrderDetailsRepository : BaseRepository<PurchaseOrderDetails>, IPurchaseOrderDetailsRepository
    {
        public PurchaseOrderDetailsRepository(FlowerSellingDbContext context) : base(context)
        {
        }
    }
}
