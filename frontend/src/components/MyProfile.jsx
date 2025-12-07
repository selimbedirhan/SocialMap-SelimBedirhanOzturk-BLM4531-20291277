import { useState, useEffect } from 'react';
import { api } from '../services/api';

export default function MyProfile({ user, onUserClick }) {
  const [profile, setProfile] = useState(null);
  const [loading, setLoading] = useState(true);
  const [activeTab, setActiveTab] = useState('posts');
  const [followers, setFollowers] = useState([]);
  const [following, setFollowing] = useState([]);
  const [comments, setComments] = useState([]);
  const [likedPosts, setLikedPosts] = useState([]);
  const [showFollowers, setShowFollowers] = useState(false);
  const [showFollowing, setShowFollowing] = useState(false);
  const [uploadingPhoto, setUploadingPhoto] = useState(false);
  const [showPhotoUpload, setShowPhotoUpload] = useState(false);
  const [photoPreview, setPhotoPreview] = useState(null);
  const [isDragging, setIsDragging] = useState(false);

  useEffect(() => {
    if (user?.id) {
      loadProfile();
      loadFollowers();
      loadFollowing();
      loadComments();
      loadLikedPosts();
    }
  }, [user]);

  const loadProfile = async () => {
    try {
      setLoading(true);
      const data = await api.getProfile(user.id);
      setProfile(data);
    } catch (err) {
      console.error('Profil yüklenemedi:', err);
      setProfile(null);
    } finally {
      setLoading(false);
    }
  };

  const loadFollowers = async () => {
    try {
      const data = await api.getFollowers(user.id);
      setFollowers(data);
    } catch (err) {
      console.error('Takipçiler yüklenemedi:', err);
    }
  };

  const loadFollowing = async () => {
    try {
      const data = await api.getFollowing(user.id);
      setFollowing(data);
    } catch (err) {
      console.error('Takip edilenler yüklenemedi:', err);
    }
  };

  const loadComments = async () => {
    try {
      const data = await api.getCommentsByUser(user.id);
      setComments(data);
    } catch (err) {
      console.error('Yorumlar yüklenemedi:', err);
    }
  };

  const loadLikedPosts = async () => {
    try {
      const data = await api.getLikesByUser(user.id);
      const postIds = data.map(like => like.postId);
      // Her beğeni için post bilgisini al
      const posts = await Promise.all(postIds.map(id => api.getPostById(id).catch(() => null)));
      setLikedPosts(posts.filter(p => p !== null));
    } catch (err) {
      console.error('Beğenilen gönderiler yüklenemedi:', err);
    }
  };

  const validateAndPreviewFile = (file) => {
    if (!file) return false;

    // Dosya tipi kontrolü
    const allowedTypes = ['image/jpeg', 'image/jpg', 'image/png', 'image/gif', 'image/webp'];
    if (!allowedTypes.includes(file.type)) {
      alert('Sadece resim dosyaları yüklenebilir (JPG, PNG, GIF, WEBP)');
      return false;
    }

    // Dosya boyutu kontrolü (5MB)
    if (file.size > 5 * 1024 * 1024) {
      alert('Dosya boyutu 5MB\'dan büyük olamaz');
      return false;
    }

    // Önizleme oluştur
    const reader = new FileReader();
    reader.onloadend = () => {
      setPhotoPreview(reader.result);
    };
    reader.readAsDataURL(file);
    return true;
  };

  const handleFileSelect = (file) => {
    if (validateAndPreviewFile(file)) {
      // Dosya seçildi, önizleme gösterilecek
    }
  };

  const handlePhotoUpload = async () => {
    if (!photoPreview) {
      alert('Lütfen bir resim seçin');
      return;
    }

    // File input'tan dosyayı al
    const fileInput = document.getElementById('profile-photo-input');
    const file = fileInput?.files[0];
    if (!file) return;

    try {
      setUploadingPhoto(true);
      
      // Resmi yükle
      const uploadResult = await api.uploadImage(file);
      
      // Kullanıcı bilgilerini güncelle
      const currentUser = await api.getUserById(user.id);
      await api.updateUserProfile(user.id, {
        ...currentUser,
        profilePhotoUrl: uploadResult.url
      });

      // Profili yeniden yükle
      await loadProfile();
      
      // LocalStorage'daki kullanıcı bilgisini güncelle
      const updatedUser = { ...user, profilePhotoUrl: uploadResult.url };
      localStorage.setItem('user', JSON.stringify(updatedUser));
      
      setShowPhotoUpload(false);
      setPhotoPreview(null);
      setIsDragging(false);
      alert('Profil resmi başarıyla güncellendi!');
    } catch (err) {
      console.error('Profil resmi yüklenemedi:', err);
      alert('Profil resmi yüklenirken hata oluştu: ' + (err.message || 'Bilinmeyen hata'));
    } finally {
      setUploadingPhoto(false);
      // Input'u temizle
      if (fileInput) fileInput.value = '';
    }
  };

  const handleDragOver = (e) => {
    e.preventDefault();
    e.stopPropagation();
    setIsDragging(true);
  };

  const handleDragLeave = (e) => {
    e.preventDefault();
    e.stopPropagation();
    setIsDragging(false);
  };

  const handleDrop = (e) => {
    e.preventDefault();
    e.stopPropagation();
    setIsDragging(false);

    const file = e.dataTransfer.files[0];
    if (file) {
      handleFileSelect(file);
      // File input'a da dosyayı set et
      const dataTransfer = new DataTransfer();
      dataTransfer.items.add(file);
      const fileInput = document.getElementById('profile-photo-input');
      if (fileInput) {
        fileInput.files = dataTransfer.files;
      }
    }
  };

  const handleFileInputChange = (e) => {
    const file = e.target.files[0];
    if (file) {
      handleFileSelect(file);
    }
  };

  if (loading) {
    return <div className="loading">Yükleniyor...</div>;
  }

  if (!profile || !profile.user) {
    return <div className="error">Profil bulunamadı.</div>;
  }

  return (
    <div>
      <div style={{ display: 'flex', gap: '20px', alignItems: 'center', marginBottom: '30px' }}>
        <div style={{ position: 'relative' }}>
          {profile.user.profilePhotoUrl ? (
            <img
              src={`http://localhost:5280${profile.user.profilePhotoUrl}`}
              alt={profile.user.username}
              style={{ width: '100px', height: '100px', borderRadius: '50%', objectFit: 'cover', cursor: 'pointer' }}
              onClick={() => setShowPhotoUpload(!showPhotoUpload)}
              title="Profil resmini değiştirmek için tıklayın"
            />
          ) : (
            <div 
              style={{ 
                width: '100px', 
                height: '100px', 
                borderRadius: '50%', 
                background: '#ddd', 
                display: 'flex', 
                alignItems: 'center', 
                justifyContent: 'center', 
                fontSize: '36px',
                cursor: 'pointer'
              }}
              onClick={() => setShowPhotoUpload(!showPhotoUpload)}
              title="Profil resmi eklemek için tıklayın"
            >
              {profile.user.username[0].toUpperCase()}
            </div>
          )}
          {showPhotoUpload && (
            <div style={{
              position: 'absolute',
              top: '110px',
              left: '0',
              background: 'white',
              padding: '20px',
              borderRadius: '12px',
              boxShadow: '0 8px 24px rgba(0,0,0,0.15)',
              zIndex: 10,
              minWidth: '320px',
              border: '1px solid #e0e0e0'
            }}>
              <div style={{ 
                display: 'flex', 
                justifyContent: 'space-between', 
                alignItems: 'center',
                marginBottom: '15px'
              }}>
                <h3 style={{ margin: 0, fontSize: '18px', color: '#2c3e50' }}>Profil Resmi Yükle</h3>
                <button
                  onClick={() => {
                    setShowPhotoUpload(false);
                    setPhotoPreview(null);
                    setIsDragging(false);
                    const fileInput = document.getElementById('profile-photo-input');
                    if (fileInput) fileInput.value = '';
                  }}
                  style={{
                    background: 'none',
                    border: 'none',
                    fontSize: '20px',
                    cursor: 'pointer',
                    color: '#7f8c8d',
                    padding: '0',
                    width: '24px',
                    height: '24px',
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'center'
                  }}
                >
                  ×
                </button>
              </div>

              <div
                onDragOver={handleDragOver}
                onDragLeave={handleDragLeave}
                onDrop={handleDrop}
                style={{
                  border: `2px dashed ${isDragging ? '#3498db' : '#ddd'}`,
                  borderRadius: '12px',
                  padding: '30px',
                  textAlign: 'center',
                  background: isDragging ? '#f0f8ff' : '#fafafa',
                  transition: 'all 0.3s ease',
                  marginBottom: '15px',
                  cursor: 'pointer',
                  minHeight: '200px',
                  display: 'flex',
                  flexDirection: 'column',
                  alignItems: 'center',
                  justifyContent: 'center',
                  gap: '15px'
                }}
                onClick={() => document.getElementById('profile-photo-input')?.click()}
              >
                {photoPreview ? (
                  <>
                    <img
                      src={photoPreview}
                      alt="Önizleme"
                      style={{
                        width: '150px',
                        height: '150px',
                        borderRadius: '50%',
                        objectFit: 'cover',
                        border: '3px solid #3498db',
                        boxShadow: '0 4px 8px rgba(0,0,0,0.1)'
                      }}
                    />
                    <div style={{ color: '#27ae60', fontSize: '14px', fontWeight: '500' }}>
                      ✓ Resim seçildi
                    </div>
                  </>
                ) : (
                  <>
                    <div style={{
                      width: '80px',
                      height: '80px',
                      borderRadius: '50%',
                      background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
                      display: 'flex',
                      alignItems: 'center',
                      justifyContent: 'center',
                      fontSize: '40px',
                      color: 'white',
                      marginBottom: '10px'
                    }}>
                      📷
                    </div>
                    <div style={{ color: '#2c3e50', fontSize: '16px', fontWeight: '500', marginBottom: '5px' }}>
                      {isDragging ? 'Dosyayı buraya bırakın' : 'Resmi sürükleyip bırakın veya tıklayın'}
                    </div>
                    <div style={{ color: '#7f8c8d', fontSize: '13px' }}>
                      JPG, PNG, GIF veya WEBP (Max 5MB)
                    </div>
                  </>
                )}
                <input
                  id="profile-photo-input"
                  type="file"
                  accept="image/jpeg,image/jpg,image/png,image/gif,image/webp"
                  onChange={handleFileInputChange}
                  disabled={uploadingPhoto}
                  style={{ display: 'none' }}
                />
              </div>

              <div style={{ display: 'flex', gap: '10px' }}>
                <button
                  className="btn btn-primary"
                  onClick={handlePhotoUpload}
                  disabled={!photoPreview || uploadingPhoto}
                  style={{
                    flex: 1,
                    opacity: (!photoPreview || uploadingPhoto) ? 0.6 : 1,
                    cursor: (!photoPreview || uploadingPhoto) ? 'not-allowed' : 'pointer'
                  }}
                >
                  {uploadingPhoto ? (
                    <span style={{ display: 'flex', alignItems: 'center', gap: '8px', justifyContent: 'center' }}>
                      <span style={{
                        width: '16px',
                        height: '16px',
                        border: '2px solid white',
                        borderTop: '2px solid transparent',
                        borderRadius: '50%',
                        animation: 'spin 1s linear infinite',
                        display: 'inline-block'
                      }}></span>
                      Yükleniyor...
                    </span>
                  ) : (
                    'Yükle'
                  )}
                </button>
                <button
                  className="btn btn-secondary"
                  onClick={() => {
                    setShowPhotoUpload(false);
                    setPhotoPreview(null);
                    setIsDragging(false);
                    const fileInput = document.getElementById('profile-photo-input');
                    if (fileInput) fileInput.value = '';
                  }}
                  disabled={uploadingPhoto}
                  style={{ flex: 1 }}
                >
                  İptal
                </button>
              </div>
            </div>
          )}
        </div>
        <div style={{ flex: 1 }}>
          <h2>{profile.user.username}</h2>
          {profile.user.bio && <p style={{ color: '#555', marginTop: '10px' }}>{profile.user.bio}</p>}
          <div style={{ display: 'flex', gap: '20px', marginTop: '15px' }}>
            <div style={{ cursor: 'pointer' }} onClick={() => setShowFollowers(!showFollowers)}>
              <strong>{profile.stats?.followerCount ?? 0}</strong> Takipçi
            </div>
            <div style={{ cursor: 'pointer' }} onClick={() => setShowFollowing(!showFollowing)}>
              <strong>{profile.stats?.followingCount ?? 0}</strong> Takip Edilen
            </div>
            <div>
              <strong>{profile.stats?.postsCount ?? 0}</strong> Gönderi
            </div>
          </div>
        </div>
      </div>

      {showFollowers && (
        <div style={{ marginBottom: '20px', padding: '20px', background: '#f9f9f9', borderRadius: '8px' }}>
          <h3>Takipçiler ({followers.length})</h3>
          <div className="users-list">
            {followers.map((follower) => (
              <div
                key={follower.id}
                className="user-card"
                style={{
                  cursor: 'pointer',
                  transition: 'all 0.3s ease',
                  display: 'flex',
                  flexDirection: 'column',
                  alignItems: 'center',
                  padding: '25px',
                  background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
                  color: 'white',
                  border: 'none',
                  boxShadow: '0 4px 6px rgba(0,0,0,0.1)'
                }}
                onClick={() => onUserClick && onUserClick(follower.id)}
                onMouseEnter={(e) => {
                  e.currentTarget.style.transform = 'translateY(-5px)';
                  e.currentTarget.style.boxShadow = '0 8px 15px rgba(0,0,0,0.2)';
                }}
                onMouseLeave={(e) => {
                  e.currentTarget.style.transform = 'translateY(0)';
                  e.currentTarget.style.boxShadow = '0 4px 6px rgba(0,0,0,0.1)';
                }}
              >
                <div style={{
                  width: '80px',
                  height: '80px',
                  borderRadius: '50%',
                  background: 'rgba(255,255,255,0.3)',
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'center',
                  fontSize: '32px',
                  fontWeight: 'bold',
                  marginBottom: '15px',
                  border: '3px solid rgba(255,255,255,0.5)'
                }}>
                  {follower.profilePhotoUrl ? (
                    <img
                      src={`http://localhost:5280${follower.profilePhotoUrl}`}
                      alt={follower.username}
                      style={{
                        width: '100%',
                        height: '100%',
                        borderRadius: '50%',
                        objectFit: 'cover'
                      }}
                    />
                  ) : (
                    follower.username[0].toUpperCase()
                  )}
                </div>
                <div style={{
                  fontSize: '20px',
                  fontWeight: '600',
                  marginBottom: '8px',
                  textAlign: 'center'
                }}>
                  {follower.username}
                </div>
                <div style={{
                  fontSize: '14px',
                  opacity: 0.9,
                  textAlign: 'center',
                  marginBottom: '10px'
                }}>
                  {follower.email}
                </div>
                {follower.bio && (
                  <div style={{
                    fontSize: '13px',
                    opacity: 0.8,
                    textAlign: 'center',
                    marginTop: '8px',
                    fontStyle: 'italic'
                  }}>
                    {follower.bio.length > 50 ? `${follower.bio.substring(0, 50)}...` : follower.bio}
                  </div>
                )}
                <div style={{
                  marginTop: '15px',
                  padding: '8px 16px',
                  background: 'rgba(255,255,255,0.2)',
                  borderRadius: '20px',
                  fontSize: '12px',
                  fontWeight: '500'
                }}>
                  Profili Gör →
                </div>
              </div>
            ))}
          </div>
        </div>
      )}

      {showFollowing && (
        <div style={{ marginBottom: '20px', padding: '20px', background: '#f9f9f9', borderRadius: '8px' }}>
          <h3>Takip Edilenler ({following.length})</h3>
          <div className="users-list">
            {following.map((follow) => (
              <div
                key={follow.id}
                className="user-card"
                style={{
                  cursor: 'pointer',
                  transition: 'all 0.3s ease',
                  display: 'flex',
                  flexDirection: 'column',
                  alignItems: 'center',
                  padding: '25px',
                  background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
                  color: 'white',
                  border: 'none',
                  boxShadow: '0 4px 6px rgba(0,0,0,0.1)'
                }}
                onClick={() => onUserClick && onUserClick(follow.id)}
                onMouseEnter={(e) => {
                  e.currentTarget.style.transform = 'translateY(-5px)';
                  e.currentTarget.style.boxShadow = '0 8px 15px rgba(0,0,0,0.2)';
                }}
                onMouseLeave={(e) => {
                  e.currentTarget.style.transform = 'translateY(0)';
                  e.currentTarget.style.boxShadow = '0 4px 6px rgba(0,0,0,0.1)';
                }}
              >
                <div style={{
                  width: '80px',
                  height: '80px',
                  borderRadius: '50%',
                  background: 'rgba(255,255,255,0.3)',
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'center',
                  fontSize: '32px',
                  fontWeight: 'bold',
                  marginBottom: '15px',
                  border: '3px solid rgba(255,255,255,0.5)'
                }}>
                  {follow.profilePhotoUrl ? (
                    <img
                      src={`http://localhost:5280${follow.profilePhotoUrl}`}
                      alt={follow.username}
                      style={{
                        width: '100%',
                        height: '100%',
                        borderRadius: '50%',
                        objectFit: 'cover'
                      }}
                    />
                  ) : (
                    follow.username[0].toUpperCase()
                  )}
                </div>
                <div style={{
                  fontSize: '20px',
                  fontWeight: '600',
                  marginBottom: '8px',
                  textAlign: 'center'
                }}>
                  {follow.username}
                </div>
                <div style={{
                  fontSize: '14px',
                  opacity: 0.9,
                  textAlign: 'center',
                  marginBottom: '10px'
                }}>
                  {follow.email}
                </div>
                {follow.bio && (
                  <div style={{
                    fontSize: '13px',
                    opacity: 0.8,
                    textAlign: 'center',
                    marginTop: '8px',
                    fontStyle: 'italic'
                  }}>
                    {follow.bio.length > 50 ? `${follow.bio.substring(0, 50)}...` : follow.bio}
                  </div>
                )}
                <div style={{
                  marginTop: '15px',
                  padding: '8px 16px',
                  background: 'rgba(255,255,255,0.2)',
                  borderRadius: '20px',
                  fontSize: '12px',
                  fontWeight: '500'
                }}>
                  Profili Gör →
                </div>
              </div>
            ))}
          </div>
        </div>
      )}

      <div style={{ display: 'flex', gap: '10px', marginBottom: '20px' }}>
        <button
          className={`btn ${activeTab === 'posts' ? 'btn-primary' : 'btn-secondary'}`}
          onClick={() => setActiveTab('posts')}
        >
          Gönderiler
        </button>
        <button
          className={`btn ${activeTab === 'comments' ? 'btn-primary' : 'btn-secondary'}`}
          onClick={() => setActiveTab('comments')}
        >
          Yorumlar
        </button>
        <button
          className={`btn ${activeTab === 'likes' ? 'btn-primary' : 'btn-secondary'}`}
          onClick={() => setActiveTab('likes')}
        >
          Beğeniler
        </button>
      </div>

      {activeTab === 'posts' && (
        <div>
          <h3>Gönderiler ({profile.posts?.length ?? 0})</h3>
          <div className="posts-grid">
            {(profile.posts || []).map((post) => (
              <div key={post.id} className="post-card">
                {post.mediaUrl && (
                  <img
                    src={`http://localhost:5280${post.mediaUrl}`}
                    alt={post.caption}
                    className="post-image"
                  />
                )}
                {post.caption && <div className="post-caption">{post.caption}</div>}
                <div className="post-stats">
                  ❤️ {post.likesCount} • 💬 {post.commentsCount}
                </div>
              </div>
            ))}
          </div>
        </div>
      )}

      {activeTab === 'comments' && (
        <div>
          <h3>Yorumlar ({comments.length})</h3>
          {comments.map((comment) => (
            <div key={comment.id} className="comment" style={{ marginBottom: '15px' }}>
              <div className="comment-header">
                <span className="comment-username">@{comment.username}</span>
                <span style={{ fontSize: '12px', color: '#7f8c8d' }}>
                  {new Date(comment.createdAt).toLocaleDateString('tr-TR')}
                </span>
              </div>
              <div className="comment-text">{comment.text}</div>
            </div>
          ))}
        </div>
      )}

      {activeTab === 'likes' && (
        <div>
          <h3>Beğenilen Gönderiler ({likedPosts.length})</h3>
          <div className="posts-grid">
            {likedPosts.map((post) => (
              <div key={post.id} className="post-card">
                {post.mediaUrl && (
                  <img
                    src={`http://localhost:5280${post.mediaUrl}`}
                    alt={post.caption}
                    className="post-image"
                  />
                )}
                {post.caption && <div className="post-caption">{post.caption}</div>}
                <div className="post-place" style={{ fontSize: '12px', color: '#7f8c8d', marginTop: '10px' }}>
                  📍 {post.placeLocation}
                </div>
                <div className="post-stats">
                  ❤️ {post.likesCount} • 💬 {post.commentsCount}
                </div>
              </div>
            ))}
          </div>
        </div>
      )}
    </div>
  );
}

