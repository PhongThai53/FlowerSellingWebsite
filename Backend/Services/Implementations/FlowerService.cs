using AutoMapper;
using FlowerSellingWebsite.Models.DTOs;
using FlowerSellingWebsite.Models.Entities;
using FlowerSellingWebsite.Services.Interfaces;
using FlowerSellingWebsite.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace FlowerSellingWebsite.Services.Implementations
{
    public class FlowerService : IFlowerService
    {
        private readonly IBaseRepository<Flowers> _flowerRepo;
        private readonly IBaseRepository<ProductFlowers> _productFlowerRepo;
        private readonly IBaseRepository<FlowerPricing> _flowerPricingRepo;
        private readonly IBaseRepository<FlowerImages> _flowerImage;
        private readonly IBaseRepository<SupplierListings> _supplierListingRepository;
        
        private readonly IMapper _mapper;

        public FlowerService(IBaseRepository<Flowers> flowerRepo, IBaseRepository<FlowerPricing> flowerPricingRepo, IBaseRepository<FlowerImages> flowerImage, IMapper mapper, IBaseRepository<SupplierListings> supplierListingRepository, IBaseRepository<ProductFlowers> productFlowerRepo)
        {
            _flowerRepo = flowerRepo;
            _mapper = mapper;
            _supplierListingRepository = supplierListingRepository;
            _productFlowerRepo = productFlowerRepo;
            _flowerPricingRepo = flowerPricingRepo;
            _flowerImage = flowerImage;
        }

        public async Task<bool> CreateFlowerWithSupplier(CreateSupplierListingRequest request, int supplierId)
        {
            try
            {
                var flower = await _flowerRepo.AsQueryable().FirstOrDefaultAsync(x => x.Id == request.ProductFlowers.FlowerId);

                var supplierListing = new SupplierListings
                {
                    SupplierId = supplierId,
                    FlowerId = flower.Id,
                    AvailableQuantity = request.ProductFlowers.Quantity,
                    UnitPrice = request.FlowerPriceRequests.Price,
                    ShelfLifeDays = request.ShelfLifeDays,
                    MinOrderQty = request.MinOrderQty,
                    Status = "pending"
                };

                var historyPrice = new FlowerPricing
                {
                    FlowerId = flower.Id,
                    Price = request.FlowerPriceRequests.Price,
                    Currency = "VND",
                    EffectiveDate = request.FlowerPriceRequests.EffectiveDate,
                    ExpiryDate = request.FlowerPriceRequests.ExpiryDate,
                    PriceType = "type",
                    IsActive = true
                };

                await _supplierListingRepository.createAsync(supplierListing);
                await _flowerPricingRepo.createAsync(historyPrice);

                return true;
            } catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<bool> CreateFlowerAsync(CreateFlowerWithSupplierRequest request, int supplerId)
        {
            try
            {
                var isExist = await _flowerRepo.AsQueryable().AnyAsync(x => x.Name == request.Name);

                if (isExist) return false;

                var newFlower = new Flowers
                {
                    Name = request.Name,
                    Description = request.Description,
                    FlowerCategoryId = request.FlowerCategoryId,
                    FlowerTypeId = request.FlowerTypeId,
                    FlowerColorId = request.FlowerColorId,
                    Size = request.Size
                };

                var rsflower = await _flowerRepo.createAsync(newFlower);

                var newImage = new FlowerImages
                {
                    FlowerId = rsflower.Id,
                    ImageUrl = request.FlowerImageRequests.ImageUrl,
                    ImageType = request.FlowerImageRequests.ImageType,
                    IsPrimary = request.FlowerImageRequests.IsPrimary,
                    DisplayOrder = request.FlowerImageRequests.DisplayOrder,
                    IsActive = true
                };

                var rsImage = await _flowerImage.createAsync(newImage);

                var historyPrice = new FlowerPricing
                {
                    FlowerId = rsflower.Id,
                    Price = request.FlowerPriceRequests.Price,
                    Currency = "VND",
                    EffectiveDate = request.FlowerPriceRequests.EffectiveDate,
                    ExpiryDate = request.FlowerPriceRequests.ExpiryDate,
                    PriceType = "type",
                    IsActive = true
                };

                await _flowerPricingRepo.createAsync(historyPrice);

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<PagedResult<FlowerResponse>> GetListFlowerAsync(UrlQueryParams queryParams, int? supplierId = null)
        {
            IQueryable<Flowers> flowerQuery = _flowerRepo.AsQueryable()
                .Include(x => x.FlowerCategory)
                .Include(x => x.FlowerType)
                .Include(x => x.FlowerColor);

            if (supplierId.HasValue)
            {
                //flowerQuery = flowerQuery.Where(x => x.SupplierId == supplierId.Value);
            }

            if (!string.IsNullOrEmpty(queryParams.SearchBy) && !string.IsNullOrEmpty(queryParams.SearchValue))
            {
                flowerQuery = queryParams.SearchBy.ToLower() switch
                {
                    "category" => flowerQuery.Where(x => x.FlowerCategory.CategoryName.Contains(queryParams.SearchValue)),
                    "size" => flowerQuery.Where(x => x.Size.Contains(queryParams.SearchValue)),
                    "type" => flowerQuery.Where(x => x.FlowerType.TypeName.Contains(queryParams.SearchValue)),
                    "color" => flowerQuery.Where(x => x.FlowerColor.ColorName.Contains(queryParams.SearchValue)),
                    _ => flowerQuery
                };
            }

            // Get total count before paging
            var totalRecords = await flowerQuery.CountAsync();

            // Apply paging
            var items = await flowerQuery
                .Skip((queryParams.Page - 1) * queryParams.PageSize)
                .Take(queryParams.PageSize)
                .ToListAsync();

            return new PagedResult<FlowerResponse>
            {
                Items = _mapper.Map<List<FlowerResponse>>(items),
                TotalItems = totalRecords,
                Page = queryParams.Page,
                PageSize = queryParams.PageSize
            };
        }

    }
}
