using Microsoft.AspNetCore.Mvc;
using SocialMap.Business.Services;
using SocialMap.Core.Interfaces;

namespace SocialMap.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FileUploadController : ControllerBase
{
    private readonly IWebHostEnvironment _environment;
    private readonly IBlobStorageService _blobStorageService;
    private readonly ILogger<FileUploadController> _logger;
    private const string UploadFolder = "uploads";

    public FileUploadController(
        IWebHostEnvironment environment, 
        IBlobStorageService blobStorageService,
        ILogger<FileUploadController> logger)
    {
        _environment = environment;
        _blobStorageService = blobStorageService;
        _logger = logger;
    }

    [HttpPost("image")]
    public async Task<ActionResult<string>> UploadImage(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("Dosya yüklenmedi.");

        // Dosya tipi kontrolü
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
        
        if (!allowedExtensions.Contains(fileExtension))
            return BadRequest("Sadece resim dosyaları yüklenebilir.");

        // Dosya boyutu kontrolü (5MB)
        if (file.Length > 5 * 1024 * 1024)
            return BadRequest("Dosya boyutu 5MB'dan büyük olamaz.");

        // Benzersiz dosya adı oluştur
        var fileName = $"{Guid.NewGuid()}{fileExtension}";
        
        // Cloudinary yapılandırılmışsa Cloudinary'e yükle
        if (_blobStorageService is CloudinaryStorageService cloudinaryService && cloudinaryService.IsConfigured)
        {
            try
            {
                using var stream = file.OpenReadStream();
                var cloudinaryUrl = await _blobStorageService.UploadAsync(stream, fileName, file.ContentType);
                _logger.LogInformation("File uploaded to Cloudinary: {Url}", cloudinaryUrl);
                return Ok(new { url = cloudinaryUrl });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Cloudinary upload failed, falling back to local storage");
                // Cloudinary yüklemesi başarısız olursa local storage'a düş
            }
        }

        // Local storage'a yükle (fallback)
        var uploadPath = Path.Combine(_environment.ContentRootPath, UploadFolder);
        if (!Directory.Exists(uploadPath))
            Directory.CreateDirectory(uploadPath);

        var filePath = Path.Combine(uploadPath, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var fileUrl = $"/{UploadFolder}/{fileName}";
        _logger.LogInformation("File uploaded to local storage: {Url}", fileUrl);
        return Ok(new { url = fileUrl });
    }

    [HttpDelete("image/{fileName}")]
    public async Task<IActionResult> DeleteImage(string fileName)
    {
        // Cloudinary'den silmeyi dene
        if (_blobStorageService is CloudinaryStorageService cloudinaryService && cloudinaryService.IsConfigured)
        {
            try
            {
                await _blobStorageService.DeleteAsync(fileName);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Cloudinary delete failed, trying local storage");
            }
        }

        // Local storage'dan sil
        var uploadPath = Path.Combine(_environment.ContentRootPath, UploadFolder, fileName);
        
        if (System.IO.File.Exists(uploadPath))
        {
            System.IO.File.Delete(uploadPath);
            return NoContent();
        }

        return NotFound();
    }
}
