using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using IPVault.API.Data;
using IPVault.API.Helpers;
using IPVault.API.Interfaces;
using IPVault.API.Middleware;
using IPVault.API.Repositories;
using IPVault.API.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. Database Configuration
var provider = builder.Configuration["DatabaseProvider"] ?? "Sqlite";
if (provider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
{
    var sqlConn = builder.Configuration.GetConnectionString("DefaultConnection");
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(sqlConn));
}
else
{
    var sqliteConn = builder.Configuration.GetConnectionString("SqliteConnection") ?? "Data Source=ipvault.db";
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlite(sqliteConn));
}

// 2. Dependency Injection Registration

// Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IClientRepository, ClientRepository>();
builder.Services.AddScoped<IInventorRepository, InventorRepository>();
builder.Services.AddScoped<IPatentStatusRepository, PatentStatusRepository>();
builder.Services.AddScoped<IPatentRepository, PatentRepository>();
builder.Services.AddScoped<IPatentDocumentRepository, PatentDocumentRepository>();

// Helpers & Utilities
builder.Services.AddScoped<IJwtHelper, JwtHelper>();

// Business Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IClientService, ClientService>();
builder.Services.AddScoped<IInventorService, InventorService>();
builder.Services.AddScoped<IPatentStatusService, PatentStatusService>();
builder.Services.AddScoped<IPatentService, PatentService>();
builder.Services.AddScoped<IDocumentService, DocumentService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();

// 3. Authentication & JWT Configuration
var jwtKey = builder.Configuration["Jwt:Key"] ?? "IPVault_Super_Secret_Key_For_Jwt_Authentication_2026_Key!";
var issuer = builder.Configuration["Jwt:Issuer"] ?? "IPVault.API";
var audience = builder.Configuration["Jwt:Audience"] ?? "IPVault.Client";

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
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = issuer,
        ValidAudience = audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("AttorneyOrAdmin", policy => policy.RequireRole("Admin", "Attorney"));
});

// 4. CORS Configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:4200", "http://localhost:3000", "http://127.0.0.1:4200")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// 5. Controllers & Model State Configuration
builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

            var response = new IPVault.API.DTOs.Common.ApiResponse<object>
            {
                Success = false,
                Message = "One or more validation errors occurred.",
                StatusCode = StatusCodes.Status400BadRequest,
                Errors = errors
            };

            return new Microsoft.AspNetCore.Mvc.BadRequestObjectResult(response);
        };
    });

// 6. Swagger / OpenAPI with JWT Support
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "IPVault API — Intellectual Property & Patent Management",
        Version = "v1",
        Description = "RESTful API for managing Clients, Inventors, Patents, Patent Statuses, and Legal Documents."
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// 7. Seed Database on Startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        await DbInitializer.SeedAsync(context);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

// 8. HTTP Pipeline Configuration
app.UseMiddleware<ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "IPVault API v1");
        c.RoutePrefix = string.Empty; // Serve Swagger at root URL
    });
}

app.UseStaticFiles();

// Serve uploads folder as static files if accessed
var uploadsPath = Path.Combine(app.Environment.ContentRootPath, "uploads");
if (!Directory.Exists(uploadsPath))
{
    Directory.CreateDirectory(uploadsPath);
}

app.UseCors("CorsPolicy");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

// For test project integration
public partial class Program { }
