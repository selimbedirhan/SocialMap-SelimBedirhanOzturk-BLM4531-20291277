using FluentAssertions;
using Moq;
using SocialMap.Business.Services;
using SocialMap.Core.Entities;
using SocialMap.Core.Interfaces;
using Xunit;

namespace SocialMap.Tests.Services;

public class PostServiceTests
{
    private readonly Mock<IPostRepository> _postRepositoryMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IPlaceRepository> _placeRepositoryMock;
    private readonly Mock<IFollowRepository> _followRepositoryMock;
    private readonly Mock<INotificationService> _notificationServiceMock;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly PostService _postService;

    public PostServiceTests()
    {
        _postRepositoryMock = new Mock<IPostRepository>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _placeRepositoryMock = new Mock<IPlaceRepository>();
        _followRepositoryMock = new Mock<IFollowRepository>();
        _notificationServiceMock = new Mock<INotificationService>();
        _cacheServiceMock = new Mock<ICacheService>();

        _postService = new PostService(
            _postRepositoryMock.Object,
            _userRepositoryMock.Object,
            _placeRepositoryMock.Object,
            _followRepositoryMock.Object,
            _notificationServiceMock.Object,
            _cacheServiceMock.Object
        );
    }

    [Fact]
    public async Task CreatePostAsync_ShouldInvalidateCache_WhenSuccessful()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, Username = "testuser" };
        
        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync(user);
        _postRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Post>()))
            .ReturnsAsync((Post p) => { p.Id = Guid.NewGuid(); return p; });
        _followRepositoryMock.Setup(x => x.GetFollowersAsync(userId)).ReturnsAsync(new List<Follow>());

        // Act
        // Manuel yer girisi ile post (ankara, 1.0, 1.0)
        await _postService.CreatePostAsync(userId, null, "Ankara", 39.9, 32.8, "Ankara", "Turkey", "url", "caption");

        // Assert
        _cacheServiceMock.Verify(x => x.RemoveAsync("recent_posts_20"), Times.Once);
    }

    [Fact]
    public async Task CreatePostAsync_ShouldThrow_WhenNoLocationProvided()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User { Id = userId };
        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync(user);

        // Act
        Func<Task> act = async () => await _postService.CreatePostAsync(userId, null, null, null, null, null, null, null, null);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Yer etiketi zorunludur*");
    }

    [Fact]
    public async Task GetRecentPostsAsync_ShouldReturnFromCache_WhenCacheExists()
    {
        // Arrange
        var cachedPosts = new List<Post> { new Post { Caption = "Cached Post" } };
        _cacheServiceMock.Setup(x => x.GetAsync<IEnumerable<Post>>("recent_posts_20"))
            .ReturnsAsync(cachedPosts);

        // Act
        var result = await _postService.GetRecentPostsAsync(20);

        // Assert
        result.Should().BeEquivalentTo(cachedPosts);
        _postRepositoryMock.Verify(x => x.GetRecentPostsAsync(It.IsAny<int>()), Times.Never);
    }
}
