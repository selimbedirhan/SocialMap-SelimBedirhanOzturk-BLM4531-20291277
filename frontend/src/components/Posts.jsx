import { useState, useEffect } from 'react';
import { api } from '../services/api';
import PlaceSearch from './PlaceSearch';

export default function Posts() {
  const [posts, setPosts] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [showForm, setShowForm] = useState(false);
  const [formData, setFormData] = useState({
    userId: '',
    placeName: null,
    latitude: null,
    longitude: null,
    city: null,
    country: null,
    mediaUrl: '',
    caption: '',
  });
  const [uploading, setUploading] = useState(false);
  const [users, setUsers] = useState([]);

  useEffect(() => {
    loadPosts();
    loadUsers();
  }, []);

  const loadPosts = async () => {
    try {
      setLoading(true);
      const data = await api.getPosts();
      setPosts(data);
      setError(null);
    } catch (err) {
      setError('Gönderiler yüklenirken hata oluştu.');
    } finally {
      setLoading(false);
    }
  };

  const loadUsers = async () => {
    try {
      const data = await api.getUsers();
      setUsers(data);
    } catch (err) {
      console.error('Kullanıcılar yüklenemedi:', err);
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
        userId: formData.userId,
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
        userId: '',
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
      setError('Gönderi oluşturulurken hata oluştu.');
    }
  };

  const handleLike = async (postId, userId) => {
    if (!userId) {
      alert('Lütfen önce bir kullanıcı seçin!');
      return;
    }
    try {
      const isLiked = await api.isLiked(postId, userId);
      if (isLiked) {
        await api.removeLike(postId, userId);
      } else {
        await api.addLike(postId, userId);
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
        <h2>Gönderiler</h2>
        <button className="btn btn-primary" onClick={() => setShowForm(!showForm)}>
          {showForm ? 'İptal' : 'Yeni Gönderi'}
        </button>
      </div>

      {error && <div className="error">{error}</div>}

      {showForm && (
        <form className="form" onSubmit={handleSubmit} style={{ marginBottom: '30px' }}>
          <div className="form-group">
            <label>Kullanıcı</label>
            <select
              value={formData.userId}
              onChange={(e) => setFormData({ ...formData, userId: e.target.value })}
              required
            >
              <option value="">Seçiniz...</option>
              {users.map((user) => (
                <option key={user.id} value={user.id}>
                  {user.username}
                </option>
              ))}
            </select>
          </div>
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
            />
          </div>
          <button type="submit" className="btn btn-primary" disabled={!formData.placeName || !formData.latitude || !formData.longitude}>
            Oluştur
          </button>
        </form>
      )}

      <div className="posts-grid">
        {posts.map((post) => (
          <PostCard key={post.id} post={post} onLike={handleLike} />
        ))}
      </div>
    </div>
  );
}

function PostCard({ post, onLike }) {
  const [selectedUserId, setSelectedUserId] = useState('');
  const [users, setUsers] = useState([]);
  const [isLiked, setIsLiked] = useState(false);
  const [comments, setComments] = useState([]);
  const [showComments, setShowComments] = useState(false);
  const [newComment, setNewComment] = useState({ text: '', userId: '' });

  useEffect(() => {
    loadUsers();
    loadComments();
  }, [post.id]);

  const loadUsers = async () => {
    try {
      const data = await api.getUsers();
      setUsers(data);
      if (data.length > 0) {
        setSelectedUserId(data[0].id);
        checkLikeStatus(data[0].id);
      }
    } catch (err) {
      console.error('Kullanıcılar yüklenemedi:', err);
    }
  };

  const checkLikeStatus = async (userId) => {
    try {
      const liked = await api.isLiked(post.id, userId);
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
    if (selectedUserId) {
      onLike(post.id, selectedUserId);
      setIsLiked(!isLiked);
    }
  };

  const handleAddComment = async (e) => {
    e.preventDefault();
    if (!newComment.text || !newComment.userId) return;
    try {
      await api.createComment({
        postId: post.id,
        userId: newComment.userId,
        text: newComment.text,
      });
      setNewComment({ text: '', userId: '' });
      loadComments();
    } catch (err) {
      console.error('Yorum eklenemedi:', err);
    }
  };

  return (
    <div className="post-card">
      <div className="post-header">
        <div>
          <div className="post-username">@{post.username}</div>
          <div className="post-place" style={{ fontSize: '14px', color: '#3498db', marginTop: '5px' }}>
            📍 {post.placeLocation || post.placeName}
          </div>
        </div>
      </div>
      {post.mediaUrl && (
        <img src={post.mediaUrl} alt={post.caption} className="post-image" />
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
          <form onSubmit={handleAddComment} style={{ marginTop: '15px' }}>
            <div className="form-group">
              <select
                value={newComment.userId}
                onChange={(e) => setNewComment({ ...newComment, userId: e.target.value })}
                required
                style={{ marginBottom: '10px' }}
              >
                <option value="">Kullanıcı seçiniz...</option>
                {users.map((user) => (
                  <option key={user.id} value={user.id}>
                    {user.username}
                  </option>
                ))}
              </select>
              <textarea
                value={newComment.text}
                onChange={(e) => setNewComment({ ...newComment, text: e.target.value })}
                placeholder="Yorumunuzu yazın..."
                required
                style={{ marginBottom: '10px' }}
              />
              <button type="submit" className="btn btn-primary">Yorum Ekle</button>
            </div>
          </form>
        </div>
      )}
    </div>
  );
}

