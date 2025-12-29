using FluentAssertions;
using Moq;
using SocialMap.Business.Services;
using SocialMap.Core.Entities;
using SocialMap.Core.Interfaces;
using Xunit;

namespace SocialMap.Tests.Services;

public class CommentServiceTests
{
    private readonly Mock<ICommentRepository> _commentRepositoryMock;
    private readonly Mock<IPostRepository> _postRepositoryMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<INotificationService> _notificationServiceMock;
    private readonly Mock<IFollowRepository> _followRepositoryMock;
    private readonly CommentService _commentService;

    public CommentServiceTests()
    {
        _commentRepositoryMock = new Mock<ICommentRepository>();
        _postRepositoryMock = new Mock<IPostRepository>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _notificationServiceMock = new Mock<INotificationService>();
        _followRepositoryMock = new Mock<IFollowRepository>();

        _commentService = new CommentService(
            _commentRepositoryMock.Object,
            _postRepositoryMock.Object,
            _userRepositoryMock.Object,
            _notificationServiceMock.Object,
            _followRepositoryMock.Object
        );
    }

    [Fact]
    public async Task CreateCommentAsync_ShouldReturnComment_WhenValid()
    {
        // Arrange
        var postId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var postOwnerId = Guid.NewGuid();
        var text = "Great post!";
        
        var post = new Post { Id = postId, UserId = postOwnerId };
        var user = new User { Id = userId, Username = "commenter" };

        _postRepositoryMock.Setup(x => x.GetByIdAsync(postId)).ReturnsAsync(post);
        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync(user);
        _followRepositoryMock.Setup(x => x.GetFollowersAsync(postOwnerId)).ReturnsAsync(new List<Follow>());
        _commentRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Comment>()))
            .ReturnsAsync((Comment c) => { c.Id = Guid.NewGuid(); return c; });

        // Act
        var result = await _commentService.CreateCommentAsync(postId, userId, text, null);

        // Assert
        result.Should().NotBeNull();
        result.Text.Should().Be(text);
        result.PostId.Should().Be(postId);
        result.UserId.Should().Be(userId);
    }

    [Fact]
    public async Task CreateCommentAsync_ShouldThrow_WhenPostNotFound()
    {
        // Arrange
        var postId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        
        _postRepositoryMock.Setup(x => x.GetByIdAsync(postId)).ReturnsAsync((Post?)null);

        // Act
        Func<Task> act = async () => await _commentService.CreateCommentAsync(postId, userId, "text", null);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not found*");
    }

    [Fact]
    public async Task GetCommentsByPostIdAsync_ShouldReturnComments()
    {
        // Arrange
        var postId = Guid.NewGuid();
        var comments = new List<Comment>
        {
            new Comment { Id = Guid.NewGuid(), PostId = postId, Text = "Comment 1" },
            new Comment { Id = Guid.NewGuid(), PostId = postId, Text = "Comment 2" }
        };

        _commentRepositoryMock.Setup(x => x.GetByPostIdAsync(postId)).ReturnsAsync(comments);

        // Act
        var result = await _commentService.GetCommentsByPostIdAsync(postId);

        // Assert
        result.Should().HaveCount(2);
        result.Should().BeEquivalentTo(comments);
    }
}
