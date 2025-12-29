import { useState, useEffect } from 'react';
import { api } from '../services/api';
import PlaceSearch from './PlaceSearch';

export default function Home({ user, onUserClick }) {
  const [posts, setPosts] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [showForm, setShowForm] = useState(false);
  const [formData, setFormData] = useState({
    placeName: null,
    latitude: null,
    longitude: null,
    city: null,
    country: null,
    mediaUrl: '',
    caption: '',
  });
  const [uploading, setUploading] = useState(false);

  useEffect(() => {
    loadPosts();
  }, []);

  const loadPosts = async () => {
    try {
      setLoading(true);
      setError(null);
      const data = await api.getPosts(100); // Tüm gönderileri al
      setPosts(data || []);
    } catch (err) {
      console.error('Gönderiler yüklenirken hata:', err);
      setError('Gönderiler yüklenirken hata oluştu: ' + (err.message || 'Bilinmeyen hata'));
      setPosts([]);
    } finally {
      setLoading(false);
    }
  };

  const handlePlaceSelect = (place) => {
    if (place) {
      setFormData({
        ...formData,
        placeName: place.placeName,
        latitude: place.latitude,
        longitude: place.longitude,
        city: place.city,
        country: place.country,
      });
    } else {
      setFormData({
        ...formData,
        placeName: null,
        latitude: null,
        longitude: null,
        city: null,
        country: null,
      });
    }
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (!formData.placeName || !formData.latitude || !formData.longitude) {
      alert('Lütfen bir yer seçin! Yer etiketi zorunludur.');
      return;
    }
    try {
      await api.createPost({
        userId: user.id,
        placeId: null, // Artık placeId kullanmıyoruz
        placeName: formData.placeName,
        latitude: formData.latitude,
        longitude: formData.longitude,
        city: formData.city,
        country: formData.country,
        mediaUrl: formData.mediaUrl || null,
        caption: formData.caption || null,
      });
      setShowForm(false);
      setFormData({
        placeName: null,
        latitude: null,
        longitude: null,
        city: null,
        country: null,
        mediaUrl: '',
        caption: '',
      });
      loadPosts();
    } catch (err) {
      setError('Gönderi oluşturulurken hata oluştu: ' + (err.message || 'Bilinmeyen hata'));
    }
  };

  const handleLike = async (postId) => {
    try {
      const isLiked = await api.isLiked(postId, user.id);
      if (isLiked) {
        await api.removeLike(postId, user.id);
      } else {
        await api.addLike(postId, user.id);
      }
      loadPosts();
    } catch (err) {
      console.error('Like hatası:', err);
    }
  };

  if (loading) {
    return <div className="loading">Yükleniyor...</div>;
  }

  return (
    <div>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '20px' }}>
        <h2>Keşfet</h2>
        <button className="btn btn-primary" onClick={() => setShowForm(!showForm)}>
          {showForm ? 'İptal' : 'Yeni Gönderi'}
        </button>
      </div>

      {error && <div className="error">{error}</div>}

      {showForm && (
        <form className="form" onSubmit={handleSubmit} style={{ marginBottom: '30px' }}>
          <PlaceSearch onPlaceSelect={handlePlaceSelect} />
          <div className="form-group">
            <label>Fotoğraf Yükle (Opsiyonel)</label>
            <input
              type="file"
              accept="image/*"
              onChange={async (e) => {
                const file = e.target.files[0];
                if (file) {
                  try {
                    setUploading(true);
                    const result = await api.uploadImage(file);
                    setFormData({ ...formData, mediaUrl: result.url });
                  } catch (err) {
                    alert('Dosya yüklenirken hata oluştu.');
                  } finally {
                    setUploading(false);
                  }
                }
              }}
              disabled={uploading}
            />
            {formData.mediaUrl && (
              <div style={{ marginTop: '10px' }}>
                <img
                  src={`http://localhost:5280${formData.mediaUrl}`}
                  alt="Preview"
                  style={{ maxWidth: '200px', maxHeight: '200px', borderRadius: '5px' }}
                />
              </div>
            )}
            {uploading && <div style={{ marginTop: '5px', color: '#7f8c8d' }}>Yükleniyor...</div>}
          </div>
          <div className="form-group">
            <label>Açıklama (Opsiyonel)</label>
            <textarea
              value={formData.caption}
              onChange={(e) => setFormData({ ...formData, caption: e.target.value })}
              placeholder="Gönderiniz hakkında bir şeyler yazın..."
            />
          </div>
          <button type="submit" className="btn btn-primary" disabled={!formData.placeName || !formData.latitude || !formData.longitude}>
            Paylaş
          </button>
        </form>
      )}

      {posts.length === 0 ? (
        <div style={{ textAlign: 'center', padding: '40px', color: '#7f8c8d' }}>
          Henüz gönderi yok. İlk gönderiyi paylaşmak için "Yeni Gönderi" butonuna tıklayın!
        </div>
      ) : (
        <div style={{ display: 'flex', flexDirection: 'column', gap: '20px' }}>
          {posts.map((post) => (
            <PostCard key={post.id} post={post} user={user} onLike={handleLike} onUserClick={onUserClick} />
          ))}
        </div>
      )}
    </div>
  );
}

function PostCard({ post, user, onLike, onUserClick }) {
  const [isLiked, setIsLiked] = useState(false);
  const [comments, setComments] = useState([]);
  const [showComments, setShowComments] = useState(false);
  const [newComment, setNewComment] = useState('');

  useEffect(() => {
    if (user?.id) {
      checkLikeStatus();
      loadComments();
    }
  }, [post.id, user?.id]);

  const checkLikeStatus = async () => {
    try {
      const liked = await api.isLiked(post.id, user.id);
      setIsLiked(liked);
    } catch (err) {
      console.error('Like durumu kontrol edilemedi:', err);
    }
  };

  const loadComments = async () => {
    try {
      const data = await api.getCommentsByPost(post.id);
      setComments(data);
    } catch (err) {
      console.error('Yorumlar yüklenemedi:', err);
    }
  };

  const handleLike = () => {
    onLike(post.id);
    setIsLiked(!isLiked);
  };

  const handleAddComment = async (e) => {
    e.preventDefault();
    if (!newComment.trim() || !user?.id) return;
    try {
      await api.createComment({
        postId: post.id,
        userId: user.id,
        text: newComment,
      });
      setNewComment('');
      loadComments();
    } catch (err) {
      console.error('Yorum eklenemedi:', err);
    }
  };

  return (
    <div className="post-card" style={{ width: '100%', maxWidth: '600px', margin: '0 auto' }}>
      <div className="post-header">
        <div>
          <div
            className="post-username"
            style={{ cursor: 'pointer', textDecoration: 'underline' }}
            onClick={() => onUserClick && post.userId && onUserClick(post.userId)}
          >
            @{post.username}
          </div>
          <div className="post-place" style={{ fontSize: '14px', color: '#3498db', marginTop: '5px' }}>
            📍 {post.placeLocation}
          </div>
        </div>
      </div>
      {post.mediaUrl && (
        <div style={{ width: '100%', display: 'flex', justifyContent: 'center', marginBottom: '15px' }}>
          <img
            src={`http://localhost:5280${post.mediaUrl}`}
            alt={post.caption}
            className="post-image"
            style={{ width: '100%', maxWidth: '100%' }}
          />
        </div>
      )}
      {post.caption && <div className="post-caption">{post.caption}</div>}
      <div className="post-actions">
        <button
          className={`like-btn ${isLiked ? 'liked' : ''}`}
          onClick={handleLike}
        >
          {isLiked ? '❤️' : '🤍'}
        </button>
        <span className="post-stats">
          {post.likesCount} beğeni • {post.commentsCount} yorum
        </span>
      </div>
      <button
        className="btn btn-secondary"
        onClick={() => setShowComments(!showComments)}
        style={{ marginTop: '10px', width: '100%' }}
      >
        {showComments ? 'Yorumları Gizle' : 'Yorumları Göster'}
      </button>
      {showComments && (
        <div className="comments-section">
          {comments.map((comment) => (
            <div key={comment.id} className="comment">
              <div className="comment-header">
                <span className="comment-username">@{comment.username}</span>
              </div>
              <div className="comment-text">{comment.text}</div>
            </div>
          ))}
          {user && (
            <form onSubmit={handleAddComment} style={{ marginTop: '15px' }}>
              <div className="form-group">
                <textarea
                  value={newComment}
                  onChange={(e) => setNewComment(e.target.value)}
                  placeholder="Yorumunuzu yazın..."
                  required
                  style={{ marginBottom: '10px' }}
                />
                <button type="submit" className="btn btn-primary">Yorum Ekle</button>
              </div>
            </form>
          )}
        </div>
      )}
    </div>
  );
}

