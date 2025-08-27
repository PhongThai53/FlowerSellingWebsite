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
            public DbSet<FlowerDamageLogs> FlowerDamageLogs { get; set; }

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
            public DbSet<Invoices> Invoices { get; set; }
            public DbSet<Deliveries> Deliveries { get; set; }

            public DbSet<Suppliers> Suppliers { get; set; }
            public DbSet<SupplierListings> SupplierListings { get; set; }
            public DbSet<PurchaseOrders> PurchaseOrders { get; set; }
            public DbSet<PurchaseOrderDetails> PurchaseOrderDetails { get; set; }

            public DbSet<Blog> Blogs { get; set; }
            public DbSet<Comment> Comments { get; set; }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                base.OnModelCreating(modelBuilder);

                // ===== CONFIGURE RELATIONSHIPS =====

                // Users - Roles relationship
                modelBuilder.Entity<Users>()
                    .HasOne(u => u.Role)
                    .WithMany(r => r.Users)
                    .HasForeignKey(u => u.RoleId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Users - Suppliers relationship
                modelBuilder.Entity<Users>()
                    .HasOne(u => u.Supplier)
                    .WithMany(s => s.Users)
                    .HasForeignKey(u => u.SupplierId)
                    .OnDelete(DeleteBehavior.Restrict);

                // RolePermissions relationships
                modelBuilder.Entity<RolePermissions>()
                    .HasOne(rp => rp.Role)
                    .WithMany(r => r.RolePermissions)
                    .HasForeignKey(rp => rp.RoleId)
                    .OnDelete(DeleteBehavior.Cascade);

                modelBuilder.Entity<RolePermissions>()
                    .HasOne(rp => rp.Permission)
                    .WithMany(p => p.RolePermissions)
                    .HasForeignKey(rp => rp.PermissionId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Flowers relationships
                modelBuilder.Entity<Flowers>()
                    .HasOne(f => f.FlowerCategory)
                    .WithMany(fc => fc.Flowers)
                    .HasForeignKey(f => f.FlowerCategoryId)
                    .OnDelete(DeleteBehavior.SetNull);

                modelBuilder.Entity<Flowers>()
                    .HasOne(f => f.FlowerType)
                    .WithMany(ft => ft.Flowers)
                    .HasForeignKey(f => f.FlowerTypeId)
                    .OnDelete(DeleteBehavior.Restrict);

                modelBuilder.Entity<Flowers>()
                    .HasOne(f => f.FlowerColor)
                    .WithMany(fc => fc.Flowers)
                    .HasForeignKey(f => f.FlowerColorId)
                    .OnDelete(DeleteBehavior.Restrict);

                // FlowerImages relationships
                modelBuilder.Entity<FlowerImages>()
                    .HasOne(fi => fi.Flower)
                    .WithMany(f => f.FlowerImages)
                    .HasForeignKey(fi => fi.FlowerId)
                    .OnDelete(DeleteBehavior.Cascade);

                // FlowerPricing relationships
                modelBuilder.Entity<FlowerPricing>()
                    .HasOne(fp => fp.Flower)
                    .WithMany(f => f.FlowerPricings)
                    .HasForeignKey(fp => fp.FlowerId)
                    .OnDelete(DeleteBehavior.Cascade);

                // FlowerDamageLogs relationships
                modelBuilder.Entity<FlowerDamageLogs>()
                    .HasOne(fdl => fdl.PurchaseOrderDetail)
                    .WithMany(pod => pod.FlowerDamageLogs)
                    .HasForeignKey(fdl => fdl.PurchaseOrderDetailId)
                    .OnDelete(DeleteBehavior.Restrict);

                modelBuilder.Entity<FlowerDamageLogs>()
                    .HasOne(fdl => fdl.ReportedByUser)
                    .WithMany(u => u.FlowerDamageLogs)
                    .HasForeignKey(fdl => fdl.ReportedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Products relationships
                modelBuilder.Entity<Products>()
                    .HasOne(p => p.ProductCategories)
                    .WithMany(pc => pc.Products)
                    .HasForeignKey(p => p.CategoryId)
                    .OnDelete(DeleteBehavior.Restrict);

                // ProductPhotos relationships
                modelBuilder.Entity<ProductPhotos>()
                    .HasOne(pp => pp.Product)
                    .WithMany(p => p.ProductPhotos)
                    .HasForeignKey(pp => pp.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);

                // ProductFlowers relationships
                modelBuilder.Entity<ProductFlowers>()
                    .HasOne(pf => pf.Product)
                    .WithMany(p => p.ProductFlowers)
                    .HasForeignKey(pf => pf.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);

                modelBuilder.Entity<ProductFlowers>()
                    .HasOne(pf => pf.Flower)
                    .WithMany(f => f.ProductFlowers)
                    .HasForeignKey(pf => pf.FlowerId)
                    .OnDelete(DeleteBehavior.Restrict);

                // ProductPriceHistories relationships
                modelBuilder.Entity<ProductPriceHistories>()
                    .HasOne(pph => pph.Products)
                    .WithMany(p => p.PriceHistories)
                    .HasForeignKey(pph => pph.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Cart relationships
                modelBuilder.Entity<Cart>()
                    .HasOne(c => c.User)
                    .WithMany()
                    .HasForeignKey(c => c.UserId)
                    .OnDelete(DeleteBehavior.Restrict);


                // CartItem relationships
                modelBuilder.Entity<CartItem>()
                    .HasOne(ci => ci.Cart)
                    .WithMany(c => c.CartItems)
                    .HasForeignKey(ci => ci.CartId)
                    .OnDelete(DeleteBehavior.Cascade);

                modelBuilder.Entity<CartItem>()
                    .HasOne(ci => ci.Product)
                    .WithMany()
                    .HasForeignKey(ci => ci.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Orders relationships
                modelBuilder.Entity<Orders>()
                    .HasOne(o => o.Customer)
                    .WithMany(u => u.Orders)
                    .HasForeignKey(o => o.CustomerId)
                    .OnDelete(DeleteBehavior.Restrict);

                // OrderDetails relationships
                modelBuilder.Entity<OrderDetails>()
                    .HasOne(od => od.Order)
                    .WithMany(o => o.OrderDetails)
                    .HasForeignKey(od => od.OrderId)
                    .OnDelete(DeleteBehavior.Cascade);

                modelBuilder.Entity<OrderDetails>()
                    .HasOne(od => od.Product)
                    .WithMany(p => p.OrderDetails)
                    .HasForeignKey(od => od.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Payments relationships
                modelBuilder.Entity<Payments>()
                    .HasOne(p => p.Order)
                    .WithMany(o => o.Payments)
                    .HasForeignKey(p => p.OrderId)
                    .OnDelete(DeleteBehavior.Cascade);

                modelBuilder.Entity<Payments>()
                    .HasOne(p => p.PaymentMethod)
                    .WithMany(pm => pm.Payments)
                    .HasForeignKey(p => p.PaymentMethodId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Invoices relationships
                modelBuilder.Entity<Invoices>()
                    .HasOne(i => i.Order)
                    .WithMany(o => o.Invoices)
                    .HasForeignKey(i => i.OrderId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Deliveries relationships
                modelBuilder.Entity<Deliveries>()
                    .HasOne(d => d.Order)
                    .WithMany(o => o.Deliveries)
                    .HasForeignKey(d => d.OrderId)
                    .OnDelete(DeleteBehavior.Cascade);

                // SupplierListings relationships
                modelBuilder.Entity<SupplierListings>()
                    .HasOne(sl => sl.Supplier)
                    .WithMany(s => s.SupplierListings)
                    .HasForeignKey(sl => sl.SupplierId)
                    .OnDelete(DeleteBehavior.Restrict);

                modelBuilder.Entity<SupplierListings>()
                    .HasOne(sl => sl.Flower)
                    .WithMany(f => f.SupplierListings)
                    .HasForeignKey(sl => sl.FlowerId)
                    .OnDelete(DeleteBehavior.Restrict);

                // PurchaseOrders relationships
                modelBuilder.Entity<PurchaseOrders>()
                    .HasOne(po => po.Supplier)
                    .WithMany(s => s.PurchaseOrders)
                    .HasForeignKey(po => po.SupplierId)
                    .OnDelete(DeleteBehavior.Restrict);

                // PurchaseOrderDetails relationships
                modelBuilder.Entity<PurchaseOrderDetails>()
                    .HasOne(pod => pod.PurchaseOrder)
                    .WithMany(po => po.PurchaseOrderDetails)
                    .HasForeignKey(pod => pod.PurchaseOrderId)
                    .OnDelete(DeleteBehavior.Cascade);

                modelBuilder.Entity<PurchaseOrderDetails>()
                    .HasOne(pod => pod.Flower)
                    .WithMany(f => f.PurchaseOrderDetails)
                    .HasForeignKey(pod => pod.FlowerId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Blog relationships
                modelBuilder.Entity<Blog>()
                    .HasOne(b => b.User)
                    .WithMany()
                    .HasForeignKey(b => b.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                modelBuilder.Entity<Blog>()
                    .HasOne(b => b.Category)
                    .WithMany(fc => fc.Blogs)
                    .HasForeignKey(b => b.CategoryId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Comment relationships
                modelBuilder.Entity<Comment>()
                    .HasOne(c => c.User)
                    .WithMany(u => u.Comments)
                    .HasForeignKey(c => c.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                modelBuilder.Entity<Comment>()
                    .HasOne(c => c.Blog)
                    .WithMany(b => b.Comments)
                    .HasForeignKey(c => c.BlogId)
                    .OnDelete(DeleteBehavior.Cascade);

                modelBuilder.Entity<Comment>()
                    .HasOne(c => c.Parent)
                    .WithMany(c => c.Children)
                    .HasForeignKey(c => c.ParentId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Configure BaseEntity properties for all entities
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
                ConfigureBaseEntityProperties<FlowerDamageLogs>(modelBuilder);
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
                ConfigureBaseEntityProperties<Invoices>(modelBuilder);
                ConfigureBaseEntityProperties<Deliveries>(modelBuilder);
                ConfigureBaseEntityProperties<Suppliers>(modelBuilder);
                ConfigureBaseEntityProperties<SupplierListings>(modelBuilder);
                ConfigureBaseEntityProperties<PurchaseOrders>(modelBuilder);
                ConfigureBaseEntityProperties<PurchaseOrderDetails>(modelBuilder);
                ConfigureBaseEntityProperties<Blog>(modelBuilder);
                ConfigureBaseEntityProperties<Comment>(modelBuilder);

                // ===== ENTITY CONFIGURATIONS =====

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
                    entity.Property(e => e.Price).HasColumnType("decimal(10,2)");
                    entity.Property(e => e.Currency).HasMaxLength(3).IsRequired();
                    entity.Property(e => e.PriceType).HasMaxLength(20);
                });

                modelBuilder.Entity<FlowerDamageLogs>(entity =>
                {
                    entity.Property(e => e.DamageReason).HasMaxLength(500).IsRequired();
                    entity.Property(e => e.Notes).HasMaxLength(1000);
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
                    entity.Property(e => e.CustomerFirstName).HasMaxLength(100);
                    entity.Property(e => e.CustomerLastName).HasMaxLength(100);
                    entity.Property(e => e.CustomerEmail).HasMaxLength(100);
                    entity.Property(e => e.CustomerPhone).HasMaxLength(20);
                    entity.Property(e => e.CompanyName).HasMaxLength(100);
                    entity.Property(e => e.Country).HasMaxLength(100);
                    entity.Property(e => e.City).HasMaxLength(100);
                    entity.Property(e => e.State).HasMaxLength(100);
                    entity.Property(e => e.Postcode).HasMaxLength(20);
                    entity.Property(e => e.StreetAddress).HasMaxLength(500);
                    entity.Property(e => e.StreetAddress2).HasMaxLength(500);
                    entity.Property(e => e.InvoiceNumber).HasMaxLength(50);
                    entity.Property(e => e.InvoiceExportPath).HasMaxLength(500);
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
                    entity.Property(e => e.VNPayTransactionId).HasMaxLength(100);
                    entity.Property(e => e.VNPayResponseCode).HasMaxLength(10);
                    entity.Property(e => e.VNPayResponseMessage).HasMaxLength(500);
                    entity.Property(e => e.VNPayBankCode).HasMaxLength(20);
                    entity.Property(e => e.VNPayCardType).HasMaxLength(20);
                    entity.Property(e => e.VNPaySecureHash).HasMaxLength(500);
                    entity.Property(e => e.VNPayGatewayUrl).HasMaxLength(500);
                    entity.Property(e => e.VNPayLocale).HasMaxLength(10);
                    entity.Property(e => e.VNPayCurrencyCode).HasMaxLength(3);
                    entity.Property(e => e.VNPayTxnRef).HasMaxLength(100);
                    entity.Property(e => e.VNPayOrderInfo).HasMaxLength(500);
                    entity.Property(e => e.VNPayReturnUrl).HasMaxLength(500);
                    entity.Property(e => e.VNPayCancelUrl).HasMaxLength(500);
                });

                modelBuilder.Entity<PaymentMethods>(entity =>
                {
                    entity.Property(e => e.MethodName).HasMaxLength(100).IsRequired();
                    entity.Property(e => e.Description).HasMaxLength(500);
                    entity.Property(e => e.MethodType).HasMaxLength(50);
                    entity.Property(e => e.IconClass).HasMaxLength(100);
                    entity.Property(e => e.DisplayName).HasMaxLength(100);
                });

                modelBuilder.Entity<Invoices>(entity =>
                {
                    entity.Property(e => e.InvoiceNumber).HasMaxLength(50).IsRequired();
                    entity.Property(e => e.Subtotal).HasColumnType("decimal(18,2)");
                    entity.Property(e => e.TaxAmount).HasColumnType("decimal(18,2)");
                    entity.Property(e => e.TotalAmount).HasColumnType("decimal(18,2)");
                    entity.Property(e => e.CustomerName).HasMaxLength(200);
                    entity.Property(e => e.CustomerAddress).HasMaxLength(500);
                    entity.Property(e => e.CustomerPhone).HasMaxLength(20);
                    entity.Property(e => e.CustomerEmail).HasMaxLength(200);
                    entity.Property(e => e.CompanyName).HasMaxLength(200);
                    entity.Property(e => e.CompanyAddress).HasMaxLength(500);
                    entity.Property(e => e.CompanyTaxCode).HasMaxLength(50);
                    entity.Property(e => e.CompanyPhone).HasMaxLength(20);
                    entity.Property(e => e.CompanyEmail).HasMaxLength(200);
                    entity.Property(e => e.ExportPath).HasMaxLength(500);
                    entity.Property(e => e.ExportFormat).HasMaxLength(20);
                    entity.Property(e => e.Notes).HasMaxLength(1000);

                    entity.HasIndex(e => e.InvoiceNumber).IsUnique();
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

                // ===== BLOG =====
                modelBuilder.Entity<Blog>(entity =>
                {
                    entity.Property(e => e.Title).HasMaxLength(250).IsRequired();
                    entity.Property(e => e.Content).HasColumnType("ntext").IsRequired();
                    entity.Property(e => e.Tags).HasMaxLength(1000);
                    entity.Property(e => e.RejectionReason).HasMaxLength(1000);

                    // Configure Images as JSON
                    entity.Property(e => e.Images)
                        .HasColumnType("nvarchar(max)")
                        .HasConversion(
                            v => string.Join(',', v),
                            v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()
                        );
                });

                modelBuilder.Entity<Comment>(entity =>
                {
                    entity.Property(e => e.Content).HasMaxLength(2000).IsRequired();
                });

                // ===== COMPOSITE KEYS =====
                modelBuilder.Entity<RolePermissions>()
                    .HasKey(rp => new { rp.RoleId, rp.PermissionId });

                modelBuilder.Entity<ProductFlowers>()
                    .HasKey(pf => new { pf.ProductId, pf.FlowerId });

                // ===== GLOBAL QUERY FILTERS FOR SOFT DELETE =====
                modelBuilder.Entity<Users>().HasQueryFilter(e => !e.IsDeleted);
                modelBuilder.Entity<Roles>().HasQueryFilter(e => !e.IsDeleted);
                modelBuilder.Entity<Permissions>().HasQueryFilter(e => !e.IsDeleted);
                modelBuilder.Entity<Flowers>().HasQueryFilter(e => !e.IsDeleted);
                modelBuilder.Entity<FlowerCategories>().HasQueryFilter(e => !e.IsDeleted);
                modelBuilder.Entity<FlowerTypes>().HasQueryFilter(e => !e.IsDeleted);
                modelBuilder.Entity<FlowerColors>().HasQueryFilter(e => !e.IsDeleted);
                modelBuilder.Entity<Products>().HasQueryFilter(e => !e.IsDeleted); // Restored to protect shop from deleted products
                modelBuilder.Entity<ProductCategories>().HasQueryFilter(e => !e.IsDeleted);
                modelBuilder.Entity<Orders>().HasQueryFilter(e => !e.IsDeleted);
                modelBuilder.Entity<Suppliers>().HasQueryFilter(e => !e.IsDeleted);
                modelBuilder.Entity<Blog>().HasQueryFilter(e => !e.IsDeleted);
                modelBuilder.Entity<Comment>().HasQueryFilter(e => !e.IsDeleted);
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
                var entries = ChangeTracker.Entries().Where(e => e.Entity is BaseEntity &&
                    (e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted));

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