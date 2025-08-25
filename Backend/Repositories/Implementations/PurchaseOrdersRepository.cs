using FlowerSelling.Data.FlowerSellingWebsite.Data;
using FlowerSellingWebsite.Models.Entities;
using FlowerSellingWebsite.Repositories.Interfaces;
using ProjectGreenLens.Repositories.Implementations;

namespace FlowerSellingWebsite.Repositories.Implementations
{
    public class PurchaseOrdersRepository : BaseRepository<PurchaseOrders>, IPurchaseOrdersRepository
    {
        public PurchaseOrdersRepository(FlowerSellingDbContext context) : base(context)
        {
        }
    }
}
