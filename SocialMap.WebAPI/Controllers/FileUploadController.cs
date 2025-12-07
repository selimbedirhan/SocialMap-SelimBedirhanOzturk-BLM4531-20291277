using Microsoft.AspNetCore.Mvc;

namespace SocialMap.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FileUploadController : ControllerBase
{
    private readonly IWebHostEnvironment _environment;
    private const string UploadFolder = "uploads";

    public FileUploadController(IWebHostEnvironment environment)
    {
        _environment = environment;
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

        // Upload klasörünü oluştur
        var uploadPath = Path.Combine(_environment.ContentRootPath, UploadFolder);
        if (!Directory.Exists(uploadPath))
            Directory.CreateDirectory(uploadPath);

        // Benzersiz dosya adı oluştur
        var fileName = $"{Guid.NewGuid()}{fileExtension}";
        var filePath = Path.Combine(uploadPath, fileName);

        // Dosyayı kaydet
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        // URL döndür
        var fileUrl = $"/{UploadFolder}/{fileName}";
        return Ok(new { url = fileUrl });
    }

    [HttpDelete("image/{fileName}")]
    public IActionResult DeleteImage(string fileName)
    {
        var uploadPath = Path.Combine(_environment.ContentRootPath, UploadFolder, fileName);
        
        if (System.IO.File.Exists(uploadPath))
        {
            System.IO.File.Delete(uploadPath);
            return NoContent();
        }

        return NotFound();
    }
}

