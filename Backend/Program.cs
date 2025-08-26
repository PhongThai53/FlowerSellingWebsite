using FlowerSelling.Data.FlowerSellingWebsite.Data;
using FlowerSellingWebsite.Infrastructure.DbContext;
using FlowerSellingWebsite.Infrastructure.Middleware.ErrorHandlingMiddleware;
using FlowerSellingWebsite.Infrastructure.Swagger;
using FlowerSellingWebsite.Infrastructure.Authorization;
using FlowerSellingWebsite.Models.DTOs.ProductCategory;
using FlowerSellingWebsite.Repositories.Implementations;
using FlowerSellingWebsite.Repositories.Interfaces;
using FlowerSellingWebsite.Services.Implementations;
using FlowerSellingWebsite.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using ProjectGreenLens.Services.Implementations;
using ProjectGreenLens.Services.Interfaces;
using ProjectGreenLens.Settings;
using System.Text;
using System.Text.Json.Serialization;
using VNPAY.NET;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    options.SerializerOptions.WriteIndented = true;
});

// Load appsettings.json
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile("secretsettings.json", optional: true, reloadOnChange: true);

// DbContext
builder.Services.AddDbContext<FlowerSellingDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// JWT Settings
var jwtSection = builder.Configuration.GetSection("JwtSettings");
builder.Services.Configure<JwtSettings>(jwtSection);
var jwtSettings = jwtSection.Get<JwtSettings>() ?? throw new InvalidOperationException("JWT settings not configured.");

// JWT Service
builder.Services.AddSingleton(jwtSettings);
builder.Services.AddScoped<IJwtService, JwtService>();

// Repository Services
builder.Services.AddScoped(typeof(IBaseRepository<>), typeof(BaseRepository<>));
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IBlogRepository, BlogRepository>();
builder.Services.AddScoped<ICommentRepository, CommentRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IContactService, ContactService>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductCategoryRepository, ProductCategoryRepository>();
builder.Services.AddScoped<IProductPhotoRepository, ProductPhotoRepository>();
builder.Services.AddScoped<ICartRepository, CartRepository>();
builder.Services.AddScoped<IImageRepository, ImageRepository>();
builder.Services.AddScoped<IProductFlowersRepository, ProductFlowersRepository>();
builder.Services.AddScoped<ISupplierListingsRepository, SupplierListingsRepository>();
builder.Services.AddScoped<IPurchaseOrdersRepository, PurchaseOrdersRepository>();
builder.Services.AddScoped<IPurchaseOrderDetailsRepository, PurchaseOrderDetailsRepository>();

// Application Services
builder.Services.AddScoped<
    IBaseService<ProductCategoryCreateDTO, ProductCategoryUpdateDTO, ProductCategoryResponseDTO>,
    ProductCategoryService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddSingleton<IEmailVerificationService, EmailVerificationService>();
builder.Services.AddSingleton<IPasswordResetService, PasswordResetService>();
builder.Services.AddSingleton<IPendingUserService, PendingUserService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IBlogService, BlogService>();
builder.Services.AddScoped<ICommentService, CommentService>();
builder.Services.AddScoped<IFileUploadService, FileUploadService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IProductCategoryService, ProductCategoryService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IFlowerService, FlowerService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IImageService, ImageService>();
builder.Services.AddScoped<ISupplierListingService, SupplierListingService>();

builder.Services.AddScoped<IVnpay, Vnpay>();
builder.Services.AddScoped<IVNPayService, VNPayService>();

builder.Services.AddScoped<IImageService, ImageService>();

// Application Services
// Background Services
builder.Services.AddHostedService<EmailVerificationCleanupService>();
builder.Services.AddHostedService<PasswordResetTokenCleanupService>();

// AutoMapper
builder.Services.AddAutoMapper(typeof(Program).Assembly);

// HTTP Context Accessor
builder.Services.AddHttpContextAccessor();

// Authentication
var key = Encoding.ASCII.GetBytes(jwtSettings.Key);
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidateAudience = true,
        ValidAudience = jwtSettings.Audience,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", builder =>
    {
        builder.WithOrigins(
            "http://localhost:3000",
            "http://127.0.0.1:5500",
            "http://localhost:5500",
            "http://localhost:5062",
            "http://127.0.0.1:5062",
            "http://localhost:5081",
            "http://127.0.0.1:5081")
               .AllowAnyMethod()
               .AllowAnyHeader()
               .AllowCredentials()
               .SetIsOriginAllowed(origin => true); // development
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Swagger
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Flower Selling Website API",
        Version = "v1",
        Description = "API for managing flower categories, products, and orders",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "Admin",
            Email = "admin@flowershop.com"
        }
    });

    // XML comments
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }

    // JWT support
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = Microsoft.OpenApi.Models.ParameterLocation.Header
            },
            new List<string>()
        }
    });

    options.OperationFilter<SecurityRequirementsOperationFilter>();
});

var app = builder.Build();

// Serve Static files
// Default wwwroot folder
app.UseStaticFiles();

// Custom static files for Image folder
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads")),
    RequestPath = "/uploads"
});

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Flower Selling Website API v1");
    options.RoutePrefix = "swagger";
    options.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List);
    options.DefaultModelsExpandDepth(0);
});

app.UseCors("AllowFrontend");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// Error handling for API only
app.UseWhen(context => context.Request.Path.StartsWithSegments("/api"), appBuilder =>
{
    appBuilder.UseMiddleware<ErrorHandlingMiddleware>();
});

app.MapControllers();

//Optional: Seed Data
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<FlowerSellingDbContext>();
    await db.Database.MigrateAsync();
    SeedData.Initialize(db);
}

app.Run();
