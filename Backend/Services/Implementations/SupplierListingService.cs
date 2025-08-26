using FlowerSellingWebsite.Models.DTOs;
using FlowerSellingWebsite.Models.Entities;
using FlowerSellingWebsite.Services.Interfaces;
using FlowerSellingWebsite.Repositories.Interfaces;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace FlowerSellingWebsite.Services.Implementations
{
    public class SupplierListingService : ISupplierListingService
    {
        private readonly IBaseRepository<SupplierListings> _supplierListing;

        public SupplierListingService(IBaseRepository<SupplierListings> supplierListing)
        {
            _supplierListing = supplierListing;
        }

        public async Task<bool> CreateSupplierListingAsync(SupplierListingDTO request)
        {

            var findExist = await _supplierListing.AsQueryable().Where(x => x.FlowerId == request.FlowerId && x.SupplierId == request.SupplierId).FirstOrDefaultAsync();

            if(findExist != null)
            {
                findExist.AvailableQuantity += request.AvailableQuantity;

                await _supplierListing.updateAsync(findExist);

                return true;
            }

            var entity = new SupplierListings
            {
                SupplierId = request.SupplierId,
                FlowerId = request.FlowerId,
                AvailableQuantity = request.AvailableQuantity,
                UnitPrice = request.UnitPrice,
                ShelfLifeDays = request.ShelfLifeDays,
                MinOrderQty = request.MinOrderQty,
                Status = request.Status ?? "Pending"
            };

            await _supplierListing.createAsync(entity);
            return true;
        }

        public async Task<SupplierListingDTO> GetSupplierListingDetail(int supplierId)
        {
            var entity = await _supplierListing.AsQueryable()
                .Include(x => x.Supplier)
                .Include(x => x.Flower)
                .FirstOrDefaultAsync(x => x.SupplierId == supplierId);

            if (entity == null) return null!;

            return new SupplierListingDTO
            {
                SupplierId = entity.SupplierId,
                FlowerId = entity.FlowerId,
                AvailableQuantity = entity.AvailableQuantity,
                UnitPrice = entity.UnitPrice,
                ShelfLifeDays = entity.ShelfLifeDays,
                MinOrderQty = entity.MinOrderQty,
                Status = entity.Status
            };
        }

        public async Task<PagedResult<SupplierListingDTO>> ListSupplierListing(UrlQueryParams urlQueryParams)
        {
            var query = _supplierListing.AsQueryable()
                .Include(x => x.Supplier)
                .Include(x => x.Flower)
                .AsQueryable();

            // Filtering
            if (!string.IsNullOrEmpty(urlQueryParams.SearchBy))
            {
                query = query.Where(x => x.Flower.Name.Contains(urlQueryParams.SearchValue) ||
                                         x.Supplier.SupplierName.Contains(urlQueryParams.SearchValue));
            }

            // Paging
            var totalRecords = await query.CountAsync();
            var items = await query
                .Skip((urlQueryParams.Page - 1) * urlQueryParams.PageSize)
                .Take(urlQueryParams.PageSize)
                .Select(entity => new SupplierListingDTO
                {
                    SupplierId = entity.SupplierId,
                    FlowerId = entity.FlowerId,
                    AvailableQuantity = entity.AvailableQuantity,
                    UnitPrice = entity.UnitPrice,
                    ShelfLifeDays = entity.ShelfLifeDays,
                    MinOrderQty = entity.MinOrderQty,
                    Status = entity.Status
                })
                .ToListAsync();

            return new PagedResult<SupplierListingDTO>
            {
                Items = items,
                TotalItems = totalRecords,
                Page = urlQueryParams.Page,
                PageSize = urlQueryParams.PageSize
            };
        }

        public async Task<bool> RemoveSupplierListingAsync(int supplierId, int flowerId)
        {
            var entity = await _supplierListing.AsQueryable()
                .FirstOrDefaultAsync(x => x.SupplierId == supplierId && x.FlowerId == flowerId);

            if (entity == null) return false;

            await _supplierListing.deleteAsync(entity);
            return true;
        }

        public async Task<bool> UpdateSupplierListingAsync(SupplierListingDTO request)
        {
            var entity = await _supplierListing.AsQueryable()
                .FirstOrDefaultAsync(x => x.SupplierId == request.SupplierId && x.FlowerId == request.FlowerId);

            if (entity == null) return false;

            if(request.AvailableQuantity < 0)
            {
                await _supplierListing.deleteAsync(entity);
                return true;
            }

            entity.AvailableQuantity = request.AvailableQuantity;
            entity.UnitPrice = request.UnitPrice;
            entity.ShelfLifeDays = request.ShelfLifeDays;
            entity.MinOrderQty = request.MinOrderQty;
            entity.Status = request.Status;

            await _supplierListing.updateAsync(entity);
            return true;
        }
    }
}
