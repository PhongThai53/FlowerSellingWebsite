using FlowerSelling.Data;
using FlowerSellingWebsite.Infrastructure.Middleware.ErrorHandlingMiddleware;
using FlowerSellingWebsite.Repositories.Interfaces;
using FlowerSellingWebsite.Repositories.Implementations;
using FlowerSellingWebsite.Services.Interfaces;
using FlowerSellingWebsite.Services.Implementations;
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

// Application Services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddSingleton<IEmailVerificationService, EmailVerificationService>();
builder.Services.AddSingleton<IPendingUserService, PendingUserService>();

// Background Services
builder.Services.AddHostedService<EmailVerificationCleanupService>();

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
builder.Services.AddSwaggerGen();

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

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();


app.Run();
