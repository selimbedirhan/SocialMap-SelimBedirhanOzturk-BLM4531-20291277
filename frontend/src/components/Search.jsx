import { useState } from 'react';
import { api } from '../services/api';

export default function Search({ onUserClick }) {
  const [searchTerm, setSearchTerm] = useState('');
  const [results, setResults] = useState(null);
  const [loading, setLoading] = useState(false);
  const [activeTab, setActiveTab] = useState('all');
  const [filters, setFilters] = useState({
    term: '',
    city: '',
    fromDate: '',
    toDate: '',
    sortBy: 'newest'
  });
  const [postResults, setPostResults] = useState([]);

  const handleSearch = async () => {
    if (!searchTerm.trim()) return;
    
    try {
      setLoading(true);
      const data = await api.searchAll(searchTerm);
      setResults(data);
    } catch (err) {
      console.error('Arama hatası:', err);
    } finally {
      setLoading(false);
    }
  };

  const handlePostSearch = async () => {
    try {
      setLoading(true);
      const data = await api.searchPosts(filters);
      setPostResults(data);
    } catch (err) {
      console.error('Gönderi arama hatası:', err);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div>
      <h2>Arama</h2>

      <div style={{ marginBottom: '20px' }}>
        <div style={{ display: 'flex', gap: '10px', marginBottom: '15px' }}>
          <button
            className={`btn ${activeTab === 'all' ? 'btn-primary' : 'btn-secondary'}`}
            onClick={() => setActiveTab('all')}
          >
            Tümü
          </button>
          <button
            className={`btn ${activeTab === 'posts' ? 'btn-primary' : 'btn-secondary'}`}
            onClick={() => setActiveTab('posts')}
          >
            Gönderiler
          </button>
        </div>

        {activeTab === 'all' && (
          <div style={{ display: 'flex', gap: '10px' }}>
            <input
              type="text"
              placeholder="Ara..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              onKeyPress={(e) => e.key === 'Enter' && handleSearch()}
              style={{ flex: 1, padding: '10px', border: '1px solid #ddd', borderRadius: '5px' }}
            />
            <button className="btn btn-primary" onClick={handleSearch}>Ara</button>
          </div>
        )}

        {activeTab === 'posts' && (
          <div className="form">
            <div className="form-group">
              <label>Arama Terimi</label>
              <input
                type="text"
                value={filters.term}
                onChange={(e) => setFilters({ ...filters, term: e.target.value })}
              />
            </div>
            <div className="form-group">
              <label>Şehir</label>
              <input
                type="text"
                value={filters.city}
                onChange={(e) => setFilters({ ...filters, city: e.target.value })}
              />
            </div>
            <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '15px' }}>
              <div className="form-group">
                <label>Başlangıç Tarihi</label>
                <input
                  type="date"
                  value={filters.fromDate}
                  onChange={(e) => setFilters({ ...filters, fromDate: e.target.value })}
                />
              </div>
              <div className="form-group">
                <label>Bitiş Tarihi</label>
                <input
                  type="date"
                  value={filters.toDate}
                  onChange={(e) => setFilters({ ...filters, toDate: e.target.value })}
                />
              </div>
            </div>
            <div className="form-group">
              <label>Sıralama</label>
              <select
                value={filters.sortBy}
                onChange={(e) => setFilters({ ...filters, sortBy: e.target.value })}
              >
                <option value="newest">En Yeni</option>
                <option value="oldest">En Eski</option>
                <option value="likes">En Çok Beğenilen</option>
                <option value="comments">En Çok Yorumlanan</option>
              </select>
            </div>
            <button className="btn btn-primary" onClick={handlePostSearch}>Ara</button>
          </div>
        )}
      </div>

      {loading && <div className="loading">Aranıyor...</div>}

      {activeTab === 'all' && results && (
        <div>
          {results.users.length > 0 && (
            <div style={{ marginBottom: '30px' }}>
              <h3>Kullanıcılar ({results.users.length})</h3>
              <div className="users-list">
                {results.users.map((user) => (
                  <div
                    key={user.id}
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
                    onClick={() => onUserClick && onUserClick(user.id)}
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
                      {user.profilePhotoUrl ? (
                        <img
                          src={`http://localhost:5280${user.profilePhotoUrl}`}
                          alt={user.username}
                          style={{
                            width: '100%',
                            height: '100%',
                            borderRadius: '50%',
                            objectFit: 'cover'
                          }}
                        />
                      ) : (
                        user.username[0].toUpperCase()
                      )}
                    </div>
                    <div style={{
                      fontSize: '20px',
                      fontWeight: '600',
                      marginBottom: '8px',
                      textAlign: 'center'
                    }}>
                      {user.username}
                    </div>
                    <div style={{
                      fontSize: '14px',
                      opacity: 0.9,
                      textAlign: 'center',
                      marginBottom: '10px'
                    }}>
                      {user.email}
                    </div>
                    {user.bio && (
                      <div style={{
                        fontSize: '13px',
                        opacity: 0.8,
                        textAlign: 'center',
                        marginTop: '8px',
                        fontStyle: 'italic'
                      }}>
                        {user.bio.length > 50 ? `${user.bio.substring(0, 50)}...` : user.bio}
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

          {results.places.length > 0 && (
            <div style={{ marginBottom: '30px' }}>
              <h3>Yerler ({results.places.length})</h3>
              <div className="places-list">
                {results.places.map((place) => (
                  <div key={place.id} className="place-card">
                    <div className="place-name">{place.name}</div>
                    <div className="place-location">{place.city} {place.district && `- ${place.district}`}</div>
                  </div>
                ))}
              </div>
            </div>
          )}

          {results.posts.length > 0 && (
            <div>
              <h3>Gönderiler ({results.posts.length})</h3>
              <div className="posts-grid">
                {results.posts.map((post) => (
                  <div key={post.id} className="post-card">
                    {post.mediaUrl && (
                      <img src={`http://localhost:5280${post.mediaUrl}`} alt={post.caption} className="post-image" />
                    )}
                    {post.caption && <div className="post-caption">{post.caption}</div>}
                  </div>
                ))}
              </div>
            </div>
          )}
        </div>
      )}

      {activeTab === 'posts' && postResults.length > 0 && (
        <div>
          <h3>Gönderiler ({postResults.length})</h3>
          <div className="posts-grid">
            {postResults.map((post) => (
              <div key={post.id} className="post-card">
                {post.mediaUrl && (
                  <img src={`http://localhost:5280${post.mediaUrl}`} alt={post.caption} className="post-image" />
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
    </div>
  );
}

