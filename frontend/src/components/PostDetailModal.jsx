import { useState, useEffect } from 'react';
import { api } from '../services/api';
import { X, Pencil, Trash2, Heart, MessageCircle, Send } from 'lucide-react';

export default function PostDetailModal({ post, user, onClose, onUserClick, onLike }) {
  const [comments, setComments] = useState([]);
  const [newComment, setNewComment] = useState('');
  const [isLiked, setIsLiked] = useState(false);
  const [loading, setLoading] = useState(true);
  const [postingComment, setPostingComment] = useState(false);

  // Editing state
  const [isEditing, setIsEditing] = useState(false);
  const [editCaption, setEditCaption] = useState(post?.caption || '');

  // Likers List state
  const [showLikers, setShowLikers] = useState(false);
  const [likers, setLikers] = useState([]);
  const [loadingLikers, setLoadingLikers] = useState(false);

  useEffect(() => {
    if (post) {
      loadComments();
      checkLikeStatus();
      setEditCaption(post.caption || '');
    }
  }, [post, user]);

  // Helper functions
  const loadComments = async () => {
    try {
      setLoading(true);
      const data = await api.getCommentsByPost(post.id);
      setComments(Array.isArray(data) ? data : []);
    } catch (err) {
      console.error('Yorumlar yüklenemedi:', err);
      setComments([]);
    } finally {
      setLoading(false);
    }
  };

  const checkLikeStatus = async () => {
    if (user?.id) {
      try {
        const liked = await api.isLiked(post.id, user.id);
        setIsLiked(liked);
      } catch (err) {
        console.error('Beğeni durumu kontrol edilemedi:', err);
      }
    }
  };

  const handleLike = async () => {
    if (!user) return;
    try {
      if (isLiked) {
        await api.removeLike(post.id, user.id);
        setIsLiked(false);
        if (post.likesCount > 0) post.likesCount--;
      } else {
        await api.addLike(post.id, user.id);
        setIsLiked(true);
        post.likesCount = (post.likesCount || 0) + 1;
      }
      if (onLike) onLike(post.id, !isLiked);
    } catch (err) {
      console.error('Beğeni işlemi başarısız:', err);
    }
  };

  const handleShowLikers = async () => {
    if ((post.likesCount || 0) === 0) return;
    setShowLikers(true);
    setLoadingLikers(true);
    try {
      const data = await api.getLikesByPost(post.id);
      setLikers(Array.isArray(data) ? data : []);
    } catch (err) {
      console.error('Beğeniler yüklenemedi:', err);
    } finally {
      setLoadingLikers(false);
    }
  };

  const handleAddComment = async (e) => {
    e.preventDefault();
    if (!newComment.trim() || !user) return;

    try {
      setPostingComment(true);
      await api.createComment({
        postId: post.id,
        userId: user.id,
        text: newComment
      });
      setNewComment('');
      loadComments();
    } catch (err) {
      console.error('Yorum yapılamadı:', err);
      alert('Yorum gönderilemedi.');
    } finally {
      setPostingComment(false);
    }
  };

  const handleUpdate = async () => {
    try {
      if (!editCaption.trim()) return;
      await api.updatePost(post.id, { caption: editCaption });
      setIsEditing(false);
      post.caption = editCaption;
    } catch (err) {
      console.error('Update failed:', err);
      alert('Güncelleme başarısız oldu.');
    }
  };

  const handleDelete = async () => {
    if (!window.confirm('Bu gönderiyi silmek istediğinize emin misiniz?')) return;
    try {
      await api.deletePost(post.id);
      onClose();
      if (onUserClick && onUserClick.name === 'handleUserClick') {
        window.location.reload();
      } else {
        window.location.reload();
      }
    } catch (err) {
      console.error('Delete failed:', err);
      alert('Silme işlemi başarısız oldu.');
    }
  };

  // Avatar helper
  const renderAvatar = (url, username, size = 40, fontSize = 18) => {
    if (url) {
      const fullUrl = url.startsWith('http') ? url : `http://localhost:5280${url}`;
      return (
        <img
          src={fullUrl}
          alt={username}
          style={{
            width: `${size}px`,
            height: `${size}px`,
            borderRadius: '50%',
            objectFit: 'cover',
            border: '2px solid #333'
          }}
        />
      );
    }
    return (
      <div
        style={{
          width: `${size}px`,
          height: `${size}px`,
          borderRadius: '50%',
          background: 'linear-gradient(135deg, #FF6B6B, #4ECDC4)',
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center',
          fontSize: `${fontSize}px`,
          fontWeight: 'bold',
          color: 'white',
          boxShadow: '0 4px 6px rgba(0,0,0,0.2)',
          border: '2px solid #333'
        }}
      >
        {username?.[0]?.toUpperCase() || 'U'}
      </div>
    );
  };

  return (
    <div
      style={{
        position: 'fixed',
        top: 0,
        left: 0,
        right: 0,
        bottom: 0,
        background: 'rgba(0, 0, 0, 0.85)',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        zIndex: 1000,
        padding: '20px',
        backdropFilter: 'blur(5px)'
      }}
      onClick={onClose}
    >
      <div
        style={{
          background: '#1a1a1a',
          borderRadius: '16px',
          maxWidth: '900px',
          width: '100%',
          maxHeight: '90vh',
          overflow: 'hidden',
          position: 'relative',
          display: 'flex',
          flexDirection: 'column',
          boxShadow: '0 25px 50px -12px rgba(0, 0, 0, 0.5)',
          border: '1px solid #333',
          color: '#e0e0e0'
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
            background: 'rgba(50, 50, 50, 0.8)',
            color: '#fff',
            border: 'none',
            borderRadius: '50%',
            width: '36px',
            height: '36px',
            cursor: 'pointer',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            zIndex: 20,
            transition: 'all 0.2s',
            backdropFilter: 'blur(4px)'
          }}
          onMouseEnter={(e) => e.currentTarget.style.transform = 'scale(1.1)'}
          onMouseLeave={(e) => e.currentTarget.style.transform = 'scale(1)'}
        >
          <X size={20} />
        </button>

        <div style={{ display: 'flex', flexDirection: 'column', height: '100%', overflowY: 'auto' }}>

          <div style={{ padding: '24px', borderBottom: '1px solid #333', position: 'relative' }}>
            {/* Edit/Delete Buttons for Owner */}
            {user?.id === post.userId && (
              <div style={{ position: 'absolute', top: '15px', right: '60px', display: 'flex', gap: '10px', zIndex: 10 }}>
                <button
                  onClick={() => setIsEditing(!isEditing)}
                  style={{
                    background: isEditing ? 'rgba(52, 152, 219, 0.2)' : 'rgba(50, 50, 50, 0.5)',
                    color: isEditing ? '#3498db' : '#ccc',
                    border: 'none',
                    cursor: 'pointer',
                    padding: '8px',
                    borderRadius: '50%',
                    transition: 'all 0.2s',
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'center'
                  }}
                  title={isEditing ? "Düzenlemeyi Bitir" : "Düzenle"}
                >
                  <Pencil size={18} />
                </button>
                <button
                  onClick={handleDelete}
                  style={{
                    background: 'rgba(50, 50, 50, 0.5)',
                    color: '#e74c3c',
                    border: 'none',
                    cursor: 'pointer',
                    padding: '8px',
                    borderRadius: '50%',
                    transition: 'all 0.2s',
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'center'
                  }}
                  title="Sil"
                >
                  <Trash2 size={18} />
                </button>
              </div>
            )}

            <div style={{ display: 'flex', alignItems: 'center', gap: '15px', marginBottom: '15px' }}>
              <div
                onClick={() => onUserClick && onUserClick(post.userId)}
                style={{ cursor: 'pointer' }}
              >
                {renderAvatar(post.userProfilePhotoUrl, post.username, 50, 20)}
              </div>
              <div style={{ flex: 1 }}>
                <div
                  style={{
                    fontWeight: '600',
                    cursor: 'pointer',
                    fontSize: '17px',
                    color: '#fff'
                  }}
                  onClick={() => onUserClick && onUserClick(post.userId)}
                >
                  @{post.username}
                </div>
                <div style={{ fontSize: '13px', color: '#888', marginTop: '4px', display: 'flex', alignItems: 'center', gap: '4px' }}>
                  <span>📍</span> {post.placeLocation || post.placeName}
                </div>
              </div>
            </div>

            {/* Caption or Edit Form */}
            {isEditing ? (
              <div style={{ marginTop: '20px', background: '#252525', borderRadius: '12px', padding: '15px', border: '1px solid #444' }}>
                <textarea
                  value={editCaption}
                  onChange={(e) => setEditCaption(e.target.value)}
                  placeholder="Gönderiniz için yeni bir açıklama yazın..."
                  style={{
                    width: '100%',
                    padding: '12px',
                    borderRadius: '8px',
                    border: '1px solid #555',
                    minHeight: '100px',
                    fontFamily: 'inherit',
                    fontSize: '15px',
                    resize: 'vertical',
                    outline: 'none',
                    background: '#1a1a1a',
                    color: '#fff',
                    lineHeight: '1.5'
                  }}
                />
                <div style={{ marginTop: '15px', display: 'flex', justifyContent: 'flex-end', gap: '10px' }}>
                  <button
                    onClick={() => setIsEditing(false)}
                    style={{
                      padding: '8px 20px',
                      borderRadius: '8px',
                      border: '1px solid #555',
                      background: 'transparent',
                      color: '#ccc',
                      cursor: 'pointer',
                      fontWeight: '500',
                      fontSize: '14px',
                      transition: 'all 0.2s'
                    }}
                    onMouseEnter={(e) => e.currentTarget.style.background = '#333'}
                    onMouseLeave={(e) => e.currentTarget.style.background = 'transparent'}
                  >
                    İptal
                  </button>
                  <button
                    onClick={handleUpdate}
                    style={{
                      padding: '8px 24px',
                      borderRadius: '8px',
                      border: 'none',
                      background: '#3498db',
                      color: 'white',
                      cursor: 'pointer',
                      fontWeight: '600',
                      fontSize: '14px',
                      boxShadow: '0 4px 6px rgba(52, 152, 219, 0.2)'
                    }}
                  >
                    Kaydet
                  </button>
                </div>
              </div>
            ) : (
              post.caption && (
                <div style={{ marginTop: '15px', color: '#ddd', lineHeight: '1.6', fontSize: '15px', whiteSpace: 'pre-wrap' }}>
                  {post.caption}
                </div>
              )
            )}
          </div>

          {/* Post Image */}
          {
            post.mediaUrl && (
              <div style={{ width: '100%', background: '#000', borderBottom: '1px solid #333' }}>
                <img
                  src={`http://localhost:5280${post.mediaUrl}`}
                  alt={post.caption}
                  style={{
                    width: '100%',
                    maxHeight: '600px',
                    objectFit: 'contain',
                    display: 'block',
                  }}
                />
              </div>
            )
          }

          {/* Post Actions */}
          <div style={{ padding: '15px 24px', borderBottom: '1px solid #333' }}>
            <div style={{ display: 'flex', alignItems: 'center', gap: '20px' }}>
              <button
                className={`like-btn ${isLiked ? 'liked' : ''}`}
                onClick={handleLike}
                disabled={!user}
                style={{
                  background: 'none',
                  border: 'none',
                  cursor: user ? 'pointer' : 'not-allowed',
                  fontSize: '24px',
                  padding: '5px',
                  display: 'flex',
                  alignItems: 'center',
                  color: isLiked ? '#e74c3c' : '#ccc',
                  transition: 'transform 0.1s'
                }}
              >
                <Heart size={26} fill={isLiked ? "#e74c3c" : "none"} strokeWidth={isLiked ? 0 : 2} />
              </button>
              <button
                style={{
                  background: 'none',
                  border: 'none',
                  cursor: 'pointer',
                  padding: '5px',
                  display: 'flex',
                  alignItems: 'center',
                  color: '#ccc'
                }}
              >
                <MessageCircle size={26} />
              </button>
            </div>
            <div
              style={{ marginTop: '8px', color: '#888', fontSize: '14px', fontWeight: '500', cursor: 'pointer' }}
              onClick={handleShowLikers}
            >
              <span className="hover-underline">{post.likesCount || 0} beğeni</span> • {post.commentsCount || comments.length} yorum
            </div>
          </div>

          {/* Comments Section */}
          <div style={{ padding: '24px', flex: 1, overflowY: 'auto' }}>
            <h3 style={{ marginBottom: '20px', fontSize: '16px', color: '#888', textTransform: 'uppercase', letterSpacing: '1px' }}>Yorumlar</h3>

            {loading ? (
              <div style={{ textAlign: 'center', padding: '20px', color: '#666' }}>
                Yorumlar yükleniyor...
              </div>
            ) : comments.length === 0 ? (
              <div style={{ textAlign: 'center', padding: '30px', color: '#666', fontStyle: 'italic' }}>
                Henüz yorum yok. İlk yorumu sen yap!
              </div>
            ) : (
              <div style={{ display: 'flex', flexDirection: 'column', gap: '16px' }}>
                {comments.map((comment) => (
                  <div key={comment.id} className="comment" style={{ display: 'flex', gap: '12px' }}>
                    <div
                      style={{ cursor: 'pointer' }}
                      onClick={() => onUserClick && onUserClick(comment.userId)}
                    >
                      {renderAvatar(comment.userProfilePhotoUrl, comment.username, 36, 14)}
                    </div>
                    <div>
                      <div style={{ display: 'flex', alignItems: 'baseline', gap: '8px' }}>
                        <span
                          style={{ cursor: 'pointer', fontWeight: 'bold', color: '#fff', fontSize: '14px' }}
                          onClick={() => onUserClick && onUserClick(comment.userId)}
                        >
                          @{comment.username}
                        </span>
                        <span style={{ fontSize: '12px', color: '#666' }}>
                          {new Date(comment.createdAt).toLocaleDateString('tr-TR')}
                        </span>
                      </div>
                      <div style={{ color: '#ccc', fontSize: '14px', marginTop: '2px', lineHeight: '1.4' }}>{comment.text}</div>
                    </div>
                  </div>
                ))}
              </div>
            )}

            {/* Add Comment Form */}
            {user && (
              <form onSubmit={handleAddComment} style={{ marginTop: '30px', display: 'flex', gap: '12px', alignItems: 'center', position: 'sticky', bottom: 0, background: '#1a1a1a', paddingBottom: '10px' }}>
                <input
                  type="text"
                  value={newComment}
                  onChange={(e) => setNewComment(e.target.value)}
                  placeholder="Bir yorum yaz..."
                  style={{
                    flex: 1,
                    padding: '12px 20px',
                    background: '#2d2d2d',
                    border: '1px solid #444',
                    borderRadius: '24px',
                    fontSize: '14px',
                    color: '#fff',
                    outline: 'none'
                  }}
                  onFocus={(e) => e.target.style.borderColor = '#666'}
                  onBlur={(e) => e.target.style.borderColor = '#444'}
                  disabled={postingComment}
                />
                <button
                  type="submit"
                  disabled={!newComment.trim() || postingComment}
                  style={{
                    background: !newComment.trim() ? '#333' : '#3498db',
                    color: !newComment.trim() ? '#666' : 'white',
                    border: 'none',
                    borderRadius: '50%',
                    width: '42px',
                    height: '42px',
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'center',
                    cursor: !newComment.trim() ? 'default' : 'pointer',
                    transition: 'all 0.2s',
                    flexShrink: 0
                  }}
                >
                  <Send size={18} style={{ marginLeft: !newComment.trim() ? 0 : '2px' }} />
                </button>
              </form>
            )}
          </div>
        </div>
      </div>

      {/* Likers Modal Overlay */}
      {showLikers && (
        <div
          style={{
            position: 'absolute',
            top: 0,
            left: 0,
            right: 0,
            bottom: 0,
            background: 'rgba(0,0,0,0.6)',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            zIndex: 1100, // Higher than main modal
            backdropFilter: 'blur(2px)'
          }}
          onClick={(e) => {
            e.stopPropagation();
            setShowLikers(false);
          }}
        >
          <div
            style={{
              background: '#222',
              borderRadius: '12px',
              width: '400px',
              maxHeight: '400px',
              display: 'flex',
              flexDirection: 'column',
              boxShadow: '0 10px 30px rgba(0,0,0,0.5)',
              border: '1px solid #444',
              color: '#fff'
            }}
            onClick={(e) => e.stopPropagation()}
          >
            <div style={{ padding: '15px', borderBottom: '1px solid #333', display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
              <h3 style={{ margin: 0, fontSize: '16px' }}>Beğeniler</h3>
              <button
                onClick={() => setShowLikers(false)}
                style={{ background: 'none', border: 'none', color: '#ccc', cursor: 'pointer', display: 'flex' }}
              >
                <X size={20} />
              </button>
            </div>
            <div style={{ overflowY: 'auto', padding: '15px' }}>
              {loadingLikers ? (
                <div style={{ textAlign: 'center', color: '#888' }}>Yükleniyor...</div>
              ) : likers.length === 0 ? (
                <div style={{ textAlign: 'center', color: '#888' }}>Henüz beğeni yok.</div>
              ) : (
                <div style={{ display: 'flex', flexDirection: 'column', gap: '15px' }}>
                  {likers.map(liker => (
                    <div key={liker.userId} style={{ display: 'flex', alignItems: 'center', gap: '12px' }}>
                      {renderAvatar(liker.userProfilePhotoUrl, liker.username, 40, 16)}
                      <div style={{ fontWeight: '600' }}>@{liker.username}</div>
                    </div>
                  ))}
                </div>
              )}
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
