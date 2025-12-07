using SocialMap.Core.Entities;
using SocialMap.Core.Interfaces;

namespace SocialMap.Business.Services;

public class CommentService : ICommentService
{
    private readonly ICommentRepository _commentRepository;
    private readonly IPostRepository _postRepository;
    private readonly IUserRepository _userRepository;
    private readonly INotificationService _notificationService;
    private readonly IFollowRepository _followRepository;

    public CommentService(ICommentRepository commentRepository, IPostRepository postRepository, IUserRepository userRepository, INotificationService notificationService, IFollowRepository followRepository)
    {
        _commentRepository = commentRepository;
        _postRepository = postRepository;
        _userRepository = userRepository;
        _notificationService = notificationService;
        _followRepository = followRepository;
    }

    public async Task<Comment?> GetCommentByIdAsync(Guid id)
    {
        return await _commentRepository.GetByIdAsync(id);
    }

    public async Task<IEnumerable<Comment>> GetCommentsByPostIdAsync(Guid postId)
    {
        return await _commentRepository.GetByPostIdAsync(postId);
    }

    public async Task<IEnumerable<Comment>> GetCommentsByUserIdAsync(Guid userId)
    {
        return await _commentRepository.GetByUserIdAsync(userId);
    }

    public async Task<IEnumerable<Comment>> GetRepliesByCommentIdAsync(Guid parentCommentId)
    {
        return await _commentRepository.GetRepliesByParentIdAsync(parentCommentId);
    }

    public async Task<Comment> CreateCommentAsync(Guid postId, Guid userId, string text, Guid? parentCommentId = null)
    {
        var post = await _postRepository.GetByIdAsync(postId);
        if (post == null)
            throw new InvalidOperationException($"Post with ID '{postId}' not found.");

        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new InvalidOperationException($"User with ID '{userId}' not found.");

        if (parentCommentId.HasValue)
        {
            var parentComment = await _commentRepository.GetByIdAsync(parentCommentId.Value);
            if (parentComment == null)
                throw new InvalidOperationException($"Parent comment with ID '{parentCommentId.Value}' not found.");
        }

        var comment = new Comment
        {
            Id = Guid.NewGuid(),
            PostId = postId,
            UserId = userId,
            Text = text,
            ParentCommentId = parentCommentId,
            CreatedAt = DateTime.UtcNow
        };

        var addedComment = await _commentRepository.AddAsync(comment);

        // Post'un CommentsCount'unu güncelle
        // Önce post'u yeniden yükle (Comments collection'ı güncel olsun)
        var updatedPost = await _postRepository.GetByIdAsync(postId);
        if (updatedPost != null)
        {
            updatedPost.CommentsCount = updatedPost.Comments?.Count ?? 0;
            await _postRepository.UpdateAsync(updatedPost);
        }

        // Bildirim oluştur (post sahibine veya yorum sahibine)
        if (parentCommentId.HasValue)
        {
            var parentComment = await _commentRepository.GetByIdAsync(parentCommentId.Value);
            if (parentComment != null && parentComment.UserId != userId)
            {
                await _notificationService.CreateNotificationAsync(
                    parentComment.UserId,
                    "comment",
                    $"{user.Username} yorumunuza yanıt verdi",
                    relatedPostId: postId,
                    relatedUserId: userId
                );
            }
        }
        else
        {
            // Post sahibine bildirim gönder
            if (post.UserId != userId)
            {
                await _notificationService.CreateNotificationAsync(
                    post.UserId,
                    "comment",
                    $"{user.Username} gönderinize yorum yaptı",
                    relatedPostId: postId,
                    relatedUserId: userId
                );
            }

            // Post sahibinin takip edenlerine bildirim gönder (yorum yapan ve post sahibi hariç)
            var postOwner = await _userRepository.GetByIdAsync(post.UserId);
            var postOwnerFollowers = await _followRepository.GetFollowersAsync(post.UserId);
            foreach (var follow in postOwnerFollowers)
            {
                if (follow.FollowerId != userId && follow.FollowerId != post.UserId)
                {
                    await _notificationService.CreateNotificationAsync(
                        follow.FollowerId,
                        "comment",
                        $"{user.Username}, {postOwner?.Username ?? "bir kullanıcının"} gönderisine yorum yaptı",
                        relatedPostId: postId,
                        relatedUserId: userId
                    );
                }
            }
        }

        return addedComment;
    }

    public async Task UpdateCommentAsync(Comment comment)
    {
        var existingComment = await _commentRepository.GetByIdAsync(comment.Id);
        if (existingComment == null)
            throw new InvalidOperationException($"Comment with ID '{comment.Id}' not found.");

        await _commentRepository.UpdateAsync(comment);
    }

    public async Task DeleteCommentAsync(Guid id)
    {
        var comment = await _commentRepository.GetByIdAsync(id);
        if (comment == null)
            throw new InvalidOperationException($"Comment with ID '{id}' not found.");

        var post = await _postRepository.GetByIdAsync(comment.PostId);
        
        await _commentRepository.DeleteAsync(comment);

        // Post'un CommentsCount'unu güncelle
        if (post != null)
        {
            post.CommentsCount = Math.Max(0, post.CommentsCount - 1);
            await _postRepository.UpdateAsync(post);
        }
    }
}

