using FlowerSellingWebsite.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace FlowerSelling.Data
{
    public class FlowerSellingDbContext : DbContext
    {
        public FlowerSellingDbContext(DbContextOptions<FlowerSellingDbContext> options) : base(options)
        {
        }

        // Parameterless constructor for design time
        public FlowerSellingDbContext() : base()
        {
        }

        // DbSets
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }
        public DbSet<Flower> Flowers { get; set; }
        public DbSet<FlowerBatch> FlowerBatches { get; set; }
        public DbSet<FlowerBatchDetail> FlowerBatchDetails { get; set; }
        public DbSet<FlowerDamageLog> FlowerDamageLogs { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Delivery> Deliveries { get; set; }
        public DbSet<SystemLog> SystemLogs { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductFlower> ProductFlowers { get; set; }
        public DbSet<ProductFlowerBatchUsage> ProductFlowerBatchUsages { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                // Try to read from configuration
                try
                {
                    var configuration = new ConfigurationBuilder()
                        .SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile("appsettings.json", optional: true)
                        .AddJsonFile("appsettings.Development.json", optional: true)
                        .AddEnvironmentVariables()
                        .Build();

                    var connectionString = configuration.GetConnectionString("DefaultConnection");

                    if (!string.IsNullOrEmpty(connectionString))
                    {
                        optionsBuilder.UseSqlServer(connectionString);
                    }
                    else
                    {
                        // Fallback connection string
                        optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=FlowerSellingDB;Trusted_Connection=true;MultipleActiveResultSets=true;TrustServerCertificate=true");
                    }
                }
                catch
                {
                    // If configuration fails, use default connection string
                    optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=FlowerSellingDB;Trusted_Connection=true;MultipleActiveResultSets=true;TrustServerCertificate=true");
                }
            }

            optionsBuilder.ConfigureWarnings(warnings =>
        warnings.Ignore(RelationalEventId.PendingModelChangesWarning));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Primary Keys
            ConfigurePrimaryKeys(modelBuilder);

            // Configure Foreign Keys and Relationships
            ConfigureRelationships(modelBuilder);

            // Configure Indexes
            ConfigureIndexes(modelBuilder);

            // Configure Column Types and Constraints
            ConfigureColumnTypes(modelBuilder);

            // Configure Decimal Precision
            ConfigureDecimalPrecision(modelBuilder);

            // Configure Table Constraints (NET 8.0 compatible)
            ConfigureTableConstraints(modelBuilder);

            // Configure Soft Delete Global Query Filter
            ConfigureSoftDelete(modelBuilder);

            // Seed Default Data
            SeedDefaultData(modelBuilder);
        }

        private void ConfigurePrimaryKeys(ModelBuilder modelBuilder)
        {
            // All entities inherit from BaseEntity with Guid Id as primary key
            // EF Core will automatically configure this, but we can be explicit
            modelBuilder.Entity<Supplier>().HasKey(s => s.Id);
            modelBuilder.Entity<User>().HasKey(u => u.Id);
            modelBuilder.Entity<Role>().HasKey(r => r.Id);
            modelBuilder.Entity<Permission>().HasKey(p => p.Id);
            modelBuilder.Entity<RolePermission>().HasKey(rp => rp.Id);
            modelBuilder.Entity<Flower>().HasKey(f => f.Id);
            modelBuilder.Entity<FlowerBatch>().HasKey(fb => fb.Id);
            modelBuilder.Entity<FlowerBatchDetail>().HasKey(fbd => fbd.Id);
            modelBuilder.Entity<FlowerDamageLog>().HasKey(fdl => fdl.Id);
            modelBuilder.Entity<Order>().HasKey(o => o.Id);
            modelBuilder.Entity<OrderDetail>().HasKey(od => od.Id);
            modelBuilder.Entity<Payment>().HasKey(p => p.Id);
            modelBuilder.Entity<Delivery>().HasKey(d => d.Id);
            modelBuilder.Entity<SystemLog>().HasKey(sl => sl.Id);
            modelBuilder.Entity<Product>().HasKey(p => p.Id);
            modelBuilder.Entity<ProductFlower>().HasKey(pf => pf.Id);
            modelBuilder.Entity<ProductFlowerBatchUsage>().HasKey(pfbu => pfbu.Id);
        }

        private void ConfigureRelationships(ModelBuilder modelBuilder)
        {
            // User - Role relationship
            modelBuilder.Entity<User>()
                .HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            // RolePermission relationships
            modelBuilder.Entity<RolePermission>()
                .HasOne(rp => rp.Role)
                .WithMany(r => r.RolePermissions)
                .HasForeignKey(rp => rp.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RolePermission>()
                .HasOne(rp => rp.Permission)
                .WithMany(p => p.RolePermissions)
                .HasForeignKey(rp => rp.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);

            // Flower - Supplier relationship
            modelBuilder.Entity<Flower>()
                .HasOne(f => f.Supplier)
                .WithMany(s => s.Flowers)
                .HasForeignKey(f => f.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);

            // FlowerBatch relationships
            modelBuilder.Entity<FlowerBatch>()
                .HasOne(fb => fb.Supplier)
                .WithMany()
                .HasForeignKey(fb => fb.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<FlowerBatch>()
                .HasOne(fb => fb.Flower)
                .WithMany(f => f.FlowerBatches)
                .HasForeignKey(fb => fb.FlowerId)
                .OnDelete(DeleteBehavior.Restrict);

            // FlowerBatchDetail relationships
            modelBuilder.Entity<FlowerBatchDetail>()
                .HasOne(fbd => fbd.FlowerBatch)
                .WithMany(fb => fb.FlowerBatchDetails)
                .HasForeignKey(fbd => fbd.FlowerBatchId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<FlowerBatchDetail>()
                .HasOne(fbd => fbd.Flower)
                .WithMany()
                .HasForeignKey(fbd => fbd.FlowerId)
                .OnDelete(DeleteBehavior.Restrict);

            // FlowerDamageLog relationship
            modelBuilder.Entity<FlowerDamageLog>()
                .HasOne(fdl => fdl.FlowerBatchDetail)
                .WithMany(fbd => fbd.FlowerDamageLogs)
                .HasForeignKey(fdl => fdl.FlowerBatchDetailId)
                .OnDelete(DeleteBehavior.Cascade);

            // Order - User relationship
            modelBuilder.Entity<Order>()
                .HasOne(o => o.User)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // OrderDetail relationships
            modelBuilder.Entity<OrderDetail>()
                .HasOne(od => od.Order)
                .WithMany(o => o.OrderDetails)
                .HasForeignKey(od => od.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OrderDetail>()
                .HasOne(od => od.Product)
                .WithMany(p => p.OrderDetails)
                .HasForeignKey(od => od.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            // Payment - Order relationship
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Order)
                .WithMany(o => o.Payments)
                .HasForeignKey(p => p.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // Delivery - Order relationship
            modelBuilder.Entity<Delivery>()
                .HasOne(d => d.Order)
                .WithMany(o => o.Deliveries)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // SystemLog relationships
            modelBuilder.Entity<SystemLog>()
                .HasOne(sl => sl.User)
                .WithMany()
                .HasForeignKey(sl => sl.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SystemLog>()
                .HasOne(sl => sl.Order)
                .WithMany(o => o.SystemLogs)
                .HasForeignKey(sl => sl.RecordId)
                .OnDelete(DeleteBehavior.Restrict);

            // ProductFlower relationships
            modelBuilder.Entity<ProductFlower>()
                .HasOne(pf => pf.Product)
                .WithMany(p => p.ProductFlowers)
                .HasForeignKey(pf => pf.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ProductFlower>()
                .HasOne(pf => pf.Flower)
                .WithMany(f => f.ProductFlowers)
                .HasForeignKey(pf => pf.FlowerId)
                .OnDelete(DeleteBehavior.Restrict);

            // ProductFlowerBatchUsage relationships
            modelBuilder.Entity<ProductFlowerBatchUsage>()
                .HasOne(pfbu => pfbu.Product)
                .WithMany(p => p.ProductFlowerBatchUsages)
                .HasForeignKey(pfbu => pfbu.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ProductFlowerBatchUsage>()
                .HasOne(pfbu => pfbu.FlowerBatchDetail)
                .WithMany()
                .HasForeignKey(pfbu => pfbu.FlowerBatchDetailId)
                .OnDelete(DeleteBehavior.Restrict);
        }

        private void ConfigureIndexes(ModelBuilder modelBuilder)
        {
            // User indexes
            modelBuilder.Entity<User>()
                .HasIndex(u => u.UserName)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique()
                .HasFilter("[Email] IS NOT NULL");

            // Role index
            modelBuilder.Entity<Role>()
                .HasIndex(r => r.RoleName)
                .IsUnique();

            // Permission index
            modelBuilder.Entity<Permission>()
                .HasIndex(p => p.PermissionName)
                .IsUnique();

            // RolePermission composite index
            modelBuilder.Entity<RolePermission>()
                .HasIndex(rp => new { rp.RoleId, rp.PermissionId })
                .IsUnique();

            // Flower indexes
            modelBuilder.Entity<Flower>()
                .HasIndex(f => f.Name);

            modelBuilder.Entity<Flower>()
                .HasIndex(f => f.SupplierId);

            // Order indexes
            modelBuilder.Entity<Order>()
                .HasIndex(o => o.UserId);

            modelBuilder.Entity<Order>()
                .HasIndex(o => o.CreatedAt);

            // SystemLog indexes
            modelBuilder.Entity<SystemLog>()
                .HasIndex(sl => sl.UserId);

            modelBuilder.Entity<SystemLog>()
                .HasIndex(sl => sl.CreatedAt);

            // ProductFlower composite index
            modelBuilder.Entity<ProductFlower>()
                .HasIndex(pf => new { pf.ProductId, pf.FlowerId })
                .IsUnique();
        }

        private void ConfigureColumnTypes(ModelBuilder modelBuilder)
        {
            // Configure string column types and lengths

            // Supplier
            modelBuilder.Entity<Supplier>(entity =>
            {
                entity.Property(e => e.SupplierName)
                    .HasColumnType("nvarchar(150)")
                    .IsRequired();
                entity.Property(e => e.ContactPerson)
                    .HasColumnType("nvarchar(100)");
                entity.Property(e => e.Phone)
                    .HasColumnType("nvarchar(20)");
                entity.Property(e => e.Email)
                    .HasColumnType("nvarchar(100)");
                entity.Property(e => e.Address)
                    .HasColumnType("nvarchar(300)");
            });

            // User
            modelBuilder.Entity<User>(entity =>
            {
                entity.Property(e => e.UserName)
                    .HasColumnType("nvarchar(100)")
                    .IsRequired();
                entity.Property(e => e.PasswordHash)
                    .HasColumnType("nvarchar(256)");
                entity.Property(e => e.FullName)
                    .HasColumnType("nvarchar(150)");
                entity.Property(e => e.Email)
                    .HasColumnType("nvarchar(50)");
                entity.Property(e => e.Phone)
                    .HasColumnType("nvarchar(20)");
                entity.Property(e => e.Address)
                    .HasColumnType("nvarchar(300)");
            });

            // Role
            modelBuilder.Entity<Role>(entity =>
            {
                entity.Property(e => e.RoleName)
                    .HasColumnType("nvarchar(50)")
                    .IsRequired();
                entity.Property(e => e.Description)
                    .HasColumnType("nvarchar(200)");
            });

            // Permission
            modelBuilder.Entity<Permission>(entity =>
            {
                entity.Property(e => e.PermissionName)
                    .HasColumnType("nvarchar(100)")
                    .IsRequired();
                entity.Property(e => e.Description)
                    .HasColumnType("nvarchar(200)");
            });

            // Flower
            modelBuilder.Entity<Flower>(entity =>
            {
                entity.Property(e => e.Name)
                    .HasColumnType("nvarchar(150)")
                    .IsRequired();
                entity.Property(e => e.Description)
                    .HasColumnType("nvarchar(300)");
                entity.Property(e => e.Price)
                    .HasColumnType("decimal(18,2)");
            });

            // FlowerBatch
            modelBuilder.Entity<FlowerBatch>(entity =>
            {
                entity.Property(e => e.Notes)
                    .HasColumnType("nvarchar(300)");
                entity.Property(e => e.TotalAmount)
                    .HasColumnType("decimal(18,2)");
            });

            // FlowerBatchDetail
            modelBuilder.Entity<FlowerBatchDetail>(entity =>
            {
                entity.Property(e => e.UnitPrice)
                    .HasColumnType("decimal(18,2)");
            });

            // FlowerDamageLog
            modelBuilder.Entity<FlowerDamageLog>(entity =>
            {
                entity.Property(e => e.DamageReason)
                    .HasColumnType("nvarchar(300)");
            });

            // Order
            modelBuilder.Entity<Order>(entity =>
            {
                entity.Property(e => e.TotalAmount)
                    .HasColumnType("decimal(18,2)");
            });

            // OrderDetail
            modelBuilder.Entity<OrderDetail>(entity =>
            {
                entity.Property(e => e.UnitPrice)
                    .HasColumnType("decimal(18,2)");
            });

            // Payment
            modelBuilder.Entity<Payment>(entity =>
            {
                entity.Property(e => e.PaymentStatus)
                    .HasColumnType("nvarchar(50)");
                entity.Property(e => e.Amount)
                    .HasColumnType("decimal(18,2)");
            });

            // Delivery
            modelBuilder.Entity<Delivery>(entity =>
            {
                entity.Property(e => e.DeliveryStatus)
                    .HasColumnType("nvarchar(50)");
                entity.Property(e => e.DeliveryAddress)
                    .HasColumnType("nvarchar(300)");
            });

            // SystemLog
            modelBuilder.Entity<SystemLog>(entity =>
            {
                entity.Property(e => e.Action)
                    .HasColumnType("nvarchar(100)");
                entity.Property(e => e.TableName)
                    .HasColumnType("nvarchar(100)");
                entity.Property(e => e.IPAddress)
                    .HasColumnType("nvarchar(45)");
            });

            // Product
            modelBuilder.Entity<Product>(entity =>
            {
                entity.Property(e => e.Name)
                    .HasColumnType("nvarchar(150)");
                entity.Property(e => e.Description)
                    .HasColumnType("nvarchar(300)");
                entity.Property(e => e.Price)
                    .HasColumnType("decimal(18,2)");
            });
            // Configure decimal precision for financial fields
            modelBuilder.Entity<Flower>()
                .Property(f => f.Price)
                .HasPrecision(18, 2);

            modelBuilder.Entity<FlowerBatch>()
                    .Property(fb => fb.TotalAmount)
                    .HasPrecision(18, 2);

            modelBuilder.Entity<FlowerBatchDetail>()
                    .Property(fbd => fbd.UnitPrice)
                    .HasPrecision(18, 2);

            modelBuilder.Entity<Order>()
                    .Property(o => o.TotalAmount)
                    .HasPrecision(18, 2);

            modelBuilder.Entity<OrderDetail>()
                    .Property(od => od.UnitPrice)
                    .HasPrecision(18, 2);

            modelBuilder.Entity<Payment>()
                    .Property(p => p.Amount)
                    .HasPrecision(18, 2);

            modelBuilder.Entity<Product>()
                    .Property(p => p.Price)
                    .HasPrecision(18, 2);
        }




        private void ConfigureDecimalPrecision(ModelBuilder modelBuilder)
        {
            // Configure decimal precision for financial fields (backup configuration)
            modelBuilder.Entity<Flower>()
                .Property(f => f.Price)
                .HasPrecision(18, 2);

            modelBuilder.Entity<FlowerBatch>()
                .Property(fb => fb.TotalAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<FlowerBatchDetail>()
                .Property(fbd => fbd.UnitPrice)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Order>()
                .Property(o => o.TotalAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<OrderDetail>()
                .Property(od => od.UnitPrice)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Payment>()
                .Property(p => p.Amount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Product>()
                .Property(p => p.Price)
                .HasPrecision(18, 2);
        }

        private void ConfigureTableConstraints(ModelBuilder modelBuilder)
        {
            // NET 8.0 compatible table constraints using ToTable() method

            // Supplier constraints
            modelBuilder.Entity<Supplier>().ToTable(tb =>
            {
                tb.HasCheckConstraint("CK_Supplier_SupplierName_NotEmpty", "[SupplierName] != ''");
            });

            // User constraints
            modelBuilder.Entity<User>().ToTable(tb =>
            {
                tb.HasCheckConstraint("CK_User_UserName_NotEmpty", "[UserName] != ''");
                tb.HasCheckConstraint("CK_User_Email_Format", "[Email] IS NULL OR [Email] LIKE '%@%.%'");
                tb.HasCheckConstraint("CK_User_Phone_Format", "[Phone] IS NULL OR LEN([Phone]) >= 10");
            });

            // Role constraints
            modelBuilder.Entity<Role>().ToTable(tb =>
            {
                tb.HasCheckConstraint("CK_Role_RoleName_NotEmpty", "[RoleName] != ''");
            });

            // Permission constraints
            modelBuilder.Entity<Permission>().ToTable(tb =>
            {
                tb.HasCheckConstraint("CK_Permission_PermissionName_NotEmpty", "[PermissionName] != ''");
            });

            // Flower constraints
            modelBuilder.Entity<Flower>().ToTable(tb =>
            {
                tb.HasCheckConstraint("CK_Flower_Name_NotEmpty", "[Name] != ''");
                tb.HasCheckConstraint("CK_Flower_Price_NonNegative", "[Price] >= 0");
                tb.HasCheckConstraint("CK_Flower_StockQuantity_NonNegative", "[StockQuantity] >= 0");
            });

            // FlowerBatch constraints
            modelBuilder.Entity<FlowerBatch>().ToTable(tb =>
            {
                tb.HasCheckConstraint("CK_FlowerBatch_TotalAmount_NonNegative", "[TotalAmount] >= 0");
            });

            // FlowerBatchDetail constraints
            modelBuilder.Entity<FlowerBatchDetail>().ToTable(tb =>
            {
                tb.HasCheckConstraint("CK_FlowerBatchDetail_Quantity_Positive", "[Quantity] > 0");
                tb.HasCheckConstraint("CK_FlowerBatchDetail_UnitPrice_NonNegative", "[UnitPrice] >= 0");
                tb.HasCheckConstraint("CK_FlowerBatchDetail_QuantityAvailable_NonNegative", "[QuantityAvailable] >= 0");
                tb.HasCheckConstraint("CK_FlowerBatchDetail_QuantityAvailable_LessOrEqualQuantity", "[QuantityAvailable] <= [Quantity]");
            });

            // FlowerDamageLog constraints
            modelBuilder.Entity<FlowerDamageLog>().ToTable(tb =>
            {
                tb.HasCheckConstraint("CK_FlowerDamageLog_DamageQuantity_Positive", "[DamageQuantity] > 0");
            });

            // Order constraints
            modelBuilder.Entity<Order>().ToTable(tb =>
            {
                tb.HasCheckConstraint("CK_Order_TotalAmount_NonNegative", "[TotalAmount] >= 0");
            });

            // OrderDetail constraints
            modelBuilder.Entity<OrderDetail>().ToTable(tb =>
            {
                tb.HasCheckConstraint("CK_OrderDetail_Quantity_Positive", "[Quantity] > 0");
                tb.HasCheckConstraint("CK_OrderDetail_UnitPrice_NonNegative", "[UnitPrice] >= 0");
            });

            // Payment constraints
            modelBuilder.Entity<Payment>().ToTable(tb =>
            {
                tb.HasCheckConstraint("CK_Payment_Amount_Positive", "[Amount] > 0");
                tb.HasCheckConstraint("CK_Payment_PaymentStatus_Valid",
                    "[PaymentStatus] IS NULL OR [PaymentStatus] IN ('Pending', 'Processing', 'Completed', 'Failed', 'Cancelled', 'Refunded', 'PartialRefund')");
            });

            // Delivery constraints
            modelBuilder.Entity<Delivery>().ToTable(tb =>
            {
                tb.HasCheckConstraint("CK_Delivery_DeliveryStatus_Valid",
                    "[DeliveryStatus] IS NULL OR [DeliveryStatus] IN ('Pending', 'Confirmed', 'Preparing', 'InTransit', 'OutForDelivery', 'Delivered', 'Failed', 'Cancelled', 'Returned')");
            });

            // Product constraints
            modelBuilder.Entity<Product>().ToTable(tb =>
            {
                tb.HasCheckConstraint("CK_Product_Price_NonNegative", "[Price] >= 0");
            });

            // ProductFlower constraints
            modelBuilder.Entity<ProductFlower>().ToTable(tb =>
            {
                tb.HasCheckConstraint("CK_ProductFlower_QuantityUsed_Positive", "[QuantityUsed] > 0");
            });

            // ProductFlowerBatchUsage constraints
            modelBuilder.Entity<ProductFlowerBatchUsage>().ToTable(tb =>
            {
                tb.HasCheckConstraint("CK_ProductFlowerBatchUsage_QuantityUsed_Positive", "[QuantityUsed] > 0");
            });

            // SystemLog constraints
            modelBuilder.Entity<SystemLog>().ToTable(tb =>
            {
                tb.HasCheckConstraint("CK_SystemLog_Action_NotEmpty", "[Action] IS NULL OR [Action] != ''");
                tb.HasCheckConstraint("CK_SystemLog_TableName_NotEmpty", "[TableName] IS NULL OR [TableName] != ''");
            });

            // Base Entity constraints (applied to all entities)
            ConfigureBaseEntityTableConstraints(modelBuilder);
        }

        private void ConfigureBaseEntityTableConstraints(ModelBuilder modelBuilder)
        {
            var entityTypes = modelBuilder.Model.GetEntityTypes()
                .Where(t => typeof(BaseEntity).IsAssignableFrom(t.ClrType));

            foreach (var entityType in entityTypes)
            {
                var tableName = entityType.GetTableName();
                var clrType = entityType.ClrType;

                modelBuilder.Entity(clrType).ToTable(tb =>
                {
                    // CreatedAt cannot be in the future (allowing some tolerance for server time differences)
                    tb.HasCheckConstraint($"CK_{tableName}_CreatedAt_NotFuture",
                        "[CreatedAt] <= DATEADD(minute, 5, GETUTCDATE())");

                    // UpdatedAt must be after CreatedAt if not null
                    tb.HasCheckConstraint($"CK_{tableName}_UpdatedAt_AfterCreated",
                        "[UpdatedAt] IS NULL OR [UpdatedAt] >= [CreatedAt]");

                    // DeletedAt must be after CreatedAt if not null
                    tb.HasCheckConstraint($"CK_{tableName}_DeletedAt_AfterCreated",
                        "[DeletedAt] IS NULL OR [DeletedAt] >= [CreatedAt]");

                    // If IsDeleted is true, DeletedAt must not be null
                    tb.HasCheckConstraint($"CK_{tableName}_DeletedAt_RequiredWhenDeleted",
                        "[IsDeleted] = 0 OR ([IsDeleted] = 1 AND [DeletedAt] IS NOT NULL)");

                    // Id cannot be empty GUID
                    tb.HasCheckConstraint($"CK_{tableName}_Id_NotEmpty",
                        "[Id] != '00000000-0000-0000-0000-000000000000'");
                });
            }
        }

        private void ConfigureSoftDelete(ModelBuilder modelBuilder)
        {
            // Apply global query filter for soft delete
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
                {
                    modelBuilder.Entity(entityType.ClrType)
                        .HasQueryFilter(CreateIsNotDeletedFilter(entityType.ClrType));
                }
            }
        }

        private static System.Linq.Expressions.LambdaExpression CreateIsNotDeletedFilter(Type entityType)
        {
            var parameter = System.Linq.Expressions.Expression.Parameter(entityType, "e");
            var propertyAccess = System.Linq.Expressions.Expression.PropertyOrField(parameter, nameof(BaseEntity.IsDeleted));
            var notDeleted = System.Linq.Expressions.Expression.Equal(propertyAccess, System.Linq.Expressions.Expression.Constant(false));
            return System.Linq.Expressions.Expression.Lambda(notDeleted, parameter);
        }

        private void SeedDefaultData(ModelBuilder modelBuilder)
        {
            // Seed default roles
            var adminRoleId = Guid.NewGuid();
            var userRoleId = Guid.NewGuid();
            var staffRoleId = Guid.NewGuid();

            modelBuilder.Entity<Role>().HasData(
                new Role
                {
                    Id = adminRoleId,
                    RoleName = "Admin",
                    Description = "System Administrator",
                    CreatedAt = DateTime.UtcNow
                },
                new Role
                {
                    Id = userRoleId,
                    RoleName = "User",
                    Description = "Regular User",
                    CreatedAt = DateTime.UtcNow
                },
                new Role
                {
                    Id = staffRoleId,
                    RoleName = "Staff",
                    Description = "Staff Member",
                    CreatedAt = DateTime.UtcNow
                }
            );

            // Seed default permissions
            var permissions = new[]
            {
                new { Id = Guid.NewGuid(), Name = "VIEW_USERS", Description = "View users" },
                new { Id = Guid.NewGuid(), Name = "CREATE_USERS", Description = "Create users" },
                new { Id = Guid.NewGuid(), Name = "EDIT_USERS", Description = "Edit users" },
                new { Id = Guid.NewGuid(), Name = "DELETE_USERS", Description = "Delete users" },
                new { Id = Guid.NewGuid(), Name = "VIEW_ORDERS", Description = "View orders" },
                new { Id = Guid.NewGuid(), Name = "CREATE_ORDERS", Description = "Create orders" },
                new { Id = Guid.NewGuid(), Name = "EDIT_ORDERS", Description = "Edit orders" },
                new { Id = Guid.NewGuid(), Name = "DELETE_ORDERS", Description = "Delete orders" },
                new { Id = Guid.NewGuid(), Name = "VIEW_FLOWERS", Description = "View flowers" },
                new { Id = Guid.NewGuid(), Name = "CREATE_FLOWERS", Description = "Create flowers" },
                new { Id = Guid.NewGuid(), Name = "EDIT_FLOWERS", Description = "Edit flowers" },
                new { Id = Guid.NewGuid(), Name = "DELETE_FLOWERS", Description = "Delete flowers" },
                new { Id = Guid.NewGuid(), Name = "VIEW_REPORTS", Description = "View reports" }
            };

            foreach (var permission in permissions)
            {
                modelBuilder.Entity<Permission>().HasData(
                    new Permission
                    {
                        Id = permission.Id,
                        PermissionName = permission.Name,
                        Description = permission.Description,
                        CreatedAt = DateTime.UtcNow
                    }
                );
            }

            // Seed default admin user
            var adminUserId = Guid.NewGuid();
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = adminUserId,
                    UserName = "admin",
                    PasswordHash = "AQAAAAEAACcQAAAAEJ5G8J5G8J5G8J5G8J5G8J5G8J5G8J5G8J5G8J5G8J5G8J5G8J5G8J5G8J5G8J5G8J5G8J5G8J5G8J5G8=", // admin123
                    FullName = "System Administrator",
                    Email = "admin@flowershop.com",
                    RoleId = adminRoleId,
                    CreatedAt = DateTime.UtcNow
                }
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
                        entry.Entity.IsDeleted = false;
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

        // Method to include deleted entities in query (for admin purposes)
        public IQueryable<T> IncludeDeleted<T>() where T : BaseEntity
        {
            return Set<T>().IgnoreQueryFilters();
        }

        // Method to get only deleted entities
        public IQueryable<T> OnlyDeleted<T>() where T : BaseEntity
        {
            return Set<T>().IgnoreQueryFilters().Where(e => e.IsDeleted);
        }

        // Method to permanently delete an entity
        public void HardDelete<T>(T entity) where T : BaseEntity
        {
            Set<T>().Remove(entity);
        }

        // Design Time Factory implementation within the same class
        public class Factory : IDesignTimeDbContextFactory<FlowerSellingDbContext>
        {
            public FlowerSellingDbContext CreateDbContext(string[] args)
            {
                var optionsBuilder = new DbContextOptionsBuilder<FlowerSellingDbContext>();

                // Try to get connection string from arguments first
                string connectionString = null;
                if (args.Length > 0)
                {
                    connectionString = args[0];
                }

                // If no connection string from args, try configuration
                if (string.IsNullOrEmpty(connectionString))
                {
                    try
                    {
                        var configuration = new ConfigurationBuilder()
                            .SetBasePath(Directory.GetCurrentDirectory())
                            .AddJsonFile("appsettings.json", optional: true)
                            .AddJsonFile("appsettings.Development.json", optional: true)
                            .AddEnvironmentVariables()
                            .Build();

                        connectionString = configuration.GetConnectionString("DefaultConnection");
                    }
                    catch
                    {
                        // Ignore configuration errors
                    }
                }

                optionsBuilder.UseSqlServer(connectionString);
                return new FlowerSellingDbContext(optionsBuilder.Options);
            }
        }
    }
}