using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SocialMap.Core.Interfaces;

namespace SocialMap.Business.Services;

/// <summary>
/// Cloudinary implementasyonu - Ücretsiz 25GB depolama
/// </summary>
public class CloudinaryStorageService : IBlobStorageService
{
    private readonly Cloudinary? _cloudinary;
    private readonly ILogger<CloudinaryStorageService> _logger;
    private readonly bool _isConfigured;

    public CloudinaryStorageService(IConfiguration configuration, ILogger<CloudinaryStorageService> logger)
    {
        _logger = logger;
        
        var cloudName = configuration["Cloudinary:CloudName"];
        var apiKey = configuration["Cloudinary:ApiKey"];
        var apiSecret = configuration["Cloudinary:ApiSecret"];

        if (!string.IsNullOrEmpty(cloudName) && 
            !string.IsNullOrEmpty(apiKey) && 
            !string.IsNullOrEmpty(apiSecret) &&
            cloudName != "<YOUR_CLOUD_NAME>")
        {
            try
            {
                var account = new Account(cloudName, apiKey, apiSecret);
                _cloudinary = new Cloudinary(account);
                _cloudinary.Api.Secure = true;
                
                _isConfigured = true;
                _logger.LogInformation("Cloudinary configured successfully with cloud: {CloudName}", cloudName);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Cloudinary could not be configured. Will fall back to local storage.");
                _isConfigured = false;
            }
        }
        else
        {
            _logger.LogInformation("Cloudinary not configured. Using local storage fallback.");
            _isConfigured = false;
        }
    }

    public bool IsConfigured => _isConfigured;

    public async Task<string> UploadAsync(Stream fileStream, string fileName, string contentType)
    {
        if (!_isConfigured || _cloudinary == null)
        {
            throw new InvalidOperationException("Cloudinary is not configured");
        }

        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(fileName, fileStream),
            PublicId = Path.GetFileNameWithoutExtension(fileName),
            Folder = "socialmap",
            Overwrite = true
        };

        var uploadResult = await _cloudinary.UploadAsync(uploadParams);

        if (uploadResult.Error != null)
        {
            throw new Exception($"Cloudinary upload failed: {uploadResult.Error.Message}");
        }

        _logger.LogInformation("File uploaded to Cloudinary: {Url}", uploadResult.SecureUrl);
        
        return uploadResult.SecureUrl.ToString();
    }

    public async Task DeleteAsync(string imageUrl)
    {
        if (!_isConfigured || _cloudinary == null)
        {
            throw new InvalidOperationException("Cloudinary is not configured");
        }

        try
        {
            // URL'den public ID'yi çıkar
            var uri = new Uri(imageUrl);
            var segments = uri.AbsolutePath.Split('/');
            var fileName = segments.Last();
            var publicId = $"socialmap/{Path.GetFileNameWithoutExtension(fileName)}";
            
            var deleteParams = new DeletionParams(publicId);
            var result = await _cloudinary.DestroyAsync(deleteParams);
            
            _logger.LogInformation("File deleted from Cloudinary: {PublicId}, Result: {Result}", publicId, result.Result);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to delete from Cloudinary: {Url}", imageUrl);
        }
    }
}
