using FluentAssertions;
using Moq;
using SocialMap.Business.Services;
using SocialMap.Core.Entities;
using SocialMap.Core.Interfaces;
using Xunit;

namespace SocialMap.Tests.Services;

public class FollowServiceTests
{
    private readonly Mock<IFollowRepository> _followRepositoryMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<INotificationService> _notificationServiceMock;
    private readonly FollowService _followService;

    public FollowServiceTests()
    {
        _followRepositoryMock = new Mock<IFollowRepository>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _notificationServiceMock = new Mock<INotificationService>();

        _followService = new FollowService(
            _followRepositoryMock.Object,
            _userRepositoryMock.Object,
            _notificationServiceMock.Object
        );
    }

    [Fact]
    public async Task FollowUserAsync_ShouldCreateFollow_WhenValid()
    {
        // Arrange
        var followerId = Guid.NewGuid();
        var followingId = Guid.NewGuid();
        
        var follower = new User { Id = followerId, Username = "follower" };
        var following = new User { Id = followingId, Username = "following" };

        _userRepositoryMock.Setup(x => x.GetByIdAsync(followerId)).ReturnsAsync(follower);
        _userRepositoryMock.Setup(x => x.GetByIdAsync(followingId)).ReturnsAsync(following);
        _followRepositoryMock.Setup(x => x.GetFollowAsync(followerId, followingId)).ReturnsAsync((Follow?)null);
        _followRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Follow>()))
            .ReturnsAsync((Follow f) => { f.Id = Guid.NewGuid(); return f; });

        // Act
        var result = await _followService.FollowUserAsync(followerId, followingId);

        // Assert
        result.Should().NotBeNull();
        _followRepositoryMock.Verify(x => x.AddAsync(It.Is<Follow>(f => 
            f.FollowerId == followerId && f.FollowingId == followingId)), Times.Once);
    }

    [Fact]
    public async Task FollowUserAsync_ShouldThrow_WhenAlreadyFollowing()
    {
        // Arrange
        var followerId = Guid.NewGuid();
        var followingId = Guid.NewGuid();
        var existingFollow = new Follow { FollowerId = followerId, FollowingId = followingId };
        
        var follower = new User { Id = followerId };
        var following = new User { Id = followingId };

        _userRepositoryMock.Setup(x => x.GetByIdAsync(followerId)).ReturnsAsync(follower);
        _userRepositoryMock.Setup(x => x.GetByIdAsync(followingId)).ReturnsAsync(following);
        _followRepositoryMock.Setup(x => x.GetFollowAsync(followerId, followingId)).ReturnsAsync(existingFollow);

        // Act
        Func<Task> act = async () => await _followService.FollowUserAsync(followerId, followingId);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*takip ediyorsunuz*");
    }

    [Fact]
    public async Task FollowUserAsync_ShouldThrow_WhenFollowingSelf()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        Func<Task> act = async () => await _followService.FollowUserAsync(userId, userId);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Kendinizi*");
    }

    [Fact]
    public async Task UnfollowUserAsync_ShouldRemoveFollow_WhenFollowing()
    {
        // Arrange
        var followerId = Guid.NewGuid();
        var followingId = Guid.NewGuid();
        var existingFollow = new Follow { Id = Guid.NewGuid(), FollowerId = followerId, FollowingId = followingId };

        _followRepositoryMock.Setup(x => x.GetFollowAsync(followerId, followingId))
            .ReturnsAsync(existingFollow);

        // Act
        await _followService.UnfollowUserAsync(followerId, followingId);

        // Assert
        _followRepositoryMock.Verify(x => x.DeleteAsync(existingFollow), Times.Once);
    }

    [Fact]
    public async Task IsFollowingAsync_ShouldReturnTrue_WhenFollowing()
    {
        // Arrange
        var followerId = Guid.NewGuid();
        var followingId = Guid.NewGuid();

        _followRepositoryMock.Setup(x => x.IsFollowingAsync(followerId, followingId)).ReturnsAsync(true);

        // Act
        var result = await _followService.IsFollowingAsync(followerId, followingId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task GetFollowerCountAsync_ShouldReturnCount()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _followRepositoryMock.Setup(x => x.GetFollowerCountAsync(userId)).ReturnsAsync(5);

        // Act
        var result = await _followService.GetFollowerCountAsync(userId);

        // Assert
        result.Should().Be(5);
    }

    [Fact]
    public async Task GetFollowingCountAsync_ShouldReturnCount()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _followRepositoryMock.Setup(x => x.GetFollowingCountAsync(userId)).ReturnsAsync(10);

        // Act
        var result = await _followService.GetFollowingCountAsync(userId);

        // Assert
        result.Should().Be(10);
    }
}
