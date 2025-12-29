using FluentAssertions;
using Moq;
using SocialMap.Business.Services;
using SocialMap.Core.Entities;
using SocialMap.Core.Interfaces;
using Xunit;

namespace SocialMap.Tests.Services;

public class LikeServiceTests
{
    private readonly Mock<ILikeRepository> _likeRepositoryMock;
    private readonly Mock<IPostRepository> _postRepositoryMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<INotificationService> _notificationServiceMock;
    private readonly LikeService _likeService;

    public LikeServiceTests()
    {
        _likeRepositoryMock = new Mock<ILikeRepository>();
        _postRepositoryMock = new Mock<IPostRepository>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _notificationServiceMock = new Mock<INotificationService>();

        _likeService = new LikeService(
            _likeRepositoryMock.Object,
            _postRepositoryMock.Object,
            _userRepositoryMock.Object,
            _notificationServiceMock.Object
        );
    }

    [Fact]
    public async Task AddLikeAsync_ShouldReturnLike_WhenValid()
    {
        // Arrange
        var postId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var postOwnerId = Guid.NewGuid();
        
        var post = new Post { Id = postId, UserId = postOwnerId };
        var user = new User { Id = userId, Username = "liker" };

        _postRepositoryMock.Setup(x => x.GetByIdAsync(postId)).ReturnsAsync(post);
        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync(user);
        _likeRepositoryMock.Setup(x => x.GetByPostAndUserAsync(postId, userId)).ReturnsAsync((Like?)null);
        _likeRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Like>()))
            .ReturnsAsync((Like l) => { l.Id = Guid.NewGuid(); return l; });

        // Act
        var result = await _likeService.AddLikeAsync(postId, userId);

        // Assert
        result.Should().NotBeNull();
        result.PostId.Should().Be(postId);
        result.UserId.Should().Be(userId);
    }

    [Fact]
    public async Task AddLikeAsync_ShouldThrow_WhenAlreadyLiked()
    {
        // Arrange
        var postId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        
        var post = new Post { Id = postId };
        var user = new User { Id = userId };
        var existingLike = new Like { PostId = postId, UserId = userId };
        
        _postRepositoryMock.Setup(x => x.GetByIdAsync(postId)).ReturnsAsync(post);
        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync(user);
        _likeRepositoryMock.Setup(x => x.GetByPostAndUserAsync(postId, userId)).ReturnsAsync(existingLike);

        // Act
        Func<Task> act = async () => await _likeService.AddLikeAsync(postId, userId);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*already liked*");
    }

    [Fact]
    public async Task RemoveLikeAsync_ShouldRemove_WhenLikeExists()
    {
        // Arrange
        var postId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var existingLike = new Like { Id = Guid.NewGuid(), PostId = postId, UserId = userId };
        var post = new Post { Id = postId, LikesCount = 1 };

        _likeRepositoryMock.Setup(x => x.GetByPostAndUserAsync(postId, userId)).ReturnsAsync(existingLike);
        _postRepositoryMock.Setup(x => x.GetByIdAsync(postId)).ReturnsAsync(post);

        // Act
        await _likeService.RemoveLikeAsync(postId, userId);

        // Assert
        _likeRepositoryMock.Verify(x => x.DeleteAsync(existingLike), Times.Once);
    }

    [Fact]
    public async Task IsPostLikedByUserAsync_ShouldReturnTrue_WhenLikeExists()
    {
        // Arrange
        var postId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _likeRepositoryMock.Setup(x => x.IsLikedAsync(postId, userId)).ReturnsAsync(true);

        // Act
        var result = await _likeService.IsPostLikedByUserAsync(postId, userId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsPostLikedByUserAsync_ShouldReturnFalse_WhenLikeNotExists()
    {
        // Arrange
        var postId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _likeRepositoryMock.Setup(x => x.IsLikedAsync(postId, userId)).ReturnsAsync(false);

        // Act
        var result = await _likeService.IsPostLikedByUserAsync(postId, userId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetLikesByPostIdAsync_ShouldReturnLikes()
    {
        // Arrange
        var postId = Guid.NewGuid();
        var likes = new List<Like>
        {
            new Like { Id = Guid.NewGuid(), PostId = postId },
            new Like { Id = Guid.NewGuid(), PostId = postId }
        };

        _likeRepositoryMock.Setup(x => x.GetByPostIdAsync(postId)).ReturnsAsync(likes);

        // Act
        var result = await _likeService.GetLikesByPostIdAsync(postId);

        // Assert
        result.Should().HaveCount(2);
    }
}
