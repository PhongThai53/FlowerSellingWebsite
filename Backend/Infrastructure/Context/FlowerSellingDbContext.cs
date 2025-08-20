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

            // DbSets
            public DbSet<Users> Users { get; set; }
            public DbSet<Roles> Roles { get; set; }
            public DbSet<Permissions> Permissions { get; set; }
            public DbSet<RolePermissions> RolePermissions { get; set; }

            public DbSet<Flowers> Flowers { get; set; }
            public DbSet<FlowerCategories> FlowerCategories { get; set; }
            public DbSet<FlowerTypes> FlowerTypes { get; set; }
            public DbSet<FlowerColors> FlowerColors { get; set; }
            public DbSet<FlowerImages> FlowerImages { get; set; }
            public DbSet<FlowerPricing> FlowerPricing { get; set; }
            public DbSet<FlowerPriceHistory> FlowerPriceHistory { get; set; }

            public DbSet<Products> Products { get; set; }
            public DbSet<ProductCategories> ProductCategories { get; set; }
            public DbSet<ProductPhotos> ProductPhotos { get; set; }
            public DbSet<ProductFlowers> ProductFlowers { get; set; }
            public DbSet<ProductPriceHistories> ProductPriceHistories { get; set; }

            public DbSet<Cart> Cart { get; set; }
            public DbSet<CartItem> CartItem { get; set; }
            public DbSet<Orders> Orders { get; set; }
            public DbSet<OrderDetails> OrderDetails { get; set; }

            public DbSet<Payments> Payments { get; set; }
            public DbSet<PaymentMethods> PaymentMethods { get; set; }
            public DbSet<Deliveries> Deliveries { get; set; }

            public DbSet<Suppliers> Suppliers { get; set; }
            public DbSet<SupplierListings> SupplierListings { get; set; }
            public DbSet<SupplierListingPhotos> SupplierListingPhotos { get; set; }
            public DbSet<PurchaseOrders> PurchaseOrders { get; set; }
            public DbSet<PurchaseOrderDetails> PurchaseOrderDetails { get; set; }
            public DbSet<FlowerDamageLogs> FlowerDamageLogs { get; set; }

            public DbSet<Blog> Blogs { get; set; }
            public DbSet<Comment> Comments { get; set; }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                base.OnModelCreating(modelBuilder);

                // ===== CONFIGURE DATA TYPES =====

                // BaseEntity properties for all entities
                ConfigureBaseEntityProperties<Users>(modelBuilder);
                ConfigureBaseEntityProperties<Roles>(modelBuilder);
                ConfigureBaseEntityProperties<Permissions>(modelBuilder);
                ConfigureBaseEntityProperties<RolePermissions>(modelBuilder);
                ConfigureBaseEntityProperties<Flowers>(modelBuilder);
                ConfigureBaseEntityProperties<FlowerCategories>(modelBuilder);
                ConfigureBaseEntityProperties<FlowerTypes>(modelBuilder);
                ConfigureBaseEntityProperties<FlowerColors>(modelBuilder);
                ConfigureBaseEntityProperties<FlowerImages>(modelBuilder);
                ConfigureBaseEntityProperties<FlowerPricing>(modelBuilder);
                ConfigureBaseEntityProperties<FlowerPriceHistory>(modelBuilder);
                ConfigureBaseEntityProperties<Products>(modelBuilder);
                ConfigureBaseEntityProperties<ProductCategories>(modelBuilder);
                ConfigureBaseEntityProperties<ProductPhotos>(modelBuilder);
                ConfigureBaseEntityProperties<ProductFlowers>(modelBuilder);
                ConfigureBaseEntityProperties<ProductPriceHistories>(modelBuilder);
                ConfigureBaseEntityProperties<Cart>(modelBuilder);
                ConfigureBaseEntityProperties<CartItem>(modelBuilder);
                ConfigureBaseEntityProperties<Orders>(modelBuilder);
                ConfigureBaseEntityProperties<OrderDetails>(modelBuilder);
                ConfigureBaseEntityProperties<Payments>(modelBuilder);
                ConfigureBaseEntityProperties<PaymentMethods>(modelBuilder);
                ConfigureBaseEntityProperties<Deliveries>(modelBuilder);
                ConfigureBaseEntityProperties<Suppliers>(modelBuilder);
                ConfigureBaseEntityProperties<SupplierListings>(modelBuilder);
                ConfigureBaseEntityProperties<SupplierListingPhotos>(modelBuilder);
                ConfigureBaseEntityProperties<PurchaseOrders>(modelBuilder);
                ConfigureBaseEntityProperties<PurchaseOrderDetails>(modelBuilder);
                ConfigureBaseEntityProperties<FlowerDamageLogs>(modelBuilder);
                ConfigureBaseEntityProperties<Blog>(modelBuilder);
                ConfigureBaseEntityProperties<Comment>(modelBuilder);

                // ===== USERS & ROLES =====
                modelBuilder.Entity<Users>(entity =>
                {
                    entity.Property(e => e.UserName).HasMaxLength(100).IsRequired();
                    entity.Property(e => e.PasswordHash).HasMaxLength(500).IsRequired();
                    entity.Property(e => e.FullName).HasMaxLength(200).IsRequired();
                    entity.Property(e => e.Email).HasMaxLength(200).IsRequired();
                    entity.Property(e => e.Phone).HasMaxLength(20);
                    entity.Property(e => e.Address).HasMaxLength(500);

                    entity.HasIndex(e => e.UserName).IsUnique();
                    entity.HasIndex(e => e.Email).IsUnique();
                });

                modelBuilder.Entity<Roles>(entity =>
                {
                    entity.Property(e => e.RoleName).HasMaxLength(50).IsRequired();
                    entity.Property(e => e.Description).HasMaxLength(200);
                });

                modelBuilder.Entity<Permissions>(entity =>
                {
                    entity.Property(e => e.PermissionName).HasMaxLength(100).IsRequired();
                    entity.Property(e => e.Description).HasMaxLength(200);
                });

                // ===== FLOWERS =====
                modelBuilder.Entity<Flowers>(entity =>
                {
                    entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
                    entity.Property(e => e.Description).HasMaxLength(1000);
                    entity.Property(e => e.Size).HasMaxLength(50);
                });

                modelBuilder.Entity<FlowerCategories>(entity =>
                {
                    entity.Property(e => e.CategoryName).HasMaxLength(100).IsRequired();
                    entity.Property(e => e.Description).HasMaxLength(500);
                });

                modelBuilder.Entity<FlowerTypes>(entity =>
                {
                    entity.Property(e => e.TypeName).HasMaxLength(100).IsRequired();
                    entity.Property(e => e.Description).HasMaxLength(500);
                });

                modelBuilder.Entity<FlowerColors>(entity =>
                {
                    entity.Property(e => e.ColorName).HasMaxLength(50).IsRequired();
                    entity.Property(e => e.HexCode).HasMaxLength(7).IsRequired();
                    entity.Property(e => e.Description).HasMaxLength(200);
                });

                modelBuilder.Entity<FlowerImages>(entity =>
                {
                    entity.Property(e => e.ImageUrl).HasMaxLength(500).IsRequired();
                    entity.Property(e => e.ImageType).HasMaxLength(50);
                });

                modelBuilder.Entity<FlowerPricing>(entity =>
                {
                    entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
                    entity.Property(e => e.Currency).HasMaxLength(3).IsRequired();
                    entity.Property(e => e.PriceType).HasMaxLength(20);
                });

                modelBuilder.Entity<FlowerPriceHistory>(entity =>
                {
                    entity.Property(e => e.OldPrice).HasColumnType("decimal(18,2)");
                    entity.Property(e => e.NewPrice).HasColumnType("decimal(18,2)");
                    entity.Property(e => e.ChangeReason).HasMaxLength(500);
                });

                // ===== PRODUCTS =====
                modelBuilder.Entity<Products>(entity =>
                {
                    entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
                    entity.Property(e => e.Description).HasMaxLength(1000);
                    entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
                    entity.Property(e => e.Url).HasMaxLength(500).IsRequired();
                });

                modelBuilder.Entity<ProductCategories>(entity =>
                {
                    entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
                    entity.Property(e => e.Description).HasMaxLength(500);
                });

                modelBuilder.Entity<ProductPhotos>(entity =>
                {
                    entity.Property(e => e.Url).HasMaxLength(500).IsRequired();
                });

                modelBuilder.Entity<ProductPriceHistories>(entity =>
                {
                    entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
                });

                // ===== CART & ORDERS =====
                modelBuilder.Entity<CartItem>(entity =>
                {
                    entity.Property(e => e.UnitPrice).HasColumnType("decimal(18,2)");
                    entity.Property(e => e.LineTotal).HasColumnType("decimal(18,2)");
                });

                modelBuilder.Entity<Orders>(entity =>
                {
                    entity.Property(e => e.OrderNumber).HasMaxLength(50).IsRequired();
                    entity.Property(e => e.Status).HasMaxLength(50).IsRequired();
                    entity.Property(e => e.Subtotal).HasColumnType("decimal(18,2)");
                    entity.Property(e => e.DiscountAmount).HasColumnType("decimal(18,2)");
                    entity.Property(e => e.TaxAmount).HasColumnType("decimal(18,2)");
                    entity.Property(e => e.ShippingFee).HasColumnType("decimal(18,2)");
                    entity.Property(e => e.TotalAmount).HasColumnType("decimal(18,2)");
                    entity.Property(e => e.PaymentStatus).HasMaxLength(50).IsRequired();
                    entity.Property(e => e.ShippingAddress).HasMaxLength(500);
                    entity.Property(e => e.BillingAddress).HasMaxLength(500);
                    entity.Property(e => e.Notes).HasMaxLength(1000);
                    entity.Property(e => e.SupplierNotes).HasMaxLength(1000);
                    entity.Property(e => e.CreatedBy).HasMaxLength(100);
                    entity.Property(e => e.UpdatedBy).HasMaxLength(100);

                    entity.HasIndex(e => e.OrderNumber).IsUnique();
                });

                modelBuilder.Entity<OrderDetails>(entity =>
                {
                    entity.Property(e => e.ItemName).HasMaxLength(200);
                    entity.Property(e => e.UnitPrice).HasColumnType("decimal(18,2)");
                    entity.Property(e => e.LineTotal).HasColumnType("decimal(18,2)");
                    entity.Property(e => e.Notes).HasMaxLength(500);
                });

                // ===== PAYMENTS & DELIVERY =====
                modelBuilder.Entity<Payments>(entity =>
                {
                    entity.Property(e => e.MethodName).HasMaxLength(100).IsRequired();
                    entity.Property(e => e.Description).HasMaxLength(500);
                    entity.Property(e => e.Amount).HasColumnType("decimal(18,2)");
                    entity.Property(e => e.Status).HasMaxLength(50).IsRequired();
                });

                modelBuilder.Entity<PaymentMethods>(entity =>
                {
                    entity.Property(e => e.MethodName).HasMaxLength(100).IsRequired();
                    entity.Property(e => e.Description).HasMaxLength(500);
                });

                modelBuilder.Entity<Deliveries>(entity =>
                {
                    entity.Property(e => e.DeliveryStatus).HasMaxLength(50).IsRequired();
                    entity.Property(e => e.TrackingNumber).HasMaxLength(100);
                    entity.Property(e => e.ShipperName).HasMaxLength(200);
                });

                // ===== SUPPLIERS =====
                modelBuilder.Entity<Suppliers>(entity =>
                {
                    entity.Property(e => e.SupplierName).HasMaxLength(200).IsRequired();
                    entity.Property(e => e.ContactPerson).HasMaxLength(200);
                    entity.Property(e => e.Phone).HasMaxLength(20);
                    entity.Property(e => e.Email).HasMaxLength(200);
                    entity.Property(e => e.Address).HasMaxLength(500);
                });

                modelBuilder.Entity<SupplierListings>(entity =>
                {
                    entity.Property(e => e.UnitPrice).HasColumnType("decimal(18,2)");
                    entity.Property(e => e.Status).HasMaxLength(50).IsRequired();
                });

                modelBuilder.Entity<SupplierListingPhotos>(entity =>
                {
                    entity.Property(e => e.Url).HasMaxLength(500).IsRequired();
                });

                modelBuilder.Entity<PurchaseOrders>(entity =>
                {
                    entity.Property(e => e.PurchaseOrderNumber).HasMaxLength(50).IsRequired();
                    entity.Property(e => e.Status).HasMaxLength(50).IsRequired();
                    entity.Property(e => e.TotalAmount).HasColumnType("decimal(18,2)");
                    entity.Property(e => e.Notes).HasMaxLength(1000);

                    entity.HasIndex(e => e.PurchaseOrderNumber).IsUnique();
                });

                modelBuilder.Entity<PurchaseOrderDetails>(entity =>
                {
                    entity.Property(e => e.UnitPrice).HasColumnType("decimal(18,2)");
                    entity.Property(e => e.LineTotal).HasColumnType("decimal(18,2)");
                });

                modelBuilder.Entity<FlowerDamageLogs>(entity =>
                {
                    entity.Property(e => e.DamageReason).HasMaxLength(500).IsRequired();
                    entity.Property(e => e.Notes).HasMaxLength(1000);
                });

                // ===== BLOG =====
                modelBuilder.Entity<Blog>(entity =>
                {
                    entity.Property(e => e.Title).HasMaxLength(250).IsRequired();
                    entity.Property(e => e.Content).HasColumnType("ntext").IsRequired();
                    entity.Property(e => e.Tags).HasMaxLength(1000);
                    entity.Property(e => e.Images).HasColumnType("nvarchar(max)"); // JSON string
                    entity.Property(e => e.Status).HasConversion<int>();
                    entity.Property(e => e.RejectionReason).HasMaxLength(1000);
                });

                modelBuilder.Entity<Comment>(entity =>
                {
                    entity.Property(e => e.Content).HasMaxLength(2000).IsRequired();
                });

                // ===== RELATIONSHIPS =====

                // Self-referencing Comment
                modelBuilder.Entity<Comment>()
                    .HasOne(c => c.Parent)
                    .WithMany(c => c.Children)
                    .HasForeignKey(c => c.ParentId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Composite Keys
                modelBuilder.Entity<RolePermissions>()
                    .HasKey(rp => new { rp.RoleId, rp.PermissionId });

                modelBuilder.Entity<ProductFlowers>()
                    .HasKey(pf => new { pf.ProductId, pf.FlowerId });

                // Global query filters for soft delete
                modelBuilder.Entity<Users>().HasQueryFilter(e => !e.IsDeleted);
                modelBuilder.Entity<Roles>().HasQueryFilter(e => !e.IsDeleted);
                modelBuilder.Entity<Permissions>().HasQueryFilter(e => !e.IsDeleted);
                modelBuilder.Entity<Flowers>().HasQueryFilter(e => !e.IsDeleted);
                modelBuilder.Entity<FlowerCategories>().HasQueryFilter(e => !e.IsDeleted);
                modelBuilder.Entity<FlowerTypes>().HasQueryFilter(e => !e.IsDeleted);
                modelBuilder.Entity<FlowerColors>().HasQueryFilter(e => !e.IsDeleted);
                modelBuilder.Entity<Products>().HasQueryFilter(e => !e.IsDeleted);
                modelBuilder.Entity<ProductCategories>().HasQueryFilter(e => !e.IsDeleted);
                modelBuilder.Entity<Orders>().HasQueryFilter(e => !e.IsDeleted);
                modelBuilder.Entity<Suppliers>().HasQueryFilter(e => !e.IsDeleted);
                modelBuilder.Entity<Blog>().HasQueryFilter(e => !e.IsDeleted);
                modelBuilder.Entity<Comment>().HasQueryFilter(e => !e.IsDeleted);

                modelBuilder.Entity<CartItem>()
                    .Property(c => c.LineTotal)
                    .HasComputedColumnSql("[Quantity] * [UnitPrice]", stored: true);

                modelBuilder.Entity<Comment>()
      .HasOne(c => c.Parent)
      .WithMany(c => c.Children)
      .HasForeignKey(c => c.ParentId)
      .OnDelete(DeleteBehavior.Restrict); // hoặc .NoAction()
            }

            private void ConfigureBaseEntityProperties<T>(ModelBuilder modelBuilder) where T : BaseEntity
            {
                modelBuilder.Entity<T>(entity =>
                {
                    entity.HasKey(e => e.Id);
                    entity.Property(e => e.Id).ValueGeneratedOnAdd();
                    entity.Property(e => e.PublicId).IsRequired();
                    entity.Property(e => e.CreatedAt).IsRequired();
                    entity.Property(e => e.UpdatedAt);
                    entity.Property(e => e.DeletedAt);
                    entity.Property(e => e.IsDeleted).IsRequired().HasDefaultValue(false);

                    entity.HasIndex(e => e.PublicId).IsUnique();
                });
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
                var entries = ChangeTracker.Entries().Where(e => e.Entity is BaseEntity && (e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted));
                foreach (var entry in entries)
                {
                    var baseEntity = (BaseEntity)entry.Entity;
                    switch (entry.State)
                    {
                        case EntityState.Added:
                            baseEntity.CreatedAt = DateTime.UtcNow;
                            baseEntity.PublicId = Guid.NewGuid();
                            break;
                        case EntityState.Modified:
                            baseEntity.UpdatedAt = DateTime.UtcNow;
                            break;
                        case EntityState.Deleted:
                            entry.State = EntityState.Modified;
                            baseEntity.IsDeleted = true;
                            baseEntity.DeletedAt = DateTime.UtcNow;
                            baseEntity.UpdatedAt = DateTime.UtcNow;
                            break;
                    }
                }
            }
        }
    }
}