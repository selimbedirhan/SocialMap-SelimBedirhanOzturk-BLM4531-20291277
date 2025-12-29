import { useState, useEffect } from 'react';
import { api } from '../services/api';
import PostDetailModal from './PostDetailModal';

export default function Profile({ userId, currentUserId, onUserClick }) {
  const [profile, setProfile] = useState(null);
  const [loading, setLoading] = useState(true);
  const [isFollowing, setIsFollowing] = useState(false);
  const [activeTab, setActiveTab] = useState('posts');
  const [followers, setFollowers] = useState([]);
  const [following, setFollowing] = useState([]);
  const [comments, setComments] = useState([]);
  const [likedPosts, setLikedPosts] = useState([]);
  const [showFollowers, setShowFollowers] = useState(false);
  const [showFollowing, setShowFollowing] = useState(false);
  const [selectedPost, setSelectedPost] = useState(null);

  useEffect(() => {
    if (userId) {
      loadProfile();
      loadFollowers();
      loadFollowing();
      loadComments();
      loadLikedPosts();
    }
  }, [userId]);

  useEffect(() => {
    if (currentUserId && userId && currentUserId !== userId) {
      checkFollowing();
    }
  }, [currentUserId, userId]);

  const loadProfile = async () => {
    try {
      setLoading(true);
      const data = await api.getProfile(userId);
      if (data && data.user) {
        setProfile(data);
      } else {
        console.error('Profil verisi eksik:', data);
        setProfile(null);
      }
    } catch (err) {
      console.error('Profil yüklenemedi:', err);
      setProfile(null);
    } finally {
      setLoading(false);
    }
  };

  const checkFollowing = async () => {
    try {
      const following = await api.isFollowing(currentUserId, userId);
      setIsFollowing(following);
    } catch (err) {
      console.error('Takip durumu kontrol edilemedi:', err);
    }
  };

  const loadFollowers = async () => {
    try {
      const data = await api.getFollowers(userId);
      setFollowers(data);
    } catch (err) {
      console.error('Takipçiler yüklenemedi:', err);
    }
  };

  const loadFollowing = async () => {
    try {
      const data = await api.getFollowing(userId);
      setFollowing(data);
    } catch (err) {
      console.error('Takip edilenler yüklenemedi:', err);
    }
  };

  const loadComments = async () => {
    try {
      const data = await api.getCommentsByUser(userId);
      setComments(data);
    } catch (err) {
      console.error('Yorumlar yüklenemedi:', err);
    }
  };

  const loadLikedPosts = async () => {
    try {
      const data = await api.getLikesByUser(userId);
      const postIds = data.map(like => like.postId);
      // Her beğeni için post bilgisini al
      const posts = await Promise.all(postIds.map(id => api.getPostById(id).catch(() => null)));
      setLikedPosts(posts.filter(p => p !== null));
    } catch (err) {
      console.error('Beğenilen gönderiler yüklenemedi:', err);
    }
  };

  const handleFollow = async () => {
    if (!currentUserId) {
      return;
    }
    try {
      if (isFollowing) {
        await api.unfollowUser(currentUserId, userId);
        setIsFollowing(false);
      } else {
        await api.followUser(currentUserId, userId);
        setIsFollowing(true);
      }
      loadProfile();
      loadFollowers();
    } catch (err) {
      console.error('Takip hatası:', err);
    }
  };

  if (loading) {
    return <div className="loading">Yükleniyor...</div>;
  }

  if (!profile || !profile.user) {
    return <div className="error">Profil bulunamadı.</div>;
  }

  // Helper for rendering avatar
  const renderAvatar = (user) => {
    if (user.profilePhotoUrl) {
      return (
        <img
          src={`http://localhost:5280${user.profilePhotoUrl}`}
          alt={user.username}
          style={{ width: '100px', height: '100px', borderRadius: '50%', objectFit: 'cover' }}
        />
      );
    } else {
      return (
        <div style={{ width: '100px', height: '100px', borderRadius: '50%', background: '#ddd', display: 'flex', alignItems: 'center', justifyContent: 'center', fontSize: '36px' }}>
          {user.username[0].toUpperCase()}
        </div>
      );
    }
  };

  return (
    <div>
      <div style={{ display: 'flex', gap: '20px', alignItems: 'center', marginBottom: '30px' }}>
        <div>
          {renderAvatar(profile.user)}
        </div>
        <div style={{ flex: 1 }}>
          <div style={{ display: 'flex', alignItems: 'center', gap: '15px', marginBottom: '10px' }}>
            <h2 style={{ margin: 0 }}>{profile.user.username}</h2>
            {currentUserId && currentUserId !== userId && (
              <button
                className={`btn ${isFollowing ? 'btn-secondary' : 'btn-primary'}`}
                onClick={handleFollow}
              >
                {isFollowing ? 'Takipten Çık' : 'Takip Et'}
              </button>
            )}
          </div>
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
              <div
                key={post.id}
                className="post-card"
                style={{ cursor: 'pointer' }}
                onClick={() => setSelectedPost(post)}
              >
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
              <div
                key={post.id}
                className="post-card"
                style={{ cursor: 'pointer' }}
                onClick={() => setSelectedPost(post)}
              >
                {post.mediaUrl && (
                  <img
                    src={`http://localhost:5280${post.mediaUrl}`}
                    alt={post.caption}
                    className="post-image"
                  />
                )}
                {post.caption && <div className="post-caption">{post.caption}</div>}
                {post.placeLocation && (
                  <div className="post-place" style={{ fontSize: '12px', color: '#7f8c8d', marginTop: '10px' }}>
                    📍 {post.placeLocation}
                  </div>
                )}
                <div className="post-stats">
                  ❤️ {post.likesCount} • 💬 {post.commentsCount}
                </div>
              </div>
            ))}
          </div>
        </div>
      )}

      {selectedPost && (
        <PostDetailModal
          post={selectedPost}
          user={{ id: currentUserId }}
          onClose={() => setSelectedPost(null)}
          onUserClick={onUserClick}
          onLike={(postId, isLiked) => {
            // Opsiyonel güncellemeler
          }}
        />
      )}
    </div>
  );
}
