using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FlowerSellingWebsite.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FlowerCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoryName = table.Column<string>(type: "nvarchar(100)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", nullable: true),
                    PublicId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FlowerCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FlowerColors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ColorName = table.Column<string>(type: "nvarchar(100)", nullable: false),
                    HexCode = table.Column<string>(type: "nvarchar(10)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", nullable: true),
                    PublicId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FlowerColors", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FlowerTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TypeName = table.Column<string>(type: "nvarchar(100)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", nullable: true),
                    PublicId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FlowerTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PaymentMethods",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MethodName = table.Column<string>(type: "nvarchar(100)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    PublicId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentMethods", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Permissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PermissionName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    PublicId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permissions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", nullable: true),
                    Category = table.Column<string>(type: "nvarchar(100)", nullable: true),
                    PublicId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    PublicId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Suppliers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SupplierName = table.Column<string>(type: "nvarchar(200)", nullable: false),
                    ContactPerson = table.Column<string>(type: "nvarchar(200)", nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(20)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(200)", nullable: true),
                    Address = table.Column<string>(type: "nvarchar(500)", nullable: true),
                    PublicId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Suppliers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FlowerCategoryImages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FlowerCategoryId = table.Column<int>(type: "int", nullable: true),
                    FlowerTypeId = table.Column<int>(type: "int", nullable: true),
                    FlowerColorId = table.Column<int>(type: "int", nullable: true),
                    ImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ImageType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    IsPrimary = table.Column<bool>(type: "bit", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    EffectiveDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    PublicId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FlowerCategoryImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FlowerCategoryImages_FlowerCategories_FlowerCategoryId",
                        column: x => x.FlowerCategoryId,
                        principalTable: "FlowerCategories",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_FlowerCategoryImages_FlowerColors_FlowerColorId",
                        column: x => x.FlowerColorId,
                        principalTable: "FlowerColors",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_FlowerCategoryImages_FlowerTypes_FlowerTypeId",
                        column: x => x.FlowerTypeId,
                        principalTable: "FlowerTypes",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "FlowerPricing",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FlowerCategoryId = table.Column<int>(type: "int", nullable: true),
                    FlowerTypeId = table.Column<int>(type: "int", nullable: true),
                    FlowerColorId = table.Column<int>(type: "int", nullable: true),
                    Price = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    EffectiveDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PriceType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    PublicId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FlowerPricing", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FlowerPricing_FlowerCategories_FlowerCategoryId",
                        column: x => x.FlowerCategoryId,
                        principalTable: "FlowerCategories",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_FlowerPricing_FlowerColors_FlowerColorId",
                        column: x => x.FlowerColorId,
                        principalTable: "FlowerColors",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_FlowerPricing_FlowerTypes_FlowerTypeId",
                        column: x => x.FlowerTypeId,
                        principalTable: "FlowerTypes",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Flowers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", nullable: true),
                    FlowerCategoryId = table.Column<int>(type: "int", nullable: true),
                    FlowerTypeId = table.Column<int>(type: "int", nullable: false),
                    FlowerColorId = table.Column<int>(type: "int", nullable: false),
                    Size = table.Column<string>(type: "nvarchar(50)", nullable: true),
                    ShelfLifeDays = table.Column<int>(type: "int", nullable: false),
                    PublicId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Flowers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Flowers_FlowerCategories_FlowerCategoryId",
                        column: x => x.FlowerCategoryId,
                        principalTable: "FlowerCategories",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Flowers_FlowerColors_FlowerColorId",
                        column: x => x.FlowerColorId,
                        principalTable: "FlowerColors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Flowers_FlowerTypes_FlowerTypeId",
                        column: x => x.FlowerTypeId,
                        principalTable: "FlowerTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductPhotos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    Url = table.Column<string>(type: "nvarchar(500)", nullable: false),
                    IsPrimary = table.Column<bool>(type: "bit", nullable: false),
                    PublicId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductPhotos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductPhotos_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RolePermissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    PermissionId = table.Column<int>(type: "int", nullable: false),
                    PublicId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolePermissions", x => x.Id);
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
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserName = table.Column<string>(type: "nvarchar(100)", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(500)", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(200)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(200)", nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(20)", nullable: true),
                    Address = table.Column<string>(type: "nvarchar(500)", nullable: true),
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    PublicId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PurchaseOrders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PurchaseOrderNumber = table.Column<string>(type: "nvarchar(100)", nullable: false),
                    SupplierId = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", nullable: true),
                    PublicId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseOrders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PurchaseOrders_Suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Suppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SupplierListings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SupplierId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    AvailableQuantity = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    ShelfLifeDays = table.Column<int>(type: "int", nullable: false),
                    MinOrderQty = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", nullable: false),
                    PublicId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupplierListings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SupplierListings_Suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Suppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FlowerBatches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FlowerId = table.Column<int>(type: "int", nullable: false),
                    SupplierId = table.Column<int>(type: "int", nullable: false),
                    BatchCode = table.Column<string>(type: "nvarchar(100)", nullable: false),
                    ImportDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    QuantityAvailable = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    PublicId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FlowerBatches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FlowerBatches_Flowers_FlowerId",
                        column: x => x.FlowerId,
                        principalTable: "Flowers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FlowerBatches_Suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Suppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductFlowers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    FlowerId = table.Column<int>(type: "int", nullable: false),
                    QuantityNeeded = table.Column<int>(type: "int", nullable: false),
                    PublicId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductFlowers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductFlowers_Flowers_FlowerId",
                        column: x => x.FlowerId,
                        principalTable: "Flowers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductFlowers_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FlowerPriceHistory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FlowerPricingId = table.Column<int>(type: "int", nullable: false),
                    OldPrice = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    NewPrice = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    ChangeReason = table.Column<string>(type: "nvarchar(500)", nullable: true),
                    ChangedByUserId = table.Column<int>(type: "int", nullable: false),
                    ChangeDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PublicId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FlowerPriceHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FlowerPriceHistory_FlowerPricing_FlowerPricingId",
                        column: x => x.FlowerPricingId,
                        principalTable: "FlowerPricing",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FlowerPriceHistory_Users_ChangedByUserId",
                        column: x => x.ChangedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderNumber = table.Column<string>(type: "nvarchar(100)", nullable: false),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", nullable: false),
                    Subtotal = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    TaxAmount = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", nullable: true),
                    PublicId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Orders_Users_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PurchaseOrderDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PurchaseOrderId = table.Column<int>(type: "int", nullable: false),
                    FlowerId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    LineTotal = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    PublicId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseOrderDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PurchaseOrderDetails_Flowers_FlowerId",
                        column: x => x.FlowerId,
                        principalTable: "Flowers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PurchaseOrderDetails_PurchaseOrders_PurchaseOrderId",
                        column: x => x.PurchaseOrderId,
                        principalTable: "PurchaseOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SupplierListingPhotos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SupplierListingId = table.Column<int>(type: "int", nullable: false),
                    Url = table.Column<string>(type: "nvarchar(500)", nullable: false),
                    IsPrimary = table.Column<bool>(type: "bit", nullable: false),
                    PublicId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupplierListingPhotos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SupplierListingPhotos_SupplierListings_SupplierListingId",
                        column: x => x.SupplierListingId,
                        principalTable: "SupplierListings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FlowerDamageLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FlowerBatchId = table.Column<int>(type: "int", nullable: false),
                    ReportedByUserId = table.Column<int>(type: "int", nullable: false),
                    DamagedQuantity = table.Column<int>(type: "int", nullable: false),
                    DamageReason = table.Column<string>(type: "nvarchar(500)", nullable: false),
                    DamageDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", nullable: true),
                    PublicId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FlowerDamageLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FlowerDamageLogs_FlowerBatches_FlowerBatchId",
                        column: x => x.FlowerBatchId,
                        principalTable: "FlowerBatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Deliveries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    DeliveryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeliveryStatus = table.Column<string>(type: "nvarchar(50)", nullable: false),
                    TrackingNumber = table.Column<string>(type: "nvarchar(100)", nullable: true),
                    ShipperName = table.Column<string>(type: "nvarchar(200)", nullable: true),
                    PublicId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Deliveries", x => x.Id);
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
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Discount = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    LineTotal = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    PublicId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderDetails", x => x.Id);
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
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    PaymentMethodId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    PaymentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", nullable: false),
                    PublicId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Payments_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Payments_PaymentMethods_PaymentMethodId",
                        column: x => x.PaymentMethodId,
                        principalTable: "PaymentMethods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Deliveries_OrderId",
                table: "Deliveries",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_FlowerBatches_FlowerId",
                table: "FlowerBatches",
                column: "FlowerId");

            migrationBuilder.CreateIndex(
                name: "IX_FlowerBatches_SupplierId",
                table: "FlowerBatches",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_FlowerCategoryImages_FlowerCategoryId",
                table: "FlowerCategoryImages",
                column: "FlowerCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_FlowerCategoryImages_FlowerColorId",
                table: "FlowerCategoryImages",
                column: "FlowerColorId");

            migrationBuilder.CreateIndex(
                name: "IX_FlowerCategoryImages_FlowerTypeId",
                table: "FlowerCategoryImages",
                column: "FlowerTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_FlowerDamageLogs_FlowerBatchId",
                table: "FlowerDamageLogs",
                column: "FlowerBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_FlowerPriceHistory_ChangedByUserId",
                table: "FlowerPriceHistory",
                column: "ChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_FlowerPriceHistory_FlowerPricingId",
                table: "FlowerPriceHistory",
                column: "FlowerPricingId");

            migrationBuilder.CreateIndex(
                name: "IX_FlowerPricing_FlowerCategoryId",
                table: "FlowerPricing",
                column: "FlowerCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_FlowerPricing_FlowerColorId",
                table: "FlowerPricing",
                column: "FlowerColorId");

            migrationBuilder.CreateIndex(
                name: "IX_FlowerPricing_FlowerTypeId",
                table: "FlowerPricing",
                column: "FlowerTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Flowers_FlowerCategoryId",
                table: "Flowers",
                column: "FlowerCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Flowers_FlowerColorId",
                table: "Flowers",
                column: "FlowerColorId");

            migrationBuilder.CreateIndex(
                name: "IX_Flowers_FlowerTypeId",
                table: "Flowers",
                column: "FlowerTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderDetails_OrderId",
                table: "OrderDetails",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderDetails_ProductId",
                table: "OrderDetails",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_CustomerId",
                table: "Orders",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_OrderId",
                table: "Payments",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_PaymentMethodId",
                table: "Payments",
                column: "PaymentMethodId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductFlowers_FlowerId",
                table: "ProductFlowers",
                column: "FlowerId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductFlowers_ProductId",
                table: "ProductFlowers",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductPhotos_ProductId",
                table: "ProductPhotos",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrderDetails_FlowerId",
                table: "PurchaseOrderDetails",
                column: "FlowerId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrderDetails_PurchaseOrderId",
                table: "PurchaseOrderDetails",
                column: "PurchaseOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrders_SupplierId",
                table: "PurchaseOrders",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_PermissionId",
                table: "RolePermissions",
                column: "PermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_RoleId",
                table: "RolePermissions",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierListingPhotos_SupplierListingId",
                table: "SupplierListingPhotos",
                column: "SupplierListingId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierListings_SupplierId",
                table: "SupplierListings",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_RoleId",
                table: "Users",
                column: "RoleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Deliveries");

            migrationBuilder.DropTable(
                name: "FlowerCategoryImages");

            migrationBuilder.DropTable(
                name: "FlowerDamageLogs");

            migrationBuilder.DropTable(
                name: "FlowerPriceHistory");

            migrationBuilder.DropTable(
                name: "OrderDetails");

            migrationBuilder.DropTable(
                name: "Payments");

            migrationBuilder.DropTable(
                name: "ProductFlowers");

            migrationBuilder.DropTable(
                name: "ProductPhotos");

            migrationBuilder.DropTable(
                name: "PurchaseOrderDetails");

            migrationBuilder.DropTable(
                name: "RolePermissions");

            migrationBuilder.DropTable(
                name: "SupplierListingPhotos");

            migrationBuilder.DropTable(
                name: "FlowerBatches");

            migrationBuilder.DropTable(
                name: "FlowerPricing");

            migrationBuilder.DropTable(
                name: "Orders");

            migrationBuilder.DropTable(
                name: "PaymentMethods");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "PurchaseOrders");

            migrationBuilder.DropTable(
                name: "Permissions");

            migrationBuilder.DropTable(
                name: "SupplierListings");

            migrationBuilder.DropTable(
                name: "Flowers");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Suppliers");

            migrationBuilder.DropTable(
                name: "FlowerCategories");

            migrationBuilder.DropTable(
                name: "FlowerColors");

            migrationBuilder.DropTable(
                name: "FlowerTypes");

            migrationBuilder.DropTable(
                name: "Roles");
        }
    }
}
