import { useState, useEffect } from 'react';
import { api } from '../services/api';

export default function Users({ onUserClick }) {
  const [users, setUsers] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [showForm, setShowForm] = useState(false);
  const [formData, setFormData] = useState({
    username: '',
    email: '',
    password: '',
  });

  useEffect(() => {
    loadUsers();
  }, []);

  const loadUsers = async () => {
    try {
      setLoading(true);
      const data = await api.getUsers();
      setUsers(data);
      setError(null);
    } catch (err) {
      setError('Kullanıcılar yüklenirken hata oluştu.');
    } finally {
      setLoading(false);
    }
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    try {
      await api.createUser(formData);
      setShowForm(false);
      setFormData({ username: '', email: '', password: '' });
      loadUsers();
    } catch (err) {
      setError('Kullanıcı oluşturulurken hata oluştu.');
    }
  };

  if (loading) {
    return <div className="loading">Yükleniyor...</div>;
  }

  return (
    <div>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '20px' }}>
        <h2>Kullanıcılar</h2>
        <button className="btn btn-primary" onClick={() => setShowForm(!showForm)}>
          {showForm ? 'İptal' : 'Yeni Kullanıcı'}
        </button>
      </div>

      {error && <div className="error">{error}</div>}

      {showForm && (
        <form className="form" onSubmit={handleSubmit} style={{ marginBottom: '30px' }}>
          <div className="form-group">
            <label>Kullanıcı Adı</label>
            <input
              type="text"
              value={formData.username}
              onChange={(e) => setFormData({ ...formData, username: e.target.value })}
              required
            />
          </div>
          <div className="form-group">
            <label>E-posta</label>
            <input
              type="email"
              value={formData.email}
              onChange={(e) => setFormData({ ...formData, email: e.target.value })}
              required
            />
          </div>
          <div className="form-group">
            <label>Şifre</label>
            <input
              type="password"
              value={formData.password}
              onChange={(e) => setFormData({ ...formData, password: e.target.value })}
              required
            />
          </div>
          <button type="submit" className="btn btn-primary">Oluştur</button>
        </form>
      )}

      <div className="users-list">
        {users.map((user) => (
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
  );
}

