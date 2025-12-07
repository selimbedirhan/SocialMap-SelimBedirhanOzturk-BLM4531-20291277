import { useState, useEffect } from 'react';
import { api } from '../services/api';

export default function PostDetailModal({ post, user, onClose, onUserClick, onLike }) {
  const [comments, setComments] = useState([]);
  const [newComment, setNewComment] = useState('');
  const [isLiked, setIsLiked] = useState(false);
  const [loading, setLoading] = useState(true);
  const [postingComment, setPostingComment] = useState(false);

  useEffect(() => {
    if (post) {
      loadComments();
      checkLikeStatus();
    }
  }, [post, user]);

  const loadComments = async () => {
    try {
      setLoading(true);
      const data = await api.getCommentsByPost(post.id);
      setComments(data);
    } catch (err) {
      console.error('Yorumlar yüklenemedi:', err);
    } finally {
      setLoading(false);
    }
  };

  const checkLikeStatus = async () => {
    if (!user?.id) return;
    try {
      const liked = await api.isLiked(post.id, user.id);
      setIsLiked(liked);
    } catch (err) {
      console.error('Like durumu kontrol edilemedi:', err);
    }
  };

  const handleLike = async () => {
    if (!user?.id) return;
    try {
      if (isLiked) {
        await api.removeLike(post.id, user.id);
        setIsLiked(false);
        if (onLike) onLike(post.id, false);
      } else {
        await api.addLike(post.id, user.id);
        setIsLiked(true);
        if (onLike) onLike(post.id, true);
      }
    } catch (err) {
      console.error('Beğeni hatası:', err);
    }
  };

  const handleAddComment = async (e) => {
    e.preventDefault();
    if (!newComment.trim() || !user?.id || postingComment) return;
    
    try {
      setPostingComment(true);
      await api.createComment({
        postId: post.id,
        userId: user.id,
        text: newComment,
      });
      setNewComment('');
      await loadComments();
    } catch (err) {
      console.error('Yorum eklenemedi:', err);
    } finally {
      setPostingComment(false);
    }
  };

  if (!post) return null;

  return (
    <div
      style={{
        position: 'fixed',
        top: 0,
        left: 0,
        right: 0,
        bottom: 0,
        background: 'rgba(0, 0, 0, 0.7)',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        zIndex: 1000,
        padding: '20px',
      }}
      onClick={onClose}
    >
      <div
        style={{
          background: 'white',
          borderRadius: '12px',
          maxWidth: '800px',
          width: '100%',
          maxHeight: '90vh',
          overflow: 'auto',
          position: 'relative',
        }}
        onClick={(e) => e.stopPropagation()}
      >
        {/* Close Button */}
        <button
          onClick={onClose}
          style={{
            position: 'absolute',
            top: '15px',
            right: '15px',
            background: 'rgba(0,0,0,0.5)',
            color: 'white',
            border: 'none',
            borderRadius: '50%',
            width: '32px',
            height: '32px',
            cursor: 'pointer',
            fontSize: '20px',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            zIndex: 10,
          }}
        >
          ×
        </button>

        <div style={{ display: 'flex', flexDirection: 'column' }}>
          {/* Post Header */}
          <div style={{ padding: '20px', borderBottom: '1px solid #e0e0e0' }}>
            <div style={{ display: 'flex', alignItems: 'center', gap: '12px', marginBottom: '10px' }}>
              <div
                style={{
                  width: '40px',
                  height: '40px',
                  borderRadius: '50%',
                  background: '#ddd',
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'center',
                  fontSize: '18px',
                  fontWeight: 'bold',
                  cursor: 'pointer',
                }}
                onClick={() => onUserClick && onUserClick(post.userId)}
              >
                {post.username?.[0]?.toUpperCase() || 'U'}
              </div>
              <div style={{ flex: 1 }}>
                <div
                  style={{
                    fontWeight: '600',
                    cursor: 'pointer',
                    fontSize: '16px',
                  }}
                  onClick={() => onUserClick && onUserClick(post.userId)}
                >
                  @{post.username}
                </div>
                <div style={{ fontSize: '14px', color: '#3498db', marginTop: '4px' }}>
                  📍 {post.placeLocation || post.placeName}
                </div>
              </div>
            </div>
            {post.caption && (
              <div style={{ marginTop: '12px', color: '#333', lineHeight: '1.6' }}>
                {post.caption}
              </div>
            )}
          </div>

          {/* Post Image */}
          {post.mediaUrl && (
            <div style={{ width: '100%', background: '#000' }}>
              <img
                src={`http://localhost:5280${post.mediaUrl}`}
                alt={post.caption}
                style={{
                  width: '100%',
                  maxHeight: '500px',
                  objectFit: 'contain',
                  display: 'block',
                }}
              />
            </div>
          )}

          {/* Post Actions */}
          <div style={{ padding: '15px 20px', borderBottom: '1px solid #e0e0e0' }}>
            <div style={{ display: 'flex', alignItems: 'center', gap: '15px' }}>
              <button
                className={`like-btn ${isLiked ? 'liked' : ''}`}
                onClick={handleLike}
                disabled={!user}
                style={{
                  background: 'none',
                  border: 'none',
                  cursor: user ? 'pointer' : 'not-allowed',
                  fontSize: '24px',
                  padding: '0',
                }}
              >
                {isLiked ? '❤️' : '🤍'}
              </button>
              <span style={{ color: '#7f8c8d', fontSize: '14px' }}>
                {post.likesCount || 0} beğeni • {post.commentsCount || comments.length} yorum
              </span>
            </div>
          </div>

          {/* Comments Section */}
          <div style={{ padding: '20px', maxHeight: '300px', overflowY: 'auto' }}>
            <h3 style={{ marginBottom: '15px', fontSize: '18px' }}>Yorumlar</h3>
            
            {loading ? (
              <div style={{ textAlign: 'center', padding: '20px', color: '#7f8c8d' }}>
                Yorumlar yükleniyor...
              </div>
            ) : comments.length === 0 ? (
              <div style={{ textAlign: 'center', padding: '20px', color: '#7f8c8d' }}>
                Henüz yorum yok. İlk yorumu sen yap!
              </div>
            ) : (
              <div style={{ display: 'flex', flexDirection: 'column', gap: '15px' }}>
                {comments.map((comment) => (
                  <div key={comment.id} className="comment">
                    <div className="comment-header">
                      <span
                        className="comment-username"
                        style={{ cursor: 'pointer' }}
                        onClick={() => onUserClick && onUserClick(comment.userId)}
                      >
                        @{comment.username}
                      </span>
                      <span style={{ fontSize: '12px', color: '#7f8c8d' }}>
                        {new Date(comment.createdAt).toLocaleDateString('tr-TR')}
                      </span>
                    </div>
                    <div className="comment-text">{comment.text}</div>
                  </div>
                ))}
              </div>
            )}

            {/* Add Comment Form */}
            {user && (
              <form onSubmit={handleAddComment} style={{ marginTop: '20px', display: 'flex', gap: '10px' }}>
                <input
                  type="text"
                  value={newComment}
                  onChange={(e) => setNewComment(e.target.value)}
                  placeholder="Yorum yaz..."
                  style={{
                    flex: 1,
                    padding: '10px',
                    border: '1px solid #ddd',
                    borderRadius: '20px',
                    fontSize: '14px',
                  }}
                  disabled={postingComment}
                />
                <button
                  type="submit"
                  className="btn btn-primary"
                  disabled={!newComment.trim() || postingComment}
                  style={{
                    padding: '10px 20px',
                    borderRadius: '20px',
                  }}
                >
                  {postingComment ? 'Gönderiliyor...' : 'Gönder'}
                </button>
              </form>
            )}
          </div>
        </div>
      </div>
    </div>
  );
}







