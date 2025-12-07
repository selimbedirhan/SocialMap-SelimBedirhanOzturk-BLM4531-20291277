using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using SocialMap.Business.Services;
using SocialMap.Core.Interfaces;
using SocialMap.Repository.Data;
using SocialMap.Repository.Repositories;
using SocialMap.WebAPI.Hubs;
using SocialMap.WebAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddControllers();
builder.Services.AddSignalR();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IPlaceRepository, PlaceRepository>();
builder.Services.AddScoped<IPostRepository, PostRepository>();
builder.Services.AddScoped<ICommentRepository, CommentRepository>();
builder.Services.AddScoped<ILikeRepository, LikeRepository>();
builder.Services.AddScoped<IFollowRepository, FollowRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();

// Services
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

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.MapOpenApi();
}

// CORS
app.UseCors("AllowAll");

// Static Files (Uploaded images)
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(builder.Environment.ContentRootPath, "uploads")),
    RequestPath = "/uploads"
});

// Database Migration
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    
    // Önce EnsureCreated ile tabloları oluştur
    dbContext.Database.EnsureCreated();
    
    // Sonra yeni kolonları ekle (eğer yoksa)
    try
    {
        var migrationTask = DatabaseMigrationHelper.EnsurePostLocationColumnsExistAsync(dbContext);
        migrationTask.GetAwaiter().GetResult();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[Startup] Kolon ekleme hatası (normal olabilir): {ex.Message}");
        // Hata olsa bile devam et, belki PostgreSQL çalışmıyor veya kolonlar zaten var
    }
    
    // Not: Mevcut postların geohash'lerini güncellemek için:
    // POST /api/admin/update-post-geohashes endpoint'ini çağır
}

//app.UseHttpsRedirection();
app.MapControllers();
app.MapHub<NotificationHub>("/notificationHub");

app.Run();
