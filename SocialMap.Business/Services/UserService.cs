using SocialMap.Core.Entities;
using SocialMap.Core.Interfaces;

namespace SocialMap.Business.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<User?> GetUserByIdAsync(Guid id)
    {
        return await _userRepository.GetByIdAsync(id);
    }

    public async Task<User?> GetUserByUsernameAsync(string username)
    {
        return await _userRepository.GetByUsernameAsync(username);
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await _userRepository.GetByEmailAsync(email);
    }

    public async Task<IEnumerable<User>> GetAllUsersAsync()
    {
        return await _userRepository.GetAllAsync();
    }

    public async Task<User> CreateUserAsync(string username, string email, string password)
    {
        if (await UsernameExistsAsync(username))
            throw new InvalidOperationException($"Username '{username}' already exists.");

        if (await EmailExistsAsync(email))
            throw new InvalidOperationException($"Email '{email}' already exists.");

        // Şifreyi BCrypt ile hashle
        string passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = username,
            Email = email,
            PasswordHash = passwordHash,
            CreatedAt = DateTime.UtcNow
        };

        return await _userRepository.AddAsync(user);
    }

    public async Task UpdateUserAsync(User user)
    {
        var existingUser = await _userRepository.GetByIdAsync(user.Id);
        if (existingUser == null)
            throw new InvalidOperationException($"User with ID '{user.Id}' not found.");

        await _userRepository.UpdateAsync(user);
    }

    public async Task DeleteUserAsync(Guid id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
            throw new InvalidOperationException($"User with ID '{id}' not found.");

        await _userRepository.DeleteAsync(user);
    }

    public async Task<bool> UsernameExistsAsync(string username)
    {
        return await _userRepository.UsernameExistsAsync(username);
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        return await _userRepository.EmailExistsAsync(email);
    }

    public async Task<IEnumerable<User>> SearchUsersAsync(string searchTerm)
    {
        return await _userRepository.FindAsync(u => 
            u.Username.Contains(searchTerm) || 
            u.Email.Contains(searchTerm));
    }

    public async Task<User?> ValidateUserAsync(string username, string password)
    {
        var user = await _userRepository.GetByUsernameAsync(username);
        if (user == null)
            return null;

        // 1. Önce BCrypt ile doğrulamayı dene (yeni ve güncellenmiş kullanıcılar için)
        // BCrypt hashi genellikle $2a$ veya benzeri bir prefix ile başlar
        if (user.PasswordHash.StartsWith("$2"))
        {
            if (BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                return user;
            return null;
        }

        // 2. Eğer BCrypt değilse, eski SHA256 hash'ini dene (mevcut kullanıcılar için)
        // Lazy Migration: Eğer eski hash tutuyorsa, kullanıcıyı yeni sisteme geçir
        var legacyHash = HashPasswordLegacy(password);
        if (user.PasswordHash == legacyHash)
        {
            // Kullanıcıyı güncelle: Şifreyi BCrypt ile hashle ve kaydet
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
            await _userRepository.UpdateAsync(user);
            return user;
        }

        return null;
    }

    private static string HashPasswordLegacy(string password)
    {
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var bytes = System.Text.Encoding.UTF8.GetBytes(password);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }
}

