using FlowerSelling.Data.FlowerSellingWebsite.Data;
using FlowerSellingWebsite.Models.Entities;
using FlowerSellingWebsite.Repositories.Interfaces;
using ProjectGreenLens.Repositories.Implementations;

namespace FlowerSellingWebsite.Repositories.Implementations
{
    public class FlowersRepository : BaseRepository<Flowers>, IFlowersRepository
    {
        public FlowersRepository(FlowerSellingDbContext context) : base(context)
        {
        }
    }
}
