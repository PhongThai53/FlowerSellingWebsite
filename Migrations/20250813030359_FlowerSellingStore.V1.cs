using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace FlowerSellingWebsite.Migrations
{
    /// <inheritdoc />
    public partial class FlowerSellingStoreV1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Permissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PermissionName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permissions", x => x.Id);
                    table.CheckConstraint("CK_Permission_PermissionName_NotEmpty", "[PermissionName] != ''");
                    table.CheckConstraint("CK_Permissions_CreatedAt_NotFuture", "[CreatedAt] <= DATEADD(minute, 5, GETUTCDATE())");
                    table.CheckConstraint("CK_Permissions_DeletedAt_AfterCreated", "[DeletedAt] IS NULL OR [DeletedAt] >= [CreatedAt]");
                    table.CheckConstraint("CK_Permissions_DeletedAt_RequiredWhenDeleted", "[IsDeleted] = 0 OR ([IsDeleted] = 1 AND [DeletedAt] IS NOT NULL)");
                    table.CheckConstraint("CK_Permissions_Id_NotEmpty", "[Id] != '00000000-0000-0000-0000-000000000000'");
                    table.CheckConstraint("CK_Permissions_UpdatedAt_AfterCreated", "[UpdatedAt] IS NULL OR [UpdatedAt] >= [CreatedAt]");
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                    table.CheckConstraint("CK_Product_Price_NonNegative", "[Price] >= 0");
                    table.CheckConstraint("CK_Products_CreatedAt_NotFuture", "[CreatedAt] <= DATEADD(minute, 5, GETUTCDATE())");
                    table.CheckConstraint("CK_Products_DeletedAt_AfterCreated", "[DeletedAt] IS NULL OR [DeletedAt] >= [CreatedAt]");
                    table.CheckConstraint("CK_Products_DeletedAt_RequiredWhenDeleted", "[IsDeleted] = 0 OR ([IsDeleted] = 1 AND [DeletedAt] IS NOT NULL)");
                    table.CheckConstraint("CK_Products_Id_NotEmpty", "[Id] != '00000000-0000-0000-0000-000000000000'");
                    table.CheckConstraint("CK_Products_UpdatedAt_AfterCreated", "[UpdatedAt] IS NULL OR [UpdatedAt] >= [CreatedAt]");
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoleName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                    table.CheckConstraint("CK_Role_RoleName_NotEmpty", "[RoleName] != ''");
                    table.CheckConstraint("CK_Roles_CreatedAt_NotFuture", "[CreatedAt] <= DATEADD(minute, 5, GETUTCDATE())");
                    table.CheckConstraint("CK_Roles_DeletedAt_AfterCreated", "[DeletedAt] IS NULL OR [DeletedAt] >= [CreatedAt]");
                    table.CheckConstraint("CK_Roles_DeletedAt_RequiredWhenDeleted", "[IsDeleted] = 0 OR ([IsDeleted] = 1 AND [DeletedAt] IS NOT NULL)");
                    table.CheckConstraint("CK_Roles_Id_NotEmpty", "[Id] != '00000000-0000-0000-0000-000000000000'");
                    table.CheckConstraint("CK_Roles_UpdatedAt_AfterCreated", "[UpdatedAt] IS NULL OR [UpdatedAt] >= [CreatedAt]");
                });

            migrationBuilder.CreateTable(
                name: "Suppliers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SupplierName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    ContactPerson = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Suppliers", x => x.Id);
                    table.CheckConstraint("CK_Supplier_SupplierName_NotEmpty", "[SupplierName] != ''");
                    table.CheckConstraint("CK_Suppliers_CreatedAt_NotFuture", "[CreatedAt] <= DATEADD(minute, 5, GETUTCDATE())");
                    table.CheckConstraint("CK_Suppliers_DeletedAt_AfterCreated", "[DeletedAt] IS NULL OR [DeletedAt] >= [CreatedAt]");
                    table.CheckConstraint("CK_Suppliers_DeletedAt_RequiredWhenDeleted", "[IsDeleted] = 0 OR ([IsDeleted] = 1 AND [DeletedAt] IS NOT NULL)");
                    table.CheckConstraint("CK_Suppliers_Id_NotEmpty", "[Id] != '00000000-0000-0000-0000-000000000000'");
                    table.CheckConstraint("CK_Suppliers_UpdatedAt_AfterCreated", "[UpdatedAt] IS NULL OR [UpdatedAt] >= [CreatedAt]");
                });

            migrationBuilder.CreateTable(
                name: "RolePermissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PermissionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolePermissions", x => x.Id);
                    table.CheckConstraint("CK_RolePermissions_CreatedAt_NotFuture", "[CreatedAt] <= DATEADD(minute, 5, GETUTCDATE())");
                    table.CheckConstraint("CK_RolePermissions_DeletedAt_AfterCreated", "[DeletedAt] IS NULL OR [DeletedAt] >= [CreatedAt]");
                    table.CheckConstraint("CK_RolePermissions_DeletedAt_RequiredWhenDeleted", "[IsDeleted] = 0 OR ([IsDeleted] = 1 AND [DeletedAt] IS NOT NULL)");
                    table.CheckConstraint("CK_RolePermissions_Id_NotEmpty", "[Id] != '00000000-0000-0000-0000-000000000000'");
                    table.CheckConstraint("CK_RolePermissions_UpdatedAt_AfterCreated", "[UpdatedAt] IS NULL OR [UpdatedAt] >= [CreatedAt]");
                    table.ForeignKey(
                        name: "FK_RolePermissions_Permissions_PermissionId",
                        column: x => x.PermissionId,
                        principalTable: "Permissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RolePermissions_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    FullName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.CheckConstraint("CK_User_Email_Format", "[Email] IS NULL OR [Email] LIKE '%@%.%'");
                    table.CheckConstraint("CK_User_Phone_Format", "[Phone] IS NULL OR LEN([Phone]) >= 10");
                    table.CheckConstraint("CK_User_UserName_NotEmpty", "[UserName] != ''");
                    table.CheckConstraint("CK_Users_CreatedAt_NotFuture", "[CreatedAt] <= DATEADD(minute, 5, GETUTCDATE())");
                    table.CheckConstraint("CK_Users_DeletedAt_AfterCreated", "[DeletedAt] IS NULL OR [DeletedAt] >= [CreatedAt]");
                    table.CheckConstraint("CK_Users_DeletedAt_RequiredWhenDeleted", "[IsDeleted] = 0 OR ([IsDeleted] = 1 AND [DeletedAt] IS NOT NULL)");
                    table.CheckConstraint("CK_Users_Id_NotEmpty", "[Id] != '00000000-0000-0000-0000-000000000000'");
                    table.CheckConstraint("CK_Users_UpdatedAt_AfterCreated", "[UpdatedAt] IS NULL OR [UpdatedAt] >= [CreatedAt]");
                    table.ForeignKey(
                        name: "FK_Users_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Flowers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    StockQuantity = table.Column<int>(type: "int", nullable: false),
                    SupplierId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Flowers", x => x.Id);
                    table.CheckConstraint("CK_Flower_Name_NotEmpty", "[Name] != ''");
                    table.CheckConstraint("CK_Flower_Price_NonNegative", "[Price] >= 0");
                    table.CheckConstraint("CK_Flower_StockQuantity_NonNegative", "[StockQuantity] >= 0");
                    table.CheckConstraint("CK_Flowers_CreatedAt_NotFuture", "[CreatedAt] <= DATEADD(minute, 5, GETUTCDATE())");
                    table.CheckConstraint("CK_Flowers_DeletedAt_AfterCreated", "[DeletedAt] IS NULL OR [DeletedAt] >= [CreatedAt]");
                    table.CheckConstraint("CK_Flowers_DeletedAt_RequiredWhenDeleted", "[IsDeleted] = 0 OR ([IsDeleted] = 1 AND [DeletedAt] IS NOT NULL)");
                    table.CheckConstraint("CK_Flowers_Id_NotEmpty", "[Id] != '00000000-0000-0000-0000-000000000000'");
                    table.CheckConstraint("CK_Flowers_UpdatedAt_AfterCreated", "[UpdatedAt] IS NULL OR [UpdatedAt] >= [CreatedAt]");
                    table.ForeignKey(
                        name: "FK_Flowers_Suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Suppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.Id);
                    table.CheckConstraint("CK_Order_TotalAmount_NonNegative", "[TotalAmount] >= 0");
                    table.CheckConstraint("CK_Orders_CreatedAt_NotFuture", "[CreatedAt] <= DATEADD(minute, 5, GETUTCDATE())");
                    table.CheckConstraint("CK_Orders_DeletedAt_AfterCreated", "[DeletedAt] IS NULL OR [DeletedAt] >= [CreatedAt]");
                    table.CheckConstraint("CK_Orders_DeletedAt_RequiredWhenDeleted", "[IsDeleted] = 0 OR ([IsDeleted] = 1 AND [DeletedAt] IS NOT NULL)");
                    table.CheckConstraint("CK_Orders_Id_NotEmpty", "[Id] != '00000000-0000-0000-0000-000000000000'");
                    table.CheckConstraint("CK_Orders_UpdatedAt_AfterCreated", "[UpdatedAt] IS NULL OR [UpdatedAt] >= [CreatedAt]");
                    table.ForeignKey(
                        name: "FK_Orders_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FlowerBatches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SupplierId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FlowerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FlowerBatches", x => x.Id);
                    table.CheckConstraint("CK_FlowerBatch_TotalAmount_NonNegative", "[TotalAmount] >= 0");
                    table.CheckConstraint("CK_FlowerBatches_CreatedAt_NotFuture", "[CreatedAt] <= DATEADD(minute, 5, GETUTCDATE())");
                    table.CheckConstraint("CK_FlowerBatches_DeletedAt_AfterCreated", "[DeletedAt] IS NULL OR [DeletedAt] >= [CreatedAt]");
                    table.CheckConstraint("CK_FlowerBatches_DeletedAt_RequiredWhenDeleted", "[IsDeleted] = 0 OR ([IsDeleted] = 1 AND [DeletedAt] IS NOT NULL)");
                    table.CheckConstraint("CK_FlowerBatches_Id_NotEmpty", "[Id] != '00000000-0000-0000-0000-000000000000'");
                    table.CheckConstraint("CK_FlowerBatches_UpdatedAt_AfterCreated", "[UpdatedAt] IS NULL OR [UpdatedAt] >= [CreatedAt]");
                    table.ForeignKey(
                        name: "FK_FlowerBatches_Flowers_FlowerId",
                        column: x => x.FlowerId,
                        principalTable: "Flowers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FlowerBatches_Suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Suppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProductFlowers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FlowerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuantityUsed = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductFlowers", x => x.Id);
                    table.CheckConstraint("CK_ProductFlower_QuantityUsed_Positive", "[QuantityUsed] > 0");
                    table.CheckConstraint("CK_ProductFlowers_CreatedAt_NotFuture", "[CreatedAt] <= DATEADD(minute, 5, GETUTCDATE())");
                    table.CheckConstraint("CK_ProductFlowers_DeletedAt_AfterCreated", "[DeletedAt] IS NULL OR [DeletedAt] >= [CreatedAt]");
                    table.CheckConstraint("CK_ProductFlowers_DeletedAt_RequiredWhenDeleted", "[IsDeleted] = 0 OR ([IsDeleted] = 1 AND [DeletedAt] IS NOT NULL)");
                    table.CheckConstraint("CK_ProductFlowers_Id_NotEmpty", "[Id] != '00000000-0000-0000-0000-000000000000'");
                    table.CheckConstraint("CK_ProductFlowers_UpdatedAt_AfterCreated", "[UpdatedAt] IS NULL OR [UpdatedAt] >= [CreatedAt]");
                    table.ForeignKey(
                        name: "FK_ProductFlowers_Flowers_FlowerId",
                        column: x => x.FlowerId,
                        principalTable: "Flowers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductFlowers_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Deliveries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DeliveryStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DeliveryAddress = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    DeliveryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Deliveries", x => x.Id);
                    table.CheckConstraint("CK_Deliveries_CreatedAt_NotFuture", "[CreatedAt] <= DATEADD(minute, 5, GETUTCDATE())");
                    table.CheckConstraint("CK_Deliveries_DeletedAt_AfterCreated", "[DeletedAt] IS NULL OR [DeletedAt] >= [CreatedAt]");
                    table.CheckConstraint("CK_Deliveries_DeletedAt_RequiredWhenDeleted", "[IsDeleted] = 0 OR ([IsDeleted] = 1 AND [DeletedAt] IS NOT NULL)");
                    table.CheckConstraint("CK_Deliveries_Id_NotEmpty", "[Id] != '00000000-0000-0000-0000-000000000000'");
                    table.CheckConstraint("CK_Deliveries_UpdatedAt_AfterCreated", "[UpdatedAt] IS NULL OR [UpdatedAt] >= [CreatedAt]");
                    table.CheckConstraint("CK_Delivery_DeliveryStatus_Valid", "[DeliveryStatus] IS NULL OR [DeliveryStatus] IN ('Pending', 'Confirmed', 'Preparing', 'InTransit', 'OutForDelivery', 'Delivered', 'Failed', 'Cancelled', 'Returned')");
                    table.ForeignKey(
                        name: "FK_Deliveries_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrderDetails",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    FlowerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderDetails", x => x.Id);
                    table.CheckConstraint("CK_OrderDetail_Quantity_Positive", "[Quantity] > 0");
                    table.CheckConstraint("CK_OrderDetail_UnitPrice_NonNegative", "[UnitPrice] >= 0");
                    table.CheckConstraint("CK_OrderDetails_CreatedAt_NotFuture", "[CreatedAt] <= DATEADD(minute, 5, GETUTCDATE())");
                    table.CheckConstraint("CK_OrderDetails_DeletedAt_AfterCreated", "[DeletedAt] IS NULL OR [DeletedAt] >= [CreatedAt]");
                    table.CheckConstraint("CK_OrderDetails_DeletedAt_RequiredWhenDeleted", "[IsDeleted] = 0 OR ([IsDeleted] = 1 AND [DeletedAt] IS NOT NULL)");
                    table.CheckConstraint("CK_OrderDetails_Id_NotEmpty", "[Id] != '00000000-0000-0000-0000-000000000000'");
                    table.CheckConstraint("CK_OrderDetails_UpdatedAt_AfterCreated", "[UpdatedAt] IS NULL OR [UpdatedAt] >= [CreatedAt]");
                    table.ForeignKey(
                        name: "FK_OrderDetails_Flowers_FlowerId",
                        column: x => x.FlowerId,
                        principalTable: "Flowers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_OrderDetails_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderDetails_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PaymentMethodId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PaymentStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.Id);
                    table.CheckConstraint("CK_Payment_Amount_Positive", "[Amount] > 0");
                    table.CheckConstraint("CK_Payment_PaymentStatus_Valid", "[PaymentStatus] IS NULL OR [PaymentStatus] IN ('Pending', 'Processing', 'Completed', 'Failed', 'Cancelled', 'Refunded', 'PartialRefund')");
                    table.CheckConstraint("CK_Payments_CreatedAt_NotFuture", "[CreatedAt] <= DATEADD(minute, 5, GETUTCDATE())");
                    table.CheckConstraint("CK_Payments_DeletedAt_AfterCreated", "[DeletedAt] IS NULL OR [DeletedAt] >= [CreatedAt]");
                    table.CheckConstraint("CK_Payments_DeletedAt_RequiredWhenDeleted", "[IsDeleted] = 0 OR ([IsDeleted] = 1 AND [DeletedAt] IS NOT NULL)");
                    table.CheckConstraint("CK_Payments_Id_NotEmpty", "[Id] != '00000000-0000-0000-0000-000000000000'");
                    table.CheckConstraint("CK_Payments_UpdatedAt_AfterCreated", "[UpdatedAt] IS NULL OR [UpdatedAt] >= [CreatedAt]");
                    table.ForeignKey(
                        name: "FK_Payments_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SystemLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    RecordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TableName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IPAddress = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemLogs", x => x.Id);
                    table.CheckConstraint("CK_SystemLog_Action_NotEmpty", "[Action] IS NULL OR [Action] != ''");
                    table.CheckConstraint("CK_SystemLog_TableName_NotEmpty", "[TableName] IS NULL OR [TableName] != ''");
                    table.CheckConstraint("CK_SystemLogs_CreatedAt_NotFuture", "[CreatedAt] <= DATEADD(minute, 5, GETUTCDATE())");
                    table.CheckConstraint("CK_SystemLogs_DeletedAt_AfterCreated", "[DeletedAt] IS NULL OR [DeletedAt] >= [CreatedAt]");
                    table.CheckConstraint("CK_SystemLogs_DeletedAt_RequiredWhenDeleted", "[IsDeleted] = 0 OR ([IsDeleted] = 1 AND [DeletedAt] IS NOT NULL)");
                    table.CheckConstraint("CK_SystemLogs_Id_NotEmpty", "[Id] != '00000000-0000-0000-0000-000000000000'");
                    table.CheckConstraint("CK_SystemLogs_UpdatedAt_AfterCreated", "[UpdatedAt] IS NULL OR [UpdatedAt] >= [CreatedAt]");
                    table.ForeignKey(
                        name: "FK_SystemLogs_Orders_RecordId",
                        column: x => x.RecordId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SystemLogs_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FlowerBatchDetails",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FlowerBatchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FlowerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    QuantityAvailable = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FlowerBatchDetails", x => x.Id);
                    table.CheckConstraint("CK_FlowerBatchDetail_Quantity_Positive", "[Quantity] > 0");
                    table.CheckConstraint("CK_FlowerBatchDetail_QuantityAvailable_LessOrEqualQuantity", "[QuantityAvailable] <= [Quantity]");
                    table.CheckConstraint("CK_FlowerBatchDetail_QuantityAvailable_NonNegative", "[QuantityAvailable] >= 0");
                    table.CheckConstraint("CK_FlowerBatchDetail_UnitPrice_NonNegative", "[UnitPrice] >= 0");
                    table.CheckConstraint("CK_FlowerBatchDetails_CreatedAt_NotFuture", "[CreatedAt] <= DATEADD(minute, 5, GETUTCDATE())");
                    table.CheckConstraint("CK_FlowerBatchDetails_DeletedAt_AfterCreated", "[DeletedAt] IS NULL OR [DeletedAt] >= [CreatedAt]");
                    table.CheckConstraint("CK_FlowerBatchDetails_DeletedAt_RequiredWhenDeleted", "[IsDeleted] = 0 OR ([IsDeleted] = 1 AND [DeletedAt] IS NOT NULL)");
                    table.CheckConstraint("CK_FlowerBatchDetails_Id_NotEmpty", "[Id] != '00000000-0000-0000-0000-000000000000'");
                    table.CheckConstraint("CK_FlowerBatchDetails_UpdatedAt_AfterCreated", "[UpdatedAt] IS NULL OR [UpdatedAt] >= [CreatedAt]");
                    table.ForeignKey(
                        name: "FK_FlowerBatchDetails_FlowerBatches_FlowerBatchId",
                        column: x => x.FlowerBatchId,
                        principalTable: "FlowerBatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FlowerBatchDetails_Flowers_FlowerId",
                        column: x => x.FlowerId,
                        principalTable: "Flowers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FlowerDamageLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FlowerBatchDetailId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DamageQuantity = table.Column<int>(type: "int", nullable: false),
                    DamageReason = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    ReportedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FlowerDamageLogs", x => x.Id);
                    table.CheckConstraint("CK_FlowerDamageLog_DamageQuantity_Positive", "[DamageQuantity] > 0");
                    table.CheckConstraint("CK_FlowerDamageLogs_CreatedAt_NotFuture", "[CreatedAt] <= DATEADD(minute, 5, GETUTCDATE())");
                    table.CheckConstraint("CK_FlowerDamageLogs_DeletedAt_AfterCreated", "[DeletedAt] IS NULL OR [DeletedAt] >= [CreatedAt]");
                    table.CheckConstraint("CK_FlowerDamageLogs_DeletedAt_RequiredWhenDeleted", "[IsDeleted] = 0 OR ([IsDeleted] = 1 AND [DeletedAt] IS NOT NULL)");
                    table.CheckConstraint("CK_FlowerDamageLogs_Id_NotEmpty", "[Id] != '00000000-0000-0000-0000-000000000000'");
                    table.CheckConstraint("CK_FlowerDamageLogs_UpdatedAt_AfterCreated", "[UpdatedAt] IS NULL OR [UpdatedAt] >= [CreatedAt]");
                    table.ForeignKey(
                        name: "FK_FlowerDamageLogs_FlowerBatchDetails_FlowerBatchDetailId",
                        column: x => x.FlowerBatchDetailId,
                        principalTable: "FlowerBatchDetails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductFlowerBatchUsages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FlowerBatchDetailId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuantityUsed = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductFlowerBatchUsages", x => x.Id);
                    table.CheckConstraint("CK_ProductFlowerBatchUsage_QuantityUsed_Positive", "[QuantityUsed] > 0");
                    table.CheckConstraint("CK_ProductFlowerBatchUsages_CreatedAt_NotFuture", "[CreatedAt] <= DATEADD(minute, 5, GETUTCDATE())");
                    table.CheckConstraint("CK_ProductFlowerBatchUsages_DeletedAt_AfterCreated", "[DeletedAt] IS NULL OR [DeletedAt] >= [CreatedAt]");
                    table.CheckConstraint("CK_ProductFlowerBatchUsages_DeletedAt_RequiredWhenDeleted", "[IsDeleted] = 0 OR ([IsDeleted] = 1 AND [DeletedAt] IS NOT NULL)");
                    table.CheckConstraint("CK_ProductFlowerBatchUsages_Id_NotEmpty", "[Id] != '00000000-0000-0000-0000-000000000000'");
                    table.CheckConstraint("CK_ProductFlowerBatchUsages_UpdatedAt_AfterCreated", "[UpdatedAt] IS NULL OR [UpdatedAt] >= [CreatedAt]");
                    table.ForeignKey(
                        name: "FK_ProductFlowerBatchUsages_FlowerBatchDetails_FlowerBatchDetailId",
                        column: x => x.FlowerBatchDetailId,
                        principalTable: "FlowerBatchDetails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductFlowerBatchUsages_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "Id", "CreatedAt", "DeletedAt", "Description", "IsDeleted", "PermissionName", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("09d9935a-ce59-40b0-9f6f-2afd43363a7d"), new DateTime(2025, 8, 13, 3, 3, 58, 745, DateTimeKind.Utc).AddTicks(2032), null, "Delete flowers", false, "DELETE_FLOWERS", null },
                    { new Guid("0bf05e4e-e28d-4307-8869-964c15792623"), new DateTime(2025, 8, 13, 3, 3, 58, 745, DateTimeKind.Utc).AddTicks(1899), null, "Delete users", false, "DELETE_USERS", null },
                    { new Guid("28189da3-2188-49d5-8fc7-4e505ad5267c"), new DateTime(2025, 8, 13, 3, 3, 58, 745, DateTimeKind.Utc).AddTicks(1951), null, "Edit orders", false, "EDIT_ORDERS", null },
                    { new Guid("41e3f332-a767-418c-92f4-89257dc2cc9f"), new DateTime(2025, 8, 13, 3, 3, 58, 745, DateTimeKind.Utc).AddTicks(2046), null, "View reports", false, "VIEW_REPORTS", null },
                    { new Guid("4d5d3e97-dc40-452f-9121-2b538ff458b3"), new DateTime(2025, 8, 13, 3, 3, 58, 745, DateTimeKind.Utc).AddTicks(1883), null, "Edit users", false, "EDIT_USERS", null },
                    { new Guid("4e2cb3c1-aa4d-4d7b-9a6f-bb0b1be00194"), new DateTime(2025, 8, 13, 3, 3, 58, 745, DateTimeKind.Utc).AddTicks(1915), null, "View orders", false, "VIEW_ORDERS", null },
                    { new Guid("65f4a85c-b5ea-44cf-a2f3-46c3c1a83535"), new DateTime(2025, 8, 13, 3, 3, 58, 745, DateTimeKind.Utc).AddTicks(1966), null, "Delete orders", false, "DELETE_ORDERS", null },
                    { new Guid("74f91d4e-1fdc-4984-81a8-fa08910c58b0"), new DateTime(2025, 8, 13, 3, 3, 58, 745, DateTimeKind.Utc).AddTicks(2017), null, "Edit flowers", false, "EDIT_FLOWERS", null },
                    { new Guid("9519a84c-d78e-4570-9ed3-698f02bc8d59"), new DateTime(2025, 8, 13, 3, 3, 58, 745, DateTimeKind.Utc).AddTicks(1981), null, "View flowers", false, "VIEW_FLOWERS", null },
                    { new Guid("ae0877e4-8fc8-4aa8-8308-788a5871b7b1"), new DateTime(2025, 8, 13, 3, 3, 58, 745, DateTimeKind.Utc).AddTicks(1576), null, "View users", false, "VIEW_USERS", null },
                    { new Guid("c2410227-e503-4393-8374-bab63d753903"), new DateTime(2025, 8, 13, 3, 3, 58, 745, DateTimeKind.Utc).AddTicks(1936), null, "Create orders", false, "CREATE_ORDERS", null },
                    { new Guid("dbaada46-49c8-472a-8acc-8eeb30b29856"), new DateTime(2025, 8, 13, 3, 3, 58, 745, DateTimeKind.Utc).AddTicks(2001), null, "Create flowers", false, "CREATE_FLOWERS", null },
                    { new Guid("f33c7396-3179-4952-822d-38f447948e87"), new DateTime(2025, 8, 13, 3, 3, 58, 745, DateTimeKind.Utc).AddTicks(1863), null, "Create users", false, "CREATE_USERS", null }
                });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "CreatedAt", "DeletedAt", "Description", "IsDeleted", "RoleName", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("5d5f1ab9-e880-44cf-ac62-e731e6fdffdf"), new DateTime(2025, 8, 13, 3, 3, 58, 744, DateTimeKind.Utc).AddTicks(1850), null, "System Administrator", false, "Admin", null },
                    { new Guid("8e295f13-51b1-43c1-b911-fbc6e6ca9fb3"), new DateTime(2025, 8, 13, 3, 3, 58, 744, DateTimeKind.Utc).AddTicks(2134), null, "Staff Member", false, "Staff", null },
                    { new Guid("d7967856-9d44-487a-ab3c-c593e53de227"), new DateTime(2025, 8, 13, 3, 3, 58, 744, DateTimeKind.Utc).AddTicks(2132), null, "Regular User", false, "User", null }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Address", "CreatedAt", "DeletedAt", "Email", "FullName", "IsDeleted", "PasswordHash", "Phone", "RoleId", "UpdatedAt", "UserName" },
                values: new object[] { new Guid("37b8d4a8-db98-4486-b1d9-afefe3b534b7"), null, new DateTime(2025, 8, 13, 3, 3, 58, 745, DateTimeKind.Utc).AddTicks(4412), null, "admin@flowershop.com", "System Administrator", false, "AQAAAAEAACcQAAAAEJ5G8J5G8J5G8J5G8J5G8J5G8J5G8J5G8J5G8J5G8J5G8J5G8J5G8J5G8J5G8J5G8J5G8J5G8J5G8J5G8=", null, new Guid("5d5f1ab9-e880-44cf-ac62-e731e6fdffdf"), null, "admin" });

            migrationBuilder.CreateIndex(
                name: "IX_Deliveries_OrderId",
                table: "Deliveries",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_FlowerBatchDetails_FlowerBatchId",
                table: "FlowerBatchDetails",
                column: "FlowerBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_FlowerBatchDetails_FlowerId",
                table: "FlowerBatchDetails",
                column: "FlowerId");

            migrationBuilder.CreateIndex(
                name: "IX_FlowerBatches_FlowerId",
                table: "FlowerBatches",
                column: "FlowerId");

            migrationBuilder.CreateIndex(
                name: "IX_FlowerBatches_SupplierId",
                table: "FlowerBatches",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_FlowerDamageLogs_FlowerBatchDetailId",
                table: "FlowerDamageLogs",
                column: "FlowerBatchDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_Flowers_Name",
                table: "Flowers",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Flowers_SupplierId",
                table: "Flowers",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderDetails_FlowerId",
                table: "OrderDetails",
                column: "FlowerId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderDetails_OrderId",
                table: "OrderDetails",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderDetails_ProductId",
                table: "OrderDetails",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_CreatedAt",
                table: "Orders",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_UserId",
                table: "Orders",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_OrderId",
                table: "Payments",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_PermissionName",
                table: "Permissions",
                column: "PermissionName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductFlowerBatchUsages_FlowerBatchDetailId",
                table: "ProductFlowerBatchUsages",
                column: "FlowerBatchDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductFlowerBatchUsages_ProductId",
                table: "ProductFlowerBatchUsages",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductFlowers_FlowerId",
                table: "ProductFlowers",
                column: "FlowerId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductFlowers_ProductId_FlowerId",
                table: "ProductFlowers",
                columns: new[] { "ProductId", "FlowerId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_PermissionId",
                table: "RolePermissions",
                column: "PermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_RoleId_PermissionId",
                table: "RolePermissions",
                columns: new[] { "RoleId", "PermissionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Roles_RoleName",
                table: "Roles",
                column: "RoleName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SystemLogs_CreatedAt",
                table: "SystemLogs",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_SystemLogs_RecordId",
                table: "SystemLogs",
                column: "RecordId");

            migrationBuilder.CreateIndex(
                name: "IX_SystemLogs_UserId",
                table: "SystemLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true,
                filter: "[Email] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Users_RoleId",
                table: "Users",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_UserName",
                table: "Users",
                column: "UserName",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Deliveries");

            migrationBuilder.DropTable(
                name: "FlowerDamageLogs");

            migrationBuilder.DropTable(
                name: "OrderDetails");

            migrationBuilder.DropTable(
                name: "Payments");

            migrationBuilder.DropTable(
                name: "ProductFlowerBatchUsages");

            migrationBuilder.DropTable(
                name: "ProductFlowers");

            migrationBuilder.DropTable(
                name: "RolePermissions");

            migrationBuilder.DropTable(
                name: "SystemLogs");

            migrationBuilder.DropTable(
                name: "FlowerBatchDetails");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "Permissions");

            migrationBuilder.DropTable(
                name: "Orders");

            migrationBuilder.DropTable(
                name: "FlowerBatches");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Flowers");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "Suppliers");
        }
    }
}
