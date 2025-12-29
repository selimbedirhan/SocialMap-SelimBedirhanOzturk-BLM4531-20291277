import { useState, useEffect } from 'react';
import { api } from '../services/api';
import PostDetailModal from './PostDetailModal';

export default function Places() {
  const [places, setPlaces] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [showForm, setShowForm] = useState(false);
  const [searchTerm, setSearchTerm] = useState('');
  const [formData, setFormData] = useState({
    name: '',
    city: '',
    country: 'Türkiye',
    district: '',
    latitude: '',
    longitude: '',
    description: '',
    tags: '',
    createdById: '',
  });
  const [users, setUsers] = useState([]);
  const [currentUser, setCurrentUser] = useState(null);
  const [selectedPlace, setSelectedPlace] = useState(null);
  const [selectedPlacePosts, setSelectedPlacePosts] = useState([]);
  const [loadingPlacePosts, setLoadingPlacePosts] = useState(false);
  const [selectedPost, setSelectedPost] = useState(null);

  const [postResults, setPostResults] = useState([]);
  const [searching, setSearching] = useState(false);

  useEffect(() => {
    loadPlaces();
    loadUsers();
    // LocalStorage'dan kullanıcı bilgisini al
    const savedUser = localStorage.getItem('user');
    if (savedUser) {
      try {
        const user = JSON.parse(savedUser);
        setCurrentUser(user);
        setFormData(prev => ({ ...prev, createdById: user.id }));
      } catch (err) {
        console.error('Kullanıcı bilgisi yüklenemedi:', err);
      }
    }
  }, []);

  const loadPlaces = async () => {
    try {
      setLoading(true);
      const data = await api.getPlaces();
      setPlaces(data);
      setError(null);
    } catch (err) {
      setError('Yerler yüklenirken hata oluştu.');
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

  const handleSearch = async () => {
    if (!searchTerm.trim()) {
      loadPlaces();
      setPostResults([]);
      return;
    }

    try {
      setLoading(true);
      setSearching(true);

      // Paralel aramalar
      const [placesData, postsData] = await Promise.all([
        api.searchPlaces(searchTerm),
        api.searchPosts({ term: searchTerm })
      ]);

      setPlaces(placesData);
      setPostResults(postsData);
    } catch (err) {
      setError('Arama yapılırken hata oluştu.');
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    try {
      await api.createPlace({
        name: formData.name,
        city: formData.city,
        country: formData.country || 'Türkiye',
        district: formData.district || null,
        latitude: formData.latitude ? parseFloat(formData.latitude) : null,
        longitude: formData.longitude ? parseFloat(formData.longitude) : null,
        description: formData.description || null,
        tags: formData.tags || null,
        createdById: formData.createdById || currentUser?.id,
      });
      setShowForm(false);
      setFormData({
        name: '',
        city: '',
        country: 'Türkiye',
        district: '',
        latitude: '',
        longitude: '',
        description: '',
        tags: '',
        createdById: currentUser?.id || '',
      });
      loadPlaces();
    } catch (err) {
      setError('Yer oluşturulurken hata oluştu.');
    }
  };

  const handlePlaceClick = async (place) => {
    setSelectedPlace(place);
    setLoadingPlacePosts(true);
    try {
      const posts = await api.getPostsByPlace(place.id);
      setSelectedPlacePosts(posts || []);
    } catch (err) {
      console.error('Yer gönderileri yüklenemedi:', err);
    } finally {
      setLoadingPlacePosts(false);
    }
  };

  if (loading && !places.length && !postResults.length) {
    return <div className="loading">Yükleniyor...</div>;
  }

  return (
    <div>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '20px' }}>
        <h2>Yerler & Keşfet</h2>
        <button className="btn btn-primary" onClick={() => setShowForm(!showForm)}>
          {showForm ? 'İptal' : 'Yeni Yer Ekle'}
        </button>
      </div>

      {error && <div className="error">{error}</div>}

      <div style={{ marginBottom: '30px', display: 'flex', gap: '10px' }}>
        <input
          type="text"
          placeholder="Şehir, mekan veya konu ara (örn: Ankara, Starbucks, Kahve)..."
          value={searchTerm}
          onChange={(e) => setSearchTerm(e.target.value)}
          onKeyDown={(e) => e.key === 'Enter' && handleSearch()}
          style={{ flex: 1, padding: '15px', border: '1px solid var(--border-color)', borderRadius: '15px', fontSize: '16px', backgroundColor: 'rgba(255,255,255,0.05)', color: 'var(--text-primary)' }}
        />
        <button className="btn btn-primary" onClick={handleSearch} style={{ padding: '0 30px' }}>Ara</button>
      </div>

      {showForm && (
        <form className="form" onSubmit={handleSubmit} style={{ marginBottom: '30px' }}>
          {/* Form Content kept same as before but encapsulated for brevity in edit if not changed, but here we replace all so included everything */}
          <div className="form-group">
            <label>Yer Adı</label>
            <input
              type="text"
              value={formData.name}
              onChange={(e) => setFormData({ ...formData, name: e.target.value })}
              required
            />
          </div>
          <div className="form-group">
            <label>Şehir</label>
            <input
              type="text"
              value={formData.city}
              onChange={(e) => setFormData({ ...formData, city: e.target.value })}
              required
            />
          </div>
          <div className="form-group">
            <label>Ülke</label>
            <input
              type="text"
              value={formData.country || 'Türkiye'}
              onChange={(e) => setFormData({ ...formData, country: e.target.value })}
              placeholder="Türkiye"
            />
          </div>
          <div className="form-group">
            <label>İlçe (Opsiyonel)</label>
            <input
              type="text"
              value={formData.district}
              onChange={(e) => setFormData({ ...formData, district: e.target.value })}
            />
          </div>
          <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '15px' }}>
            <div className="form-group">
              <label>Enlem (Opsiyonel)</label>
              <input
                type="number"
                step="any"
                value={formData.latitude}
                onChange={(e) => setFormData({ ...formData, latitude: e.target.value })}
              />
            </div>
            <div className="form-group">
              <label>Boylam (Opsiyonel)</label>
              <input
                type="number"
                step="any"
                value={formData.longitude}
                onChange={(e) => setFormData({ ...formData, longitude: e.target.value })}
              />
            </div>
          </div>
          <div className="form-group">
            <label>Açıklama (Opsiyonel)</label>
            <textarea
              value={formData.description}
              onChange={(e) => setFormData({ ...formData, description: e.target.value })}
            />
          </div>
          <div className="form-group">
            <label>Etiketler (Opsiyonel, virgülle ayırın)</label>
            <input
              type="text"
              value={formData.tags}
              onChange={(e) => setFormData({ ...formData, tags: e.target.value })}
              placeholder="doğa, tarih, kültür"
            />
          </div>
          <button type="submit" className="btn btn-primary">Oluştur</button>
        </form>
      )}

      {/* Post Results Section - Only show when searching */}
      {searching && postResults.length > 0 && (
        <div style={{ marginBottom: '40px' }}>
          <h3 style={{ marginBottom: '15px', borderBottom: '1px solid var(--border-color)', paddingBottom: '10px' }}>
            Bulunan Gönderiler ({postResults.length})
          </h3>
          <div className="posts-grid">
            {postResults.map((post) => (
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
                <div style={{ fontSize: '12px', color: '#999', margin: '5px 15px' }}>
                  📍 {post.placeName || post.city || 'Konum Yok'}
                </div>
                <div className="post-stats">
                  ❤️ {post.likesCount} • 💬 {post.commentsCount}
                </div>
              </div>
            ))}
          </div>
        </div>
      )}

      {/* Places Results Section */}
      <div>
        {searching && <h3 style={{ marginBottom: '15px', borderBottom: '1px solid var(--border-color)', paddingBottom: '10px' }}>
          {places.length > 0 ? `Bulunan Yerler (${places.length})` : 'Bulunan Yer Yok'}
        </h3>}

        <div className="places-list">
          {places.map((place) => (
            <div
              key={place.id}
              className="place-card"
              style={{ cursor: 'pointer' }}
              onClick={() => handlePlaceClick(place)}
            >
              <div className="place-name">{place.name}</div>
              <div className="place-location">
                {place.city} {place.district && `- ${place.district}`} - {place.country || 'Türkiye'}
              </div>
              {place.description && (
                <div className="place-description">{place.description}</div>
              )}
              {place.tags && (
                <div style={{ marginTop: '10px', fontSize: '12px', color: 'var(--accent-color)' }}>
                  {place.tags.split(',').map((tag, i) => (
                    <span key={i} style={{ marginRight: '5px' }}>#{tag.trim()}</span>
                  ))}
                </div>
              )}
              <div style={{ marginTop: '10px', fontSize: '12px', opacity: 0.7 }}>
                {place.postsCount ? `${place.postsCount} gönderi` : 'Henüz gönderi yok'}
              </div>
            </div>
          ))}
        </div>
      </div>

      {selectedPlace && (
        <div style={{ marginTop: '30px' }}>
          <h3>
            {selectedPlace.name} - {selectedPlace.city}{' '}
            <span style={{ fontSize: '14px', color: '#7f8c8d' }}>
              ({selectedPlace.postsCount} gönderi)
            </span>
          </h3>
          {selectedPlace.description && (
            <p style={{ marginTop: '10px', color: '#555' }}>{selectedPlace.description}</p>
          )}

          {loadingPlacePosts ? (
            <div className="loading">Gönderiler yükleniyor...</div>
          ) : selectedPlacePosts.length === 0 ? (
            <div style={{ padding: '20px', color: '#7f8c8d' }}>Bu yerde henüz gönderi yok.</div>
          ) : (
            <div className="posts-grid" style={{ marginTop: '15px' }}>
              {selectedPlacePosts.map((post) => (
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
          )}
        </div>
      )}

      {selectedPost && (
        <PostDetailModal
          post={selectedPost}
          user={currentUser}
          onClose={() => setSelectedPost(null)}
          onUserClick={(userId) => {
            // App.jsx'e navigate etmek için window.location veya parent'a prop geçmek gerekir
            // Şimdilik basit bir çözüm
            window.location.hash = `profile-${userId}`;
          }}
          onLike={async (postId, isLiked) => {
            // Hem genel aramadan hem yer detayından güncelle
            setPostResults(prev => prev.map(p => p.id === postId ? { ...p, likesCount: isLiked ? p.likesCount + 1 : Math.max(0, p.likesCount - 1) } : p));
            setSelectedPlacePosts(prev => prev.map(p => p.id === postId ? { ...p, likesCount: isLiked ? p.likesCount + 1 : Math.max(0, p.likesCount - 1) } : p));
          }}
        />
      )}
    </div>
  );
}

