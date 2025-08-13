using FlowerSellingWebsite.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace FlowerSelling.Data
{
    public class FlowerSellingDbContext : DbContext
    {
        public FlowerSellingDbContext(DbContextOptions<FlowerSellingDbContext> options) : base(options)
        {
        }

        // ========== DbSets ==========
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<FlowerCategory> FlowerCategories { get; set; }
        public DbSet<Flower> Flowers { get; set; }
        public DbSet<FlowerBatch> FlowerBatches { get; set; }
        public DbSet<FlowerDamageLog> FlowerDamageLogs { get; set; }
        public DbSet<FlowerPhoto> FlowerPhotos { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductFlower> ProductFlowers { get; set; }
        public DbSet<ProductPhoto> ProductPhotos { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<SupplierListing> SupplierListings { get; set; }
        public DbSet<SupplierListingPhoto> SupplierListingPhotos { get; set; }
        public DbSet<PaymentMethod> PaymentMethods { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Delivery> Deliveries { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<SystemLog> SystemLogs { get; set; }
        public DbSet<SystemNotification> SystemNotifications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure BaseEntity for all entities
            ConfigureBaseEntity(modelBuilder);

            // Configure relationships
            ConfigureSupplierRelationships(modelBuilder);
            ConfigureFlowerRelationships(modelBuilder);
            ConfigureProductRelationships(modelBuilder);
            ConfigureOrderRelationships(modelBuilder);
            ConfigureUserRelationships(modelBuilder);
            ConfigureSystemRelationships(modelBuilder);

            // Configure indexes for performance
            ConfigureIndexes(modelBuilder);

            // Configure constraints and validations
            ConfigureConstraints(modelBuilder);

            // Seed initial data
            SeedData(modelBuilder);
        }

        private void ConfigureBaseEntity(ModelBuilder modelBuilder)
        {
            // Configure all entities that inherit from BaseEntity
            foreach (var entityType in modelBuilder.Model.GetEntityTypes()
                .Where(e => typeof(BaseEntity).IsAssignableFrom(e.ClrType)))
            {
                // Primary Key
                modelBuilder.Entity(entityType.ClrType).HasKey("Id");

                // PublicId Index
                modelBuilder.Entity(entityType.ClrType)
                    .HasIndex("PublicId")
                    .IsUnique()
                    .HasDatabaseName($"IX_{entityType.GetTableName()}_PublicId");

                // Soft Delete Query Filter
                modelBuilder.Entity(entityType.ClrType)
                    .HasQueryFilter(GetSoftDeleteFilter(entityType.ClrType));
            }
        }

        private static LambdaExpression GetSoftDeleteFilter(Type entityType)
        {
            var parameter = Expression.Parameter(entityType, "e");
            var property = Expression.Property(parameter, "IsDeleted");
            var condition = Expression.Equal(property, Expression.Constant(false));
            return Expression.Lambda(condition, parameter);
        }

        private static void ConfigureSupplierRelationships(ModelBuilder modelBuilder)
        {
            // Supplier -> FlowerBatch (One-to-Many)
            modelBuilder.Entity<FlowerBatch>()
                .HasOne(fb => fb.Supplier)
                .WithMany(s => s.FlowerBatches)
                .HasForeignKey(fb => fb.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);

            // Supplier -> SupplierListing (One-to-Many)
            modelBuilder.Entity<SupplierListing>()
                .HasOne(sl => sl.Supplier)
                .WithMany(s => s.SupplierListings)
                .HasForeignKey(sl => sl.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);

            // Supplier -> Order (One-to-Many)
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Supplier)
                .WithMany(s => s.Orders)
                .HasForeignKey(o => o.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);
        }

        private static void ConfigureFlowerRelationships(ModelBuilder modelBuilder)
        {
            // FlowerCategory -> Flower (One-to-Many)
            modelBuilder.Entity<Flower>()
                .HasOne(f => f.FlowerCategory)
                .WithMany(fc => fc.Flowers)
                .HasForeignKey(f => f.FlowerCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Flower -> FlowerBatch (One-to-Many)
            modelBuilder.Entity<FlowerBatch>()
                .HasOne(fb => fb.Flower)
                .WithMany(f => f.FlowerBatches)
                .HasForeignKey(fb => fb.FlowerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Flower -> FlowerPhoto (One-to-Many)
            modelBuilder.Entity<FlowerPhoto>()
                .HasOne(fp => fp.Flower)
                .WithMany(f => f.FlowerPhotos)
                .HasForeignKey(fp => fp.FlowerId)
                .OnDelete(DeleteBehavior.Cascade);

            // Flower -> ProductFlower (One-to-Many)
            modelBuilder.Entity<ProductFlower>()
                .HasOne(pf => pf.Flower)
                .WithMany(f => f.ProductFlowers)
                .HasForeignKey(pf => pf.FlowerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Flower -> SupplierListing (One-to-Many)
            modelBuilder.Entity<SupplierListing>()
                .HasOne(sl => sl.Flower)
                .WithMany(f => f.SupplierListings)
                .HasForeignKey(sl => sl.FlowerId)
                .OnDelete(DeleteBehavior.Restrict);

            // FlowerBatch -> FlowerDamageLog (One-to-Many)
            modelBuilder.Entity<FlowerDamageLog>()
                .HasOne(fdl => fdl.FlowerBatch)
                .WithMany(fb => fb.FlowerDamageLogs)
                .HasForeignKey(fdl => fdl.FlowerBatchId)
                .OnDelete(DeleteBehavior.Cascade);

            // FlowerBatch -> OrderDetail (One-to-Many)
            modelBuilder.Entity<OrderDetail>()
                .HasOne(od => od.FlowerBatch)
                .WithMany(fb => fb.OrderDetails)
                .HasForeignKey(od => od.FlowerBatchId)
                .OnDelete(DeleteBehavior.Restrict);
        }

        private static void ConfigureProductRelationships(ModelBuilder modelBuilder)
        {
            // Product -> ProductFlower (One-to-Many)
            modelBuilder.Entity<ProductFlower>()
                .HasOne(pf => pf.Product)
                .WithMany(p => p.ProductFlowers)
                .HasForeignKey(pf => pf.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // Product -> ProductPhoto (One-to-Many)
            modelBuilder.Entity<ProductPhoto>()
                .HasOne(pp => pp.Product)
                .WithMany(p => p.ProductPhotos)
                .HasForeignKey(pp => pp.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // Product -> OrderDetail (One-to-Many)
            modelBuilder.Entity<OrderDetail>()
                .HasOne(od => od.Product)
                .WithMany(p => p.OrderDetails)
                .HasForeignKey(od => od.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            // SupplierListing -> SupplierListingPhoto (One-to-Many)
            modelBuilder.Entity<SupplierListingPhoto>()
                .HasOne(slp => slp.SupplierListing)
                .WithMany(sl => sl.SupplierListingPhotos)
                .HasForeignKey(slp => slp.SupplierListingId)
                .OnDelete(DeleteBehavior.Cascade);

            // SupplierListing -> OrderDetail (One-to-Many)
            modelBuilder.Entity<OrderDetail>()
                .HasOne(od => od.SupplierListing)
                .WithMany(sl => sl.OrderDetails)
                .HasForeignKey(od => od.SupplierListingId)
                .OnDelete(DeleteBehavior.Restrict);
        }

        private static void ConfigureOrderRelationships(ModelBuilder modelBuilder)
        {
            // Order -> OrderDetail (One-to-Many)
            modelBuilder.Entity<OrderDetail>()
                .HasOne(od => od.Order)
                .WithMany(o => o.OrderDetails)
                .HasForeignKey(od => od.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // Order -> Payment (One-to-Many)
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Order)
                .WithMany(o => o.Payments)
                .HasForeignKey(p => p.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // Order -> Delivery (One-to-Many)
            modelBuilder.Entity<Delivery>()
                .HasOne(d => d.Order)
                .WithMany(o => o.Deliveries)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // PaymentMethod -> Payment (One-to-Many)
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.PaymentMethod)
                .WithMany(pm => pm.Payments)
                .HasForeignKey(p => p.PaymentMethodId)
                .OnDelete(DeleteBehavior.Restrict);
        }

        private static void ConfigureUserRelationships(ModelBuilder modelBuilder)
        {
            // Role -> User (One-to-Many)
            modelBuilder.Entity<User>()
                .HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            // Role -> RolePermission (One-to-Many)
            modelBuilder.Entity<RolePermission>()
                .HasOne(rp => rp.Role)
                .WithMany(r => r.RolePermissions)
                .HasForeignKey(rp => rp.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            // Permission -> RolePermission (One-to-Many)
            modelBuilder.Entity<RolePermission>()
                .HasOne(rp => rp.Permission)
                .WithMany(p => p.RolePermissions)
                .HasForeignKey(rp => rp.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);

            // User -> Order relationships
            modelBuilder.Entity<Order>()
                .HasOne(o => o.CreatedByUser)
                .WithMany(u => u.CreatedOrders)
                .HasForeignKey(o => o.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.Customer)
                .WithMany(u => u.CustomerOrders)
                .HasForeignKey(o => o.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            // User -> FlowerDamageLog
            modelBuilder.Entity<FlowerDamageLog>()
                .HasOne(fdl => fdl.ReportedByUser)
                .WithMany(u => u.DamageReports)
                .HasForeignKey(fdl => fdl.ReportedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        }

        private static void ConfigureSystemRelationships(ModelBuilder modelBuilder)
        {
            // User -> SystemLog
            modelBuilder.Entity<SystemLog>()
                .HasOne(sl => sl.User)
                .WithMany(u => u.SystemLogs)
                .HasForeignKey(sl => sl.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Order -> SystemLog
            modelBuilder.Entity<SystemLog>()
                .HasOne(sl => sl.Order)
                .WithMany(o => o.SystemLogs)
                .HasForeignKey(sl => sl.OrderId)
                .OnDelete(DeleteBehavior.SetNull);

            // User -> SystemNotification
            modelBuilder.Entity<SystemNotification>()
                .HasOne(sn => sn.RecipientUser)
                .WithMany(u => u.Notifications)
                .HasForeignKey(sn => sn.RecipientUserId)
                .OnDelete(DeleteBehavior.Cascade);
        }

        private static void ConfigureIndexes(ModelBuilder modelBuilder)
        {
            // Supplier indexes
            modelBuilder.Entity<Supplier>()
                .HasIndex(s => s.Email)
                .HasDatabaseName("IX_Suppliers_Email");

            // Flower indexes
            modelBuilder.Entity<Flower>()
                .HasIndex(f => f.Name)
                .HasDatabaseName("IX_Flowers_Name");

            modelBuilder.Entity<Flower>()
                .HasIndex(f => f.FlowerCategoryId)
                .HasDatabaseName("IX_Flowers_FlowerCategoryId");

            // FlowerBatch indexes
            modelBuilder.Entity<FlowerBatch>()
                .HasIndex(fb => fb.BatchCode)
                .HasDatabaseName("IX_FlowerBatches_BatchCode");

            modelBuilder.Entity<FlowerBatch>()
                .HasIndex(fb => fb.ExpiryDate)
                .HasDatabaseName("IX_FlowerBatches_ExpiryDate");

            // Order indexes
            modelBuilder.Entity<Order>()
                .HasIndex(o => o.OrderNumber)
                .IsUnique()
                .HasDatabaseName("IX_Orders_OrderNumber");

            modelBuilder.Entity<Order>()
                .HasIndex(o => o.OrderDate)
                .HasDatabaseName("IX_Orders_OrderDate");

            modelBuilder.Entity<Order>()
                .HasIndex(o => o.Status)
                .HasDatabaseName("IX_Orders_Status");

            // User indexes
            modelBuilder.Entity<User>()
                .HasIndex(u => u.UserName)
                .IsUnique()
                .HasDatabaseName("IX_Users_UserName");

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .HasDatabaseName("IX_Users_Email");

            // SystemNotification indexes
            modelBuilder.Entity<SystemNotification>()
                .HasIndex(sn => new { sn.RecipientUserId, sn.IsRead })
                .HasDatabaseName("IX_SystemNotifications_RecipientUserId_IsRead");

            // SystemLog indexes
            modelBuilder.Entity<SystemLog>()
                .HasIndex(sl => sl.CreatedAt)
                .HasDatabaseName("IX_SystemLogs_CreatedAt");
        }

        private static void ConfigureConstraints(ModelBuilder modelBuilder)
        {
            // FlowerBatch constraints
            modelBuilder.Entity<FlowerBatch>()
                .ToTable(t => t.HasCheckConstraint("CK_FlowerBatch_QuantityAvailable_LessOrEqual_Quantity",
                    "[QuantityAvailable] <= [Quantity]"));

            modelBuilder.Entity<FlowerBatch>()
                .ToTable(t => t.HasCheckConstraint("CK_FlowerBatch_ExpiryDate_Greater_ImportDate",
                    "[ExpiryDate] > [ImportDate]"));

            // OrderDetail constraints
            modelBuilder.Entity<OrderDetail>()
                .ToTable(t => t.HasCheckConstraint("CK_OrderDetail_ApprovedQuantity_LessOrEqual_RequestedQuantity",
                    "[ApprovedQuantity] IS NULL OR [ApprovedQuantity] <= [RequestedQuantity]"));

            // FlowerPhoto primary photo constraint
            modelBuilder.Entity<FlowerPhoto>()
                .HasIndex(fp => new { fp.FlowerId, fp.IsPrimary })
                .HasFilter("[IsPrimary] = 1")
                .IsUnique()
                .HasDatabaseName("IX_FlowerPhotos_FlowerId_IsPrimary_Unique");

            // ProductPhoto primary photo constraint
            modelBuilder.Entity<ProductPhoto>()
                .HasIndex(pp => new { pp.ProductId, pp.IsPrimary })
                .HasFilter("[IsPrimary] = 1")
                .IsUnique()
                .HasDatabaseName("IX_ProductPhotos_ProductId_IsPrimary_Unique");

            // SupplierListingPhoto primary photo constraint
            modelBuilder.Entity<SupplierListingPhoto>()
                .HasIndex(slp => new { slp.SupplierListingId, slp.IsPrimary })
                .HasFilter("[IsPrimary] = 1")
                .IsUnique()
                .HasDatabaseName("IX_SupplierListingPhotos_SupplierListingId_IsPrimary_Unique");

            // RolePermission composite unique constraint
            modelBuilder.Entity<RolePermission>()
                .HasIndex(rp => new { rp.RoleId, rp.PermissionId })
                .IsUnique()
                .HasDatabaseName("IX_RolePermissions_RoleId_PermissionId_Unique");

            // ProductFlower composite unique constraint
            modelBuilder.Entity<ProductFlower>()
                .HasIndex(pf => new { pf.ProductId, pf.FlowerId })
                .IsUnique()
                .HasDatabaseName("IX_ProductFlowers_ProductId_FlowerId_Unique");
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Seed Roles
            modelBuilder.Entity<Role>().HasData(
                new Role { Id = 1, RoleName = "Admin", Description = "System Administrator", PublicId = Guid.NewGuid(), CreatedAt = DateTime.UtcNow },
                new Role { Id = 2, RoleName = "Manager", Description = "Store Manager", PublicId = Guid.NewGuid(), CreatedAt = DateTime.UtcNow },
                new Role { Id = 3, RoleName = "Staff", Description = "Store Staff", PublicId = Guid.NewGuid(), CreatedAt = DateTime.UtcNow },
                new Role { Id = 4, RoleName = "Customer", Description = "Customer", PublicId = Guid.NewGuid(), CreatedAt = DateTime.UtcNow },
                new Role { Id = 5, RoleName = "Supplier", Description = "Supplier", PublicId = Guid.NewGuid(), CreatedAt = DateTime.UtcNow }
            );

            // Seed Permissions
            modelBuilder.Entity<Permission>().HasData(
                new Permission { Id = 1, PermissionName = "ManageUsers", Description = "Create, update, delete users", PublicId = Guid.NewGuid(), CreatedAt = DateTime.UtcNow },
                new Permission { Id = 2, PermissionName = "ManageOrders", Description = "Create, update, delete orders", PublicId = Guid.NewGuid(), CreatedAt = DateTime.UtcNow },
                new Permission { Id = 3, PermissionName = "ManageFlowers", Description = "Create, update, delete flowers", PublicId = Guid.NewGuid(), CreatedAt = DateTime.UtcNow },
                new Permission { Id = 4, PermissionName = "ManageSuppliers", Description = "Create, update, delete suppliers", PublicId = Guid.NewGuid(), CreatedAt = DateTime.UtcNow },
                new Permission { Id = 5, PermissionName = "ViewReports", Description = "View system reports", PublicId = Guid.NewGuid(), CreatedAt = DateTime.UtcNow },
                new Permission { Id = 6, PermissionName = "PlaceOrders", Description = "Place orders", PublicId = Guid.NewGuid(), CreatedAt = DateTime.UtcNow },
                new Permission { Id = 7, PermissionName = "ManageListings", Description = "Manage supplier listings", PublicId = Guid.NewGuid(), CreatedAt = DateTime.UtcNow }
            );

            // Seed Payment Methods
            modelBuilder.Entity<PaymentMethod>().HasData(
                new PaymentMethod { Id = 1, MethodName = "Cash", Description = "Cash Payment", PublicId = Guid.NewGuid(), CreatedAt = DateTime.UtcNow },
                new PaymentMethod { Id = 2, MethodName = "Credit Card", Description = "Credit Card Payment", PublicId = Guid.NewGuid(), CreatedAt = DateTime.UtcNow },
                new PaymentMethod { Id = 3, MethodName = "Bank Transfer", Description = "Bank Transfer Payment", PublicId = Guid.NewGuid(), CreatedAt = DateTime.UtcNow },
                new PaymentMethod { Id = 4, MethodName = "Digital Wallet", Description = "Digital Wallet Payment", PublicId = Guid.NewGuid(), CreatedAt = DateTime.UtcNow }
            );

            // Seed Flower Categories
            modelBuilder.Entity<FlowerCategory>().HasData(
                new FlowerCategory { Id = 1, CategoryName = "Roses", Description = "Beautiful roses for all occasions", Color = "Red", PublicId = Guid.NewGuid(), CreatedAt = DateTime.UtcNow },
                new FlowerCategory { Id = 2, CategoryName = "Tulips", Description = "Elegant tulips", Color = "Yellow", PublicId = Guid.NewGuid(), CreatedAt = DateTime.UtcNow },
                new FlowerCategory { Id = 3, CategoryName = "Orchids", Description = "Exotic orchids", Color = "Purple", PublicId = Guid.NewGuid(), CreatedAt = DateTime.UtcNow },
                new FlowerCategory { Id = 4, CategoryName = "Lilies", Description = "Graceful lilies", Color = "White", PublicId = Guid.NewGuid(), CreatedAt = DateTime.UtcNow },
                new FlowerCategory { Id = 5, CategoryName = "Carnations", Description = "Colorful carnations", Color = "Pink", PublicId = Guid.NewGuid(), CreatedAt = DateTime.UtcNow }
            );
        }

        public override int SaveChanges()
        {
            UpdateTimestamps();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateTimestamps();
            return base.SaveChangesAsync(cancellationToken);
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
                        if (entry.Entity.PublicId == Guid.Empty)
                        {
                            entry.Entity.PublicId = Guid.NewGuid();
                        }
                        break;

                    case EntityState.Modified:
                        entry.Entity.UpdatedAt = DateTime.UtcNow;
                        break;

                    case EntityState.Deleted:
                        entry.State = EntityState.Modified;
                        entry.Entity.IsDeleted = true;
                        entry.Entity.DeletedAt = DateTime.UtcNow;
                        break;
                }
            }
        }
    }
}