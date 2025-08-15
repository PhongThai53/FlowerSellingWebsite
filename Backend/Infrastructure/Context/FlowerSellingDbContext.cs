using FlowerSellingWebsite.Models.Entities;

namespace FlowerSelling.Data
{
    using Microsoft.EntityFrameworkCore;

    namespace FlowerSellingWebsite.Data
    {
        public class FlowerSellingDbContext : DbContext
        {
            public FlowerSellingDbContext(DbContextOptions<FlowerSellingDbContext> options) : base(options)
            {
            }

            public DbSet<Users> Users { get; set; }
            public DbSet<Roles> Roles { get; set; }
            public DbSet<Permissions> Permissions { get; set; }
            public DbSet<RolePermissions> RolePermissions { get; set; }
            public DbSet<FlowerCategories> FlowerCategories { get; set; }
            public DbSet<FlowerTypes> FlowerTypes { get; set; }
            public DbSet<FlowerColors> FlowerColors { get; set; }
            public DbSet<Flowers> Flowers { get; set; }
            public DbSet<FlowerBatches> FlowerBatches { get; set; }
            public DbSet<FlowerDamageLogs> FlowerDamageLogs { get; set; }
            public DbSet<FlowerPricing> FlowerPricing { get; set; }
            public DbSet<FlowerPriceHistory> FlowerPriceHistory { get; set; }
            public DbSet<FlowerCategoryImages> FlowerCategoryImages { get; set; }
            public DbSet<Products> Products { get; set; }
            public DbSet<ProductPhotos> ProductPhotos { get; set; }
            public DbSet<ProductFlowers> ProductFlowers { get; set; }
            public DbSet<Suppliers> Suppliers { get; set; }
            public DbSet<SupplierListings> SupplierListings { get; set; }
            public DbSet<SupplierListingPhotos> SupplierListingPhotos { get; set; }
            public DbSet<PurchaseOrders> PurchaseOrders { get; set; }
            public DbSet<PurchaseOrderDetails> PurchaseOrderDetails { get; set; }
            public DbSet<Orders> Orders { get; set; }
            public DbSet<OrderDetails> OrderDetails { get; set; }
            public DbSet<PaymentMethods> PaymentMethods { get; set; }
            public DbSet<Payments> Payments { get; set; }
            public DbSet<Deliveries> Deliveries { get; set; }
            public DbSet<Blog> Blogs { get; set; }
            public DbSet<Comment> Comments { get; set; }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                base.OnModelCreating(modelBuilder);

                // Soft delete filter cho từng entity cụ thể
                modelBuilder.Entity<Users>().HasQueryFilter(e => !e.IsDeleted);
                modelBuilder.Entity<Roles>().HasQueryFilter(e => !e.IsDeleted);
                modelBuilder.Entity<Permissions>().HasQueryFilter(e => !e.IsDeleted);
                modelBuilder.Entity<RolePermissions>().HasQueryFilter(e => !e.IsDeleted);
                modelBuilder.Entity<FlowerCategories>().HasQueryFilter(e => !e.IsDeleted);
                modelBuilder.Entity<FlowerTypes>().HasQueryFilter(e => !e.IsDeleted);
                modelBuilder.Entity<FlowerColors>().HasQueryFilter(e => !e.IsDeleted);
                modelBuilder.Entity<Flowers>().HasQueryFilter(e => !e.IsDeleted);
                modelBuilder.Entity<FlowerBatches>().HasQueryFilter(e => !e.IsDeleted);
                modelBuilder.Entity<FlowerDamageLogs>().HasQueryFilter(e => !e.IsDeleted);
                modelBuilder.Entity<FlowerPricing>().HasQueryFilter(e => !e.IsDeleted);
                modelBuilder.Entity<FlowerPriceHistory>().HasQueryFilter(e => !e.IsDeleted);
                modelBuilder.Entity<FlowerCategoryImages>().HasQueryFilter(e => !e.IsDeleted);
                modelBuilder.Entity<Products>().HasQueryFilter(e => !e.IsDeleted);
                modelBuilder.Entity<ProductPhotos>().HasQueryFilter(e => !e.IsDeleted);
                modelBuilder.Entity<ProductFlowers>().HasQueryFilter(e => !e.IsDeleted);
                modelBuilder.Entity<Suppliers>().HasQueryFilter(e => !e.IsDeleted);
                modelBuilder.Entity<SupplierListings>().HasQueryFilter(e => !e.IsDeleted);
                modelBuilder.Entity<SupplierListingPhotos>().HasQueryFilter(e => !e.IsDeleted);
                modelBuilder.Entity<PurchaseOrders>().HasQueryFilter(e => !e.IsDeleted);
                modelBuilder.Entity<PurchaseOrderDetails>().HasQueryFilter(e => !e.IsDeleted);
                modelBuilder.Entity<Orders>().HasQueryFilter(e => !e.IsDeleted);
                modelBuilder.Entity<OrderDetails>().HasQueryFilter(e => !e.IsDeleted);
                modelBuilder.Entity<PaymentMethods>().HasQueryFilter(e => !e.IsDeleted);
                modelBuilder.Entity<Payments>().HasQueryFilter(e => !e.IsDeleted);
                modelBuilder.Entity<Deliveries>().HasQueryFilter(e => !e.IsDeleted);
                modelBuilder.Entity<Blog>().HasQueryFilter(e => !e.IsDeleted);
                modelBuilder.Entity<Comment>().HasQueryFilter(e => !e.IsDeleted);

                // BaseEntity configurations - không config cho abstract class
                // Các config này sẽ được inherit bởi các entity con

                // Users
                modelBuilder.Entity<Users>()
                    .Property(e => e.UserName)
                    .HasColumnType("nvarchar(100)")
                    .IsRequired();

                modelBuilder.Entity<Users>()
                    .Property(e => e.PasswordHash)
                    .HasColumnType("nvarchar(500)")
                    .IsRequired();

                modelBuilder.Entity<Users>()
                    .Property(e => e.FullName)
                    .HasColumnType("nvarchar(200)")
                    .IsRequired();

                modelBuilder.Entity<Users>()
                    .Property(e => e.Email)
                    .HasColumnType("nvarchar(200)")
                    .IsRequired();

                modelBuilder.Entity<Users>()
                    .Property(e => e.Phone)
                    .HasColumnType("nvarchar(20)");

                modelBuilder.Entity<Users>()
                    .Property(e => e.Address)
                    .HasColumnType("nvarchar(500)");

                modelBuilder.Entity<Users>()
                    .Property(e => e.RoleId)
                    .HasColumnType("int")
                    .IsRequired();

                // Roles
                modelBuilder.Entity<Roles>()
                    .Property(e => e.RoleName)
                    .HasColumnType("nvarchar(50)")
                    .IsRequired();

                modelBuilder.Entity<Roles>()
                    .Property(e => e.Description)
                    .HasColumnType("nvarchar(200)");

                // Permissions
                modelBuilder.Entity<Permissions>()
                    .Property(e => e.PermissionName)
                    .HasColumnType("nvarchar(100)")
                    .IsRequired();

                modelBuilder.Entity<Permissions>()
                    .Property(e => e.Description)
                    .HasColumnType("nvarchar(200)");

                // RolePermissions
                modelBuilder.Entity<RolePermissions>()
                    .Property(e => e.RoleId)
                    .HasColumnType("int")
                    .IsRequired();

                modelBuilder.Entity<RolePermissions>()
                    .Property(e => e.PermissionId)
                    .HasColumnType("int")
                    .IsRequired();

                // FlowerCategories
                modelBuilder.Entity<FlowerCategories>()
                    .Property(e => e.CategoryName)
                    .HasColumnType("nvarchar(100)")
                    .IsRequired();

                modelBuilder.Entity<FlowerCategories>()
                    .Property(e => e.Description)
                    .HasColumnType("nvarchar(500)");

                // FlowerTypes
                modelBuilder.Entity<FlowerTypes>()
                    .Property(e => e.TypeName)
                    .HasColumnType("nvarchar(100)")
                    .IsRequired();

                modelBuilder.Entity<FlowerTypes>()
                    .Property(e => e.Description)
                    .HasColumnType("nvarchar(500)");

                // FlowerColors
                modelBuilder.Entity<FlowerColors>()
                    .Property(e => e.ColorName)
                    .HasColumnType("nvarchar(100)")
                    .IsRequired();

                modelBuilder.Entity<FlowerColors>()
                    .Property(e => e.HexCode)
                    .HasColumnType("nvarchar(10)")
                    .IsRequired();

                modelBuilder.Entity<FlowerColors>()
                    .Property(e => e.Description)
                    .HasColumnType("nvarchar(500)");

                // Flowers
                modelBuilder.Entity<Flowers>()
                    .Property(e => e.Name)
                    .HasColumnType("nvarchar(200)")
                    .IsRequired();

                modelBuilder.Entity<Flowers>()
                    .Property(e => e.Description)
                    .HasColumnType("nvarchar(1000)");

                modelBuilder.Entity<Flowers>()
                    .Property(e => e.FlowerCategoryId)
                    .HasColumnType("int");

                modelBuilder.Entity<Flowers>()
                    .Property(e => e.FlowerTypeId)
                    .HasColumnType("int")
                    .IsRequired();

                modelBuilder.Entity<Flowers>()
                    .Property(e => e.FlowerColorId)
                    .HasColumnType("int")
                    .IsRequired();

                modelBuilder.Entity<Flowers>()
                    .Property(e => e.Size)
                    .HasColumnType("nvarchar(50)");

                modelBuilder.Entity<Flowers>()
                    .Property(e => e.ShelfLifeDays)
                    .HasColumnType("int")
                    .IsRequired();

                // FlowerBatches
                modelBuilder.Entity<FlowerBatches>()
                    .Property(e => e.FlowerId)
                    .HasColumnType("int")
                    .IsRequired();

                modelBuilder.Entity<FlowerBatches>()
                    .Property(e => e.SupplierId)
                    .HasColumnType("int")
                    .IsRequired();

                modelBuilder.Entity<FlowerBatches>()
                    .Property(e => e.BatchCode)
                    .HasColumnType("nvarchar(100)")
                    .IsRequired();

                modelBuilder.Entity<FlowerBatches>()
                    .Property(e => e.ImportDate)
                    .HasColumnType("datetime2")
                    .IsRequired();

                modelBuilder.Entity<FlowerBatches>()
                    .Property(e => e.ExpiryDate)
                    .HasColumnType("datetime2")
                    .IsRequired();

                modelBuilder.Entity<FlowerBatches>()
                    .Property(e => e.QuantityAvailable)
                    .HasColumnType("int")
                    .IsRequired();

                modelBuilder.Entity<FlowerBatches>()
                    .Property(e => e.UnitPrice)
                    .HasColumnType("decimal(10,2)")
                    .IsRequired();

                modelBuilder.Entity<FlowerBatches>()
                    .Property(e => e.TotalAmount)
                    .HasColumnType("decimal(10,2)")
                    .IsRequired();

                // FlowerDamageLogs
                modelBuilder.Entity<FlowerDamageLogs>()
                    .Property(e => e.FlowerBatchId)
                    .HasColumnType("int")
                    .IsRequired();

                modelBuilder.Entity<FlowerDamageLogs>()
                    .Property(e => e.ReportedByUserId)
                    .HasColumnType("int")
                    .IsRequired();

                modelBuilder.Entity<FlowerDamageLogs>()
                    .Property(e => e.DamagedQuantity)
                    .HasColumnType("int")
                    .IsRequired();

                modelBuilder.Entity<FlowerDamageLogs>()
                    .Property(e => e.DamageReason)
                    .HasColumnType("nvarchar(500)")
                    .IsRequired();

                modelBuilder.Entity<FlowerDamageLogs>()
                    .Property(e => e.DamageDate)
                    .HasColumnType("datetime2")
                    .IsRequired();

                modelBuilder.Entity<FlowerDamageLogs>()
                    .Property(e => e.Notes)
                    .HasColumnType("nvarchar(1000)");

                // FlowerPricing
                modelBuilder.Entity<FlowerPricing>()
                    .Property(e => e.FlowerCategoryId)
                    .HasColumnType("int");

                modelBuilder.Entity<FlowerPricing>()
                    .Property(e => e.FlowerTypeId)
                    .HasColumnType("int");

                modelBuilder.Entity<FlowerPricing>()
                    .Property(e => e.FlowerColorId)
                    .HasColumnType("int");

                modelBuilder.Entity<FlowerPricing>()
                    .Property(e => e.Price)
                    .HasColumnType("decimal(10,2)")
                    .IsRequired();

                modelBuilder.Entity<FlowerPricing>()
                    .Property(e => e.Currency)
                    .HasColumnType("nvarchar(3)")
                    .IsRequired();

                modelBuilder.Entity<FlowerPricing>()
                    .Property(e => e.EffectiveDate)
                    .HasColumnType("datetime2")
                    .IsRequired();

                modelBuilder.Entity<FlowerPricing>()
                    .Property(e => e.ExpiryDate)
                    .HasColumnType("datetime2");

                modelBuilder.Entity<FlowerPricing>()
                    .Property(e => e.PriceType)
                    .HasColumnType("nvarchar(20)")
                    .IsRequired();

                modelBuilder.Entity<FlowerPricing>()
                    .Property(e => e.Priority)
                    .HasColumnType("int")
                    .IsRequired();

                modelBuilder.Entity<FlowerPricing>()
                    .Property(e => e.IsActive)
                    .HasColumnType("bit")
                    .IsRequired();

                // FlowerPriceHistory
                modelBuilder.Entity<FlowerPriceHistory>()
                    .Property(e => e.FlowerPricingId)
                    .HasColumnType("int")
                    .IsRequired();

                modelBuilder.Entity<FlowerPriceHistory>()
                    .Property(e => e.OldPrice)
                    .HasColumnType("decimal(10,2)")
                    .IsRequired();

                modelBuilder.Entity<FlowerPriceHistory>()
                    .Property(e => e.NewPrice)
                    .HasColumnType("decimal(10,2)")
                    .IsRequired();

                modelBuilder.Entity<FlowerPriceHistory>()
                    .Property(e => e.ChangeReason)
                    .HasColumnType("nvarchar(500)");

                modelBuilder.Entity<FlowerPriceHistory>()
                    .Property(e => e.ChangedByUserId)
                    .HasColumnType("int")
                    .IsRequired();

                modelBuilder.Entity<FlowerPriceHistory>()
                    .Property(e => e.ChangeDate)
                    .HasColumnType("datetime2")
                    .IsRequired();

                // FlowerCategoryImages
                modelBuilder.Entity<FlowerCategoryImages>()
                    .Property(e => e.FlowerCategoryId)
                    .HasColumnType("int");

                modelBuilder.Entity<FlowerCategoryImages>()
                    .Property(e => e.FlowerTypeId)
                    .HasColumnType("int");

                modelBuilder.Entity<FlowerCategoryImages>()
                    .Property(e => e.FlowerColorId)
                    .HasColumnType("int");

                modelBuilder.Entity<FlowerCategoryImages>()
                    .Property(e => e.ImageUrl)
                    .HasColumnType("nvarchar(500)")
                    .IsRequired();

                modelBuilder.Entity<FlowerCategoryImages>()
                    .Property(e => e.ImageType)
                    .HasColumnType("nvarchar(50)")
                    .IsRequired();

                modelBuilder.Entity<FlowerCategoryImages>()
                    .Property(e => e.Priority)
                    .HasColumnType("int")
                    .IsRequired();

                modelBuilder.Entity<FlowerCategoryImages>()
                    .Property(e => e.IsPrimary)
                    .HasColumnType("bit")
                    .IsRequired();

                modelBuilder.Entity<FlowerCategoryImages>()
                    .Property(e => e.DisplayOrder)
                    .HasColumnType("int")
                    .IsRequired();

                modelBuilder.Entity<FlowerCategoryImages>()
                    .Property(e => e.EffectiveDate)
                    .HasColumnType("datetime2")
                    .IsRequired();

                modelBuilder.Entity<FlowerCategoryImages>()
                    .Property(e => e.ExpiryDate)
                    .HasColumnType("datetime2");

                modelBuilder.Entity<FlowerCategoryImages>()
                    .Property(e => e.IsActive)
                    .HasColumnType("bit")
                    .IsRequired();

                // Products
                modelBuilder.Entity<Products>()
                    .Property(e => e.Name)
                    .HasColumnType("nvarchar(200)")
                    .IsRequired();

                modelBuilder.Entity<Products>()
                    .Property(e => e.Description)
                    .HasColumnType("nvarchar(1000)");

                modelBuilder.Entity<Products>()
                    .Property(e => e.Category)
                    .HasColumnType("nvarchar(100)");

                // ProductPhotos
                modelBuilder.Entity<ProductPhotos>()
                    .Property(e => e.ProductId)
                    .HasColumnType("int")
                    .IsRequired();

                modelBuilder.Entity<ProductPhotos>()
                    .Property(e => e.Url)
                    .HasColumnType("nvarchar(500)")
                    .IsRequired();

                modelBuilder.Entity<ProductPhotos>()
                    .Property(e => e.IsPrimary)
                    .HasColumnType("bit")
                    .IsRequired();

                // ProductFlowers
                modelBuilder.Entity<ProductFlowers>()
                    .Property(e => e.ProductId)
                    .HasColumnType("int")
                    .IsRequired();

                modelBuilder.Entity<ProductFlowers>()
                    .Property(e => e.FlowerId)
                    .HasColumnType("int")
                    .IsRequired();

                modelBuilder.Entity<ProductFlowers>()
                    .Property(e => e.QuantityNeeded)
                    .HasColumnType("int")
                    .IsRequired();

                // Suppliers
                modelBuilder.Entity<Suppliers>()
                    .Property(e => e.SupplierName)
                    .HasColumnType("nvarchar(200)")
                    .IsRequired();

                modelBuilder.Entity<Suppliers>()
                    .Property(e => e.ContactPerson)
                    .HasColumnType("nvarchar(200)");

                modelBuilder.Entity<Suppliers>()
                    .Property(e => e.Phone)
                    .HasColumnType("nvarchar(20)");

                modelBuilder.Entity<Suppliers>()
                    .Property(e => e.Email)
                    .HasColumnType("nvarchar(200)");

                modelBuilder.Entity<Suppliers>()
                    .Property(e => e.Address)
                    .HasColumnType("nvarchar(500)");

                // SupplierListings
                modelBuilder.Entity<SupplierListings>()
                    .Property(e => e.SupplierId)
                    .HasColumnType("int")
                    .IsRequired();

                modelBuilder.Entity<SupplierListings>()
                    .Property(e => e.ProductId)
                    .HasColumnType("int")
                    .IsRequired();

                modelBuilder.Entity<SupplierListings>()
                    .Property(e => e.AvailableQuantity)
                    .HasColumnType("int")
                    .IsRequired();

                modelBuilder.Entity<SupplierListings>()
                    .Property(e => e.UnitPrice)
                    .HasColumnType("decimal(10,2)")
                    .IsRequired();

                modelBuilder.Entity<SupplierListings>()
                    .Property(e => e.ShelfLifeDays)
                    .HasColumnType("int")
                    .IsRequired();

                modelBuilder.Entity<SupplierListings>()
                    .Property(e => e.MinOrderQty)
                    .HasColumnType("int")
                    .IsRequired();

                modelBuilder.Entity<SupplierListings>()
                    .Property(e => e.Status)
                    .HasColumnType("nvarchar(50)")
                    .IsRequired();

                // SupplierListingPhotos
                modelBuilder.Entity<SupplierListingPhotos>()
                    .Property(e => e.SupplierListingId)
                    .HasColumnType("int")
                    .IsRequired();

                modelBuilder.Entity<SupplierListingPhotos>()
                    .Property(e => e.Url)
                    .HasColumnType("nvarchar(500)")
                    .IsRequired();

                modelBuilder.Entity<SupplierListingPhotos>()
                    .Property(e => e.IsPrimary)
                    .HasColumnType("bit")
                    .IsRequired();

                // PurchaseOrders
                modelBuilder.Entity<PurchaseOrders>()
                    .Property(e => e.PurchaseOrderNumber)
                    .HasColumnType("nvarchar(100)")
                    .IsRequired();

                modelBuilder.Entity<PurchaseOrders>()
                    .Property(e => e.SupplierId)
                    .HasColumnType("int")
                    .IsRequired();

                modelBuilder.Entity<PurchaseOrders>()
                    .Property(e => e.CreatedDate)
                    .HasColumnType("datetime2")
                    .IsRequired();

                modelBuilder.Entity<PurchaseOrders>()
                    .Property(e => e.Status)
                    .HasColumnType("nvarchar(50)")
                    .IsRequired();

                modelBuilder.Entity<PurchaseOrders>()
                    .Property(e => e.TotalAmount)
                    .HasColumnType("decimal(10,2)")
                    .IsRequired();

                modelBuilder.Entity<PurchaseOrders>()
                    .Property(e => e.Notes)
                    .HasColumnType("nvarchar(1000)");

                // PurchaseOrderDetails
                modelBuilder.Entity<PurchaseOrderDetails>()
                    .Property(e => e.PurchaseOrderId)
                    .HasColumnType("int")
                    .IsRequired();

                modelBuilder.Entity<PurchaseOrderDetails>()
                    .Property(e => e.FlowerId)
                    .HasColumnType("int")
                    .IsRequired();

                modelBuilder.Entity<PurchaseOrderDetails>()
                    .Property(e => e.Quantity)
                    .HasColumnType("int")
                    .IsRequired();

                modelBuilder.Entity<PurchaseOrderDetails>()
                    .Property(e => e.UnitPrice)
                    .HasColumnType("decimal(10,2)")
                    .IsRequired();

                modelBuilder.Entity<PurchaseOrderDetails>()
                    .Property(e => e.LineTotal)
                    .HasColumnType("decimal(10,2)")
                    .IsRequired();

                // Orders
                modelBuilder.Entity<Orders>()
                    .Property(e => e.OrderNumber)
                    .HasColumnType("nvarchar(100)")
                    .IsRequired();

                modelBuilder.Entity<Orders>()
                    .Property(e => e.CustomerId)
                    .HasColumnType("int")
                    .IsRequired();

                modelBuilder.Entity<Orders>()
                    .Property(e => e.CreatedDate)
                    .HasColumnType("datetime2")
                    .IsRequired();

                modelBuilder.Entity<Orders>()
                    .Property(e => e.Status)
                    .HasColumnType("nvarchar(50)")
                    .IsRequired();

                modelBuilder.Entity<Orders>()
                    .Property(e => e.Subtotal)
                    .HasColumnType("decimal(10,2)")
                    .IsRequired();

                modelBuilder.Entity<Orders>()
                    .Property(e => e.TaxAmount)
                    .HasColumnType("decimal(10,2)")
                    .IsRequired();

                modelBuilder.Entity<Orders>()
                    .Property(e => e.Notes)
                    .HasColumnType("nvarchar(1000)");

                // OrderDetails
                modelBuilder.Entity<OrderDetails>()
                    .Property(e => e.OrderId)
                    .HasColumnType("int")
                    .IsRequired();

                modelBuilder.Entity<OrderDetails>()
                    .Property(e => e.ProductId)
                    .HasColumnType("int")
                    .IsRequired();

                modelBuilder.Entity<OrderDetails>()
                    .Property(e => e.Quantity)
                    .HasColumnType("int")
                    .IsRequired();

                modelBuilder.Entity<OrderDetails>()
                    .Property(e => e.UnitPrice)
                    .HasColumnType("decimal(10,2)")
                    .IsRequired();

                modelBuilder.Entity<OrderDetails>()
                    .Property(e => e.Discount)
                    .HasColumnType("decimal(10,2)")
                    .IsRequired();

                modelBuilder.Entity<OrderDetails>()
                    .Property(e => e.LineTotal)
                    .HasColumnType("decimal(10,2)")
                    .IsRequired();

                // PaymentMethods
                modelBuilder.Entity<PaymentMethods>()
                    .Property(e => e.MethodName)
                    .HasColumnType("nvarchar(100)")
                    .IsRequired();

                modelBuilder.Entity<PaymentMethods>()
                    .Property(e => e.Description)
                    .HasColumnType("nvarchar(500)");

                modelBuilder.Entity<PaymentMethods>()
                    .Property(e => e.IsActive)
                    .HasColumnType("bit")
                    .IsRequired();

                // Payments
                modelBuilder.Entity<Payments>()
                    .Property(e => e.OrderId)
                    .HasColumnType("int")
                    .IsRequired();

                modelBuilder.Entity<Payments>()
                    .Property(e => e.PaymentMethodId)
                    .HasColumnType("int")
                    .IsRequired();

                modelBuilder.Entity<Payments>()
                    .Property(e => e.Amount)
                    .HasColumnType("decimal(10,2)")
                    .IsRequired();

                modelBuilder.Entity<Payments>()
                    .Property(e => e.PaymentDate)
                    .HasColumnType("datetime2")
                    .IsRequired();

                modelBuilder.Entity<Payments>()
                    .Property(e => e.Status)
                    .HasColumnType("nvarchar(50)")
                    .IsRequired();

                // Deliveries
                modelBuilder.Entity<Deliveries>()
                    .Property(e => e.OrderId)
                    .HasColumnType("int")
                    .IsRequired();

                modelBuilder.Entity<Deliveries>()
                    .Property(e => e.DeliveryDate)
                    .HasColumnType("datetime2")
                    .IsRequired();

                modelBuilder.Entity<Deliveries>()
                    .Property(e => e.DeliveryStatus)
                    .HasColumnType("nvarchar(50)")
                    .IsRequired();

                modelBuilder.Entity<Deliveries>()
                    .Property(e => e.TrackingNumber)
                    .HasColumnType("nvarchar(100)");

                modelBuilder.Entity<Deliveries>()
                    .Property(e => e.ShipperName)
                    .HasColumnType("nvarchar(200)");

                // Relationships configuration
                modelBuilder.Entity<Blog>()
                    .HasOne(b => b.Category)
                    .WithMany(c => c.Blogs)
                    .HasForeignKey(b => b.CategoryId)
                    .OnDelete(DeleteBehavior.NoAction); // Thay đổi từ NoAction thành Restrict để tránh lỗi

                modelBuilder.Entity<Blog>()
                    .HasOne(b => b.User)
                    .WithMany()
                    .HasForeignKey(b => b.UserId)
                    .OnDelete(DeleteBehavior.NoAction);
                modelBuilder.Entity<Comment>()
                    .HasOne(c => c.Parent)
                    .WithMany(c => c.Children)
                    .HasForeignKey(c => c.ParentId)
                    .OnDelete(DeleteBehavior.NoAction);
                modelBuilder.Entity<Comment>()
                    .HasOne(c => c.Blog)
                    .WithMany(b => b.Comments)
                    .HasForeignKey(c => c.BlogId)
                    .OnDelete(DeleteBehavior.NoAction);
            }

            public override int SaveChanges()
            {
                UpdateTimestamps();
                return base.SaveChanges();
            }

            public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
            {
                UpdateTimestamps();
                return await base.SaveChangesAsync(cancellationToken);
            }

            private void UpdateTimestamps()
            {
                var entries = ChangeTracker.Entries<BaseEntity>();

                foreach (var entry in entries)
                {
                    switch (entry.State)
                    {
                        case EntityState.Added:
                            entry.Entity.CreatedAt = DateTime.UtcNow;
                            entry.Entity.PublicId = Guid.NewGuid();
                            break;

                        case EntityState.Modified:
                            entry.Entity.UpdatedAt = DateTime.UtcNow;
                            break;

                        case EntityState.Deleted:
                            entry.State = EntityState.Modified;
                            entry.Entity.IsDeleted = true;
                            entry.Entity.DeletedAt = DateTime.UtcNow;
                            entry.Entity.UpdatedAt = DateTime.UtcNow;
                            break;
                    }
                }
            }
        }
    }
}