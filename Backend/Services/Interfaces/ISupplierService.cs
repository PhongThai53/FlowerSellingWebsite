using FlowerSellingWebsite.Models.DTOs;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

namespace FlowerSellingWebsite.Services.Interfaces
{
    public interface ISupplierService
    {
        Task<bool> CreateSupplierAsync(CreateSupplierRequest request);
    }
}
