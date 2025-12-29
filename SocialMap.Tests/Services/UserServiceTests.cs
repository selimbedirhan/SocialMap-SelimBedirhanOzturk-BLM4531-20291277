using FluentAssertions;
using Moq;
using SocialMap.Business.Services;
using SocialMap.Core.Entities;
using SocialMap.Core.Interfaces;
using Xunit;

namespace SocialMap.Tests.Services;

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly UserService _userService;

    public UserServiceTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _userService = new UserService(_userRepositoryMock.Object);
    }

    [Fact]
    public async Task CreateUserAsync_ShouldThrowException_WhenUsernameExists()
    {
        // Arrange
        var username = "existinguser";
        _userRepositoryMock.Setup(x => x.UsernameExistsAsync(username)).ReturnsAsync(true);

        // Act
        Func<Task> act = async () => await _userService.CreateUserAsync(username, "test@test.com", "password");

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"Username '{username}' already exists.");
    }

    [Fact]
    public async Task CreateUserAsync_ShouldReturnUser_WhenDataIsValid()
    {
        // Arrange
        var username = "newuser";
        var email = "new@test.com";
        var password = "password123";

        _userRepositoryMock.Setup(x => x.UsernameExistsAsync(username)).ReturnsAsync(false);
        _userRepositoryMock.Setup(x => x.EmailExistsAsync(email)).ReturnsAsync(false);
        _userRepositoryMock.Setup(x => x.AddAsync(It.IsAny<User>()))
            .ReturnsAsync((User u) => u);

        // Act
        var result = await _userService.CreateUserAsync(username, email, password);

        // Assert
        result.Should().NotBeNull();
        result.Username.Should().Be(username);
        result.Email.Should().Be(email);
        result.PasswordHash.Should().NotBeNullOrEmpty();
        result.PasswordHash.Should().StartWith("$2"); // BCrypt prefix
    }

    [Fact]
    public async Task ValidateUserAsync_ShouldReturnUser_WhenPasswordIsCorrect()
    {
        // Arrange
        var username = "validuser";
        var password = "password123";
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);
        var user = new User { Username = username, PasswordHash = passwordHash };

        _userRepositoryMock.Setup(x => x.GetByUsernameAsync(username)).ReturnsAsync(user);

        // Act
        var result = await _userService.ValidateUserAsync(username, password);

        // Assert
        result.Should().NotBeNull();
        result!.Username.Should().Be(username);
    }

    [Fact]
    public async Task ValidateUserAsync_ShouldReturnNull_WhenPasswordIsIncorrect()
    {
        // Arrange
        var username = "validuser";
        var password = "password123";
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);
        var user = new User { Username = username, PasswordHash = passwordHash };

        _userRepositoryMock.Setup(x => x.GetByUsernameAsync(username)).ReturnsAsync(user);

        // Act
        var result = await _userService.ValidateUserAsync(username, "wrongpassword");

        // Assert
        result.Should().BeNull();
    }
}
