using FlowerSelling.Data.FlowerSellingWebsite.Data;
using FlowerSellingWebsite.Infrastructure.Middleware.ErrorHandlingMiddleware;
using FlowerSellingWebsite.Infrastructure.Swagger;
using FlowerSellingWebsite.Repositories.Interfaces;
using FlowerSellingWebsite.Repositories.Implementations;
using FlowerSellingWebsite.Services.Implementations;

using FlowerSellingWebsite.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ProjectGreenLens.Services.Implementations;
using ProjectGreenLens.Services.Interfaces;
using ProjectGreenLens.Settings;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

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
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IBlogRepository, BlogRepository>();
builder.Services.AddScoped<ICommentRepository, CommentRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();

// Application Services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddSingleton<IEmailVerificationService, EmailVerificationService>();
builder.Services.AddSingleton<IPasswordResetService, PasswordResetService>();
builder.Services.AddSingleton<IPendingUserService, PendingUserService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IBlogService, BlogService>();
builder.Services.AddScoped<ICommentService, CommentService>();
builder.Services.AddScoped<IFileUploadService, FileUploadService>();
builder.Services.AddScoped<IOrderService, OrderService>();

// Background Services
builder.Services.AddHostedService<EmailVerificationCleanupService>();
builder.Services.AddHostedService<PasswordResetTokenCleanupService>();

// AutoMapper configuration
builder.Services.AddAutoMapper(typeof(Program).Assembly);

// HTTP Context Accessor for current user access
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




// CORS Configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", builder =>
    {
        builder.WithOrigins("http://localhost:3000", "http://127.0.0.1:5500", "http://localhost:5500", "http://localhost:5062", "http://127.0.0.1:5062", "http://localhost:5081", "http://127.0.0.1:5081")
               .AllowAnyMethod()
               .AllowAnyHeader()
               .AllowCredentials()
               .SetIsOriginAllowed(origin => true); // Allow any origin for development
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Enhanced Swagger configuration
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
    
    // Enable XML comments
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
    
    // Add JWT Authentication support for Swagger
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

    // Add operation filter to apply security requirements only to endpoints with [Authorize] attribute
    options.OperationFilter<SecurityRequirementsOperationFilter>();
});

var app = builder.Build();


// Seed Data
/*
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<FlowerSellingDbContext>();
    await db.Database.MigrateAsync();
    SeedData.Initialize(db);
}
*/


// Middleware
app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseCors("AllowFrontend");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();



// Make Swagger available in all environments
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Flower Selling Website API v1");
    options.RoutePrefix = "swagger";
    options.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List);
    options.DefaultModelsExpandDepth(0); // Hide schemas section by default
});

app.MapControllers();


app.Run();