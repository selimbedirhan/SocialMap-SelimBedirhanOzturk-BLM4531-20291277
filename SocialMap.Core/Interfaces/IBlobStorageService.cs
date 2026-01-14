namespace SocialMap.Core.Interfaces;

/// <summary>
/// Blob Storage servis arayüzü - Azure veya local storage için abstraction
/// </summary>
public interface IBlobStorageService
{
    /// <summary>
    /// Dosyayı blob storage'a yükler
    /// </summary>
    /// <param name="fileStream">Dosya içeriği</param>
    /// <param name="fileName">Benzersiz dosya adı</param>
    /// <param name="contentType">MIME type (örn: image/jpeg)</param>
    /// <returns>Yüklenen dosyanın URL'i</returns>
    Task<string> UploadAsync(Stream fileStream, string fileName, string contentType);

    /// <summary>
    /// Dosyayı blob storage'dan siler
    /// </summary>
    /// <param name="blobUrl">Silinecek dosyanın URL'i</param>
    Task DeleteAsync(string blobUrl);
}
