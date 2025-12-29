import { useState } from 'react';
import { api } from '../services/api';

export default function Login({ onLogin }) {
  const [isLogin, setIsLogin] = useState(true);
  const [formData, setFormData] = useState({
    username: '',
    email: '',
    password: '',
  });
  const [error, setError] = useState('');

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');

    try {
      if (isLogin) {
        // Login
        const response = await api.login({
          username: formData.username,
          password: formData.password,
        });
        localStorage.setItem('token', response.token);
        localStorage.setItem('user', JSON.stringify(response.user));
        onLogin(response.user);
      } else {
        // Register
        const response = await api.register({
          username: formData.username,
          email: formData.email,
          password: formData.password,
        });
        localStorage.setItem('token', response.token);
        localStorage.setItem('user', JSON.stringify(response.user));
        onLogin(response.user);
      }
    } catch (err) {
      setError(isLogin ? 'Giriş başarısız. Kullanıcı adı veya şifre hatalı.' : 'Kayıt başarısız. Lütfen bilgilerinizi kontrol edin.');
    }
  };

  const inputStyle = {
    width: '100%',
    padding: '12px',
    borderRadius: '8px',
    border: '1px solid #444',
    background: '#2d2d2d',
    color: '#fff',
    fontSize: '14px',
    outline: 'none',
    transition: 'border-color 0.2s',
    marginTop: '6px'
  };

  const labelStyle = {
    display: 'block',
    fontSize: '14px',
    fontWeight: '500',
    color: '#bbb',
    marginBottom: '4px'
  };

  return (
    <div style={{
      maxWidth: '400px',
      margin: '60px auto',
      padding: '40px',
      background: '#1a1a1a',
      borderRadius: '16px',
      boxShadow: '0 8px 32px rgba(0,0,0,0.4)',
      border: '1px solid #333'
    }}>
      <h2 style={{ textAlign: 'center', marginBottom: '30px', color: '#fff', fontWeight: '600' }}>
        {isLogin ? 'Giriş Yap' : 'Kayıt Ol'}
      </h2>

      {error && (
        <div style={{
          marginBottom: '20px',
          padding: '12px',
          background: 'rgba(231, 76, 60, 0.2)',
          border: '1px solid #e74c3c',
          borderRadius: '8px',
          color: '#ff6b6b',
          fontSize: '14px',
          textAlign: 'center'
        }}>
          {error}
        </div>
      )}

      <form className="form" onSubmit={handleSubmit}>
        <div style={{ marginBottom: '20px' }}>
          <label style={labelStyle}>Kullanıcı Adı</label>
          <input
            type="text"
            value={formData.username}
            onChange={(e) => setFormData({ ...formData, username: e.target.value })}
            style={inputStyle}
            onFocus={(e) => e.target.style.borderColor = '#3498db'}
            onBlur={(e) => e.target.style.borderColor = '#444'}
            placeholder="Kullanıcı adınız"
            required
          />
        </div>

        {!isLogin && (
          <div style={{ marginBottom: '20px' }}>
            <label style={labelStyle}>E-posta</label>
            <input
              type="email"
              value={formData.email}
              onChange={(e) => setFormData({ ...formData, email: e.target.value })}
              style={inputStyle}
              onFocus={(e) => e.target.style.borderColor = '#3498db'}
              onBlur={(e) => e.target.style.borderColor = '#444'}
              placeholder="E-posta adresiniz"
              required
            />
          </div>
        )}

        <div style={{ marginBottom: '25px' }}>
          <label style={labelStyle}>Şifre</label>
          <input
            type="password"
            value={formData.password}
            onChange={(e) => setFormData({ ...formData, password: e.target.value })}
            style={inputStyle}
            onFocus={(e) => e.target.style.borderColor = '#3498db'}
            onBlur={(e) => e.target.style.borderColor = '#444'}
            placeholder="••••••••"
            required
          />
        </div>

        <button
          type="submit"
          style={{
            width: '100%',
            padding: '14px',
            borderRadius: '8px',
            border: 'none',
            background: 'linear-gradient(135deg, #3498db, #2980b9)',
            color: 'white',
            fontSize: '16px',
            fontWeight: '600',
            cursor: 'pointer',
            transition: 'opacity 0.2s',
            boxShadow: '0 4px 12px rgba(52, 152, 219, 0.3)'
          }}
          onMouseEnter={(e) => e.currentTarget.style.opacity = '0.9'}
          onMouseLeave={(e) => e.currentTarget.style.opacity = '1'}
        >
          {isLogin ? 'Giriş Yap' : 'Kayıt Ol'}
        </button>
      </form>

      <div style={{ textAlign: 'center', marginTop: '25px' }}>
        <button
          type="button"
          onClick={() => {
            setIsLogin(!isLogin);
            setError('');
            setFormData({ username: '', email: '', password: '' });
          }}
          style={{ background: 'none', border: 'none', color: '#888', cursor: 'pointer', fontSize: '14px' }}
          onMouseEnter={(e) => e.target.style.color = '#ccc'}
          onMouseLeave={(e) => e.target.style.color = '#888'}
        >
          {isLogin ? (
            <span>Hesabınız yok mu? <span style={{ color: '#3498db', fontWeight: '500' }}>Kayıt olun</span></span>
          ) : (
            <span>Zaten hesabınız var mı? <span style={{ color: '#3498db', fontWeight: '500' }}>Giriş yapın</span></span>
          )}
        </button>
      </div>
    </div>
  );
}
