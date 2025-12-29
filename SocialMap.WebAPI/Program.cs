using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using AspNetCoreRateLimit;
using Serilog;
using SocialMap.Business.Services;
using SocialMap.Core.Interfaces;
using SocialMap.Repository.Data;
using SocialMap.Repository.Repositories;
using SocialMap.WebAPI.Hubs;
using SocialMap.WebAPI.Services;
using FluentValidation;
using FluentValidation.AspNetCore;
using SocialMap.Business.Mappings;
using SocialMap.WebAPI.Middleware;
using SocialMap.Business.Validators;
using SocialMap.WebAPI.Filters;

// ==================== SERILOG CONFIGURATION ====================
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", Serilog.Events.LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File("logs/socialmap-.log", 
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 7)
    .CreateLogger();

try
{
    Log.Information("SocialMap API başlatılıyor...");
    
    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseSerilog();

// ==================== JWT AUTHENTICATION ====================
var jwtKey = builder.Configuration["Jwt:Key"] ?? "YourSuperSecretKeyThatIsAtLeast32CharactersLong!";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "SocialMap";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "SocialMap";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ClockSkew = TimeSpan.Zero // Token süresini tam olarak uygula
    };

    // SignalR için JWT token desteği
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/notificationHub"))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

// ==================== RATE LIMITING ====================
builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(options =>
{
    options.EnableEndpointRateLimiting = true;
    options.StackBlockedRequests = false;
    options.HttpStatusCode = 429;
    options.RealIpHeader = "X-Real-IP";
    options.ClientIdHeader = "X-ClientId";
    options.GeneralRules = new List<RateLimitRule>
    {
        // Genel limit: dakikada 100 istek
        new RateLimitRule
        {
            Endpoint = "*",
            Period = "1m",
            Limit = 100
        },
        // Login/Register için özel limit: dakikada 10 istek
        new RateLimitRule
        {
            Endpoint = "*:/api/auth/*",
            Period = "1m",
            Limit = 10
        },
        // Post oluşturma için özel limit: dakikada 5 istek
        new RateLimitRule
        {
            Endpoint = "post:/api/posts",
            Period = "1m",
            Limit = 5
        }
    };
});

builder.Services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
builder.Services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
builder.Services.AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>();
builder.Services.AddInMemoryRateLimiting();

// ==================== OPENAPI / SWAGGER ====================
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header. Örnek: 'Bearer {token}'",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// ==================== CONTROLLERS & VALIDATION ====================
builder.Services.AddControllers(options =>
{
    options.Filters.Add<ValidationFilter>();
})
.ConfigureApiBehaviorOptions(options =>
{
    options.SuppressModelStateInvalidFilter = true;
});

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();
builder.Services.AddValidatorsFromAssemblyContaining<CreateUserDtoValidator>();
builder.Services.AddAutoMapper(typeof(MappingProfile).Assembly);
builder.Services.AddScoped<ICacheService, MemoryCacheService>();
builder.Services.AddSignalR();

// ==================== CORS ====================
// Development ve Production için farklı CORS ayarları
var allowedOrigins = builder.Environment.IsDevelopment()
    ? new[] { "http://localhost:5173", "http://localhost:3000", "http://127.0.0.1:5173" }
    : new[] { "https://yourdomain.com" }; // Prodüksiyonda kendi domain'inizi ekleyin

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials(); // SignalR için gerekli
    });
});

// ==================== DATABASE ====================
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// ==================== HEALTH CHECKS ====================
builder.Services.AddHealthChecks()
    .AddNpgSql(
        builder.Configuration.GetConnectionString("DefaultConnection")!,
        name: "postgresql",
        tags: new[] { "db", "sql", "postgresql" });

// ==================== REPOSITORIES ====================
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IPlaceRepository, PlaceRepository>();
builder.Services.AddScoped<IPostRepository, PostRepository>();
builder.Services.AddScoped<ICommentRepository, CommentRepository>();
builder.Services.AddScoped<ILikeRepository, LikeRepository>();
builder.Services.AddScoped<IFollowRepository, FollowRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<IReportRepository, ReportRepository>();
builder.Services.AddScoped<IHashtagRepository, HashtagRepository>();
builder.Services.AddScoped<ISavedPostRepository, SavedPostRepository>();

// ==================== UNIT OF WORK ====================
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// ==================== SERVICES ====================
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IPlaceService, PlaceService>();
builder.Services.AddScoped<IPostService, PostService>();
builder.Services.AddScoped<ICommentService, CommentService>();
builder.Services.AddScoped<ILikeService, LikeService>();
builder.Services.AddScoped<IFollowService, FollowService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IMapService, MapService>();
builder.Services.AddScoped<PostGeohashUpdateService>();
builder.Services.AddScoped<NotificationBroadcaster>();

var app = builder.Build();

// ==================== MIDDLEWARE PIPELINE ====================

// 1. Exception Handling (En üstte olmalı)
app.UseMiddleware<ExceptionMiddleware>();

// 2. Rate Limiting
app.UseIpRateLimiting();

// 3. HTTPS Redirection (Production'da aktif)
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
    app.UseHsts();
}

// 4. CORS
app.UseCors("AllowFrontend");

// 5. Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// 6. Swagger (Development only)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.MapOpenApi();
}

// 7. Static Files (Uploaded images)
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(builder.Environment.ContentRootPath, "uploads")),
    RequestPath = "/uploads"
});

// ==================== DATABASE MIGRATION ====================
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    
    // Tabloları oluştur
    dbContext.Database.EnsureCreated();
    
    // Yeni kolonları ekle (eğer yoksa)
    try
    {
        var migrationTask = DatabaseMigrationHelper.EnsurePostLocationColumnsExistAsync(dbContext);
        migrationTask.GetAwaiter().GetResult();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[Startup] Kolon ekleme hatası (normal olabilir): {ex.Message}");
    }
}

// ==================== ENDPOINTS ====================
app.MapControllers();
app.MapHub<NotificationHub>("/notificationHub");
app.MapHealthChecks("/health");

Log.Information("SocialMap API başarıyla başlatıldı!");
app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "SocialMap API başlatılamadı!");
    throw;
}
finally
{
    Log.CloseAndFlush();
}
