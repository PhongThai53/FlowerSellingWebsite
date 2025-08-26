using FlowerSellingWebsite.Models.DTOs;
using FlowerSellingWebsite.Models.DTOs.SupplierListing;
using FlowerSellingWebsite.Models.Entities;
using FlowerSellingWebsite.Repositories.Interfaces;
using FlowerSellingWebsite.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FlowerSellingWebsite.Services.Implementations
{
    public class SupplierListingService : ISupplierListingService
    {
        private readonly IBaseRepository<SupplierListings> _supplierListingRepository;
        private readonly IBaseRepository<Flowers> _flowerRepository;
        private readonly IBaseRepository<FlowerCategories> _categoryRepository;
        private readonly IBaseRepository<FlowerTypes> _typeRepository;
        private readonly IBaseRepository<FlowerColors> _colorRepository;

        public SupplierListingService(
            IBaseRepository<SupplierListings> supplierListingRepository,
            IBaseRepository<Flowers> flowerRepository,
            IBaseRepository<FlowerCategories> categoryRepository,
            IBaseRepository<FlowerTypes> typeRepository,
            IBaseRepository<FlowerColors> colorRepository)
        {
            _supplierListingRepository = supplierListingRepository;
            _flowerRepository = flowerRepository;
            _categoryRepository = categoryRepository;
            _typeRepository = typeRepository;
            _colorRepository = colorRepository;
        }

        public async Task<bool> CreateAsync(CreateSupplierListingDTO request)
        {
            try
            {
                // Kiểm tra xem hoa đã tồn tại trong kho của supplier chưa
                var existingListing = await _supplierListingRepository.AsQueryable()
                    .FirstOrDefaultAsync(x => x.SupplierId == request.SupplierId && x.FlowerId == request.FlowerId);

                if (existingListing != null)
                {
                    // Nếu đã tồn tại, cập nhật số lượng và giá
                    existingListing.AvailableQuantity += request.AvailableQuantity;
                    existingListing.UnitPrice = request.UnitPrice;
                    existingListing.ShelfLifeDays = request.ShelfLifeDays;
                    existingListing.MinOrderQty = request.MinOrderQty;
                    existingListing.Status = request.Status;
                    
                    await _supplierListingRepository.updateAsync(existingListing);
                }
                else
                {
                    // Nếu chưa tồn tại, tạo mới
                    var newListing = new SupplierListings
                    {
                        SupplierId = request.SupplierId,
                        FlowerId = request.FlowerId,
                        AvailableQuantity = request.AvailableQuantity,
                        UnitPrice = request.UnitPrice,
                        ShelfLifeDays = request.ShelfLifeDays,
                        MinOrderQty = request.MinOrderQty,
                        Status = request.Status
                    };

                    await _supplierListingRepository.createAsync(newListing);
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<PagedResult<SupplierListingResponseDTO>> GetListAsync(SupplierListingListRequestDTO request)
        {
            try
            {
                var query = _supplierListingRepository.AsQueryable()
                    .Include(x => x.Flower)
                    .ThenInclude(f => f.FlowerCategory)
                    .Include(x => x.Flower)
                    .ThenInclude(f => f.FlowerType)
                    .Include(x => x.Flower)
                    .ThenInclude(f => f.FlowerColor);

                // Lấy tổng số records
                var totalRecords = await query.CountAsync();

                // Áp dụng paging
                var items = await query
                    .Skip((request.PageIndex - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .ToListAsync();

                // Map sang DTO
                var responseItems = items.Select(item => new SupplierListingResponseDTO
                {
                    SupplierId = item.SupplierId,
                    FlowerId = item.FlowerId,
                    AvailableQuantity = item.AvailableQuantity,
                    UnitPrice = item.UnitPrice,
                    ShelfLifeDays = item.ShelfLifeDays,
                    MinOrderQty = item.MinOrderQty,
                    Status = item.Status,
                    FlowerName = item.Flower?.Name,
                    FlowerDescription = item.Flower?.Description,
                    FlowerSize = item.Flower?.Size,
                    CategoryName = item.Flower?.FlowerCategory?.CategoryName,
                    TypeName = item.Flower?.FlowerType?.TypeName,
                    ColorName = item.Flower?.FlowerColor?.ColorName
                }).ToList();

                var totalPages = (int)Math.Ceiling((double)totalRecords / request.PageSize);

                return new PagedResult<SupplierListingResponseDTO>
                {
                    Items = responseItems,
                    TotalItems = totalRecords,
                    Page = request.PageIndex,
                    PageSize = request.PageSize,
                    TotalPages = totalPages
                    // HasNext và HasPrev sẽ tự động được tính toán từ Page và TotalPages
                };
            }
            catch (Exception)
            {
                return new PagedResult<SupplierListingResponseDTO>
                {
                    Items = new List<SupplierListingResponseDTO>(),
                    TotalItems = 0,
                    Page = request.PageIndex,
                    PageSize = request.PageSize,
                    TotalPages = 0
                    // HasNext và HasPrev sẽ tự động được tính toán từ Page và TotalPages
                };
            }
        }
    }
}
