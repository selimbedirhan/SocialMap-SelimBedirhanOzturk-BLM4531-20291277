import { useState, useEffect } from 'react';
import './App.css';
import Login from './components/Login';
import Home from './components/Home';
import MyProfile from './components/MyProfile';
import Profile from './components/Profile';
import Notifications from './components/Notifications';
import Search from './components/Search';
import MapView from './components/MapView';
import Places from './components/Places';
import { api } from './services/api';
import { getConnection, disconnect } from './services/signalr';

function App() {
  const [user, setUser] = useState(null);
  const [activeTab, setActiveTab] = useState('home');
  const [selectedUserId, setSelectedUserId] = useState(null);
  const [unreadCount, setUnreadCount] = useState(0);

  useEffect(() => {
    // LocalStorage'dan kullanıcı bilgisini yükle
    const savedUser = localStorage.getItem('user');
    if (savedUser) {
      try {
        setUser(JSON.parse(savedUser));
      } catch (err) {
        console.error('Kullanıcı bilgisi yüklenemedi:', err);
      }
    }
  }, []);

  useEffect(() => {
    if (user?.id) {
      loadUnreadCount();
      const interval = setInterval(() => {
        loadUnreadCount();
      }, 10000);

      // SignalR bağlantısını kur
      const connection = getConnection(user.id);
      
      // Bildirim dinleyicisi - sadece sayıyı güncelle, liste güncellemesi Notifications component'inde yapılacak
      const handleNotification = (notification) => {
        console.log('Yeni bildirim:', notification);
        // Bildirim sayısını güncelle
        loadUnreadCount();
      };
      
      connection.on('ReceiveNotification', handleNotification);

      return () => {
        clearInterval(interval);
        connection.off('ReceiveNotification', handleNotification);
        // SignalR bağlantısını kapatma - Notifications component'inde kullanılıyor
      };

      return () => {
        clearInterval(interval);
        // SignalR bağlantısını kapatma - Notifications component'inde kullanılıyor
      };
    } else {
      disconnect();
    }
  }, [user]);

  const loadUnreadCount = async () => {
    if (!user?.id) return;
    try {
      const count = await api.getUnreadCount(user.id);
      setUnreadCount(count);
    } catch (err) {
      console.error('Okunmamış sayısı yüklenemedi:', err);
    }
  };

  const handleLogin = (userData) => {
    setUser(userData);
    setActiveTab('home');
  };

  const handleLogout = () => {
    localStorage.removeItem('token');
    localStorage.removeItem('user');
    setUser(null);
    setActiveTab('home');
  };

  const handleUserClick = (userId) => {
    setSelectedUserId(userId);
    setActiveTab('profile');
  };

  // Kullanıcı giriş yapmamışsa login sayfasını göster
  if (!user) {
    return (
      <div className="app">
        <div className="header">
          <h1>SocialMap</h1>
          <p>Yerlerinizi keşfedin, paylaşın ve keşfedin</p>
        </div>
        <div className="content">
          <Login onLogin={handleLogin} />
        </div>
      </div>
    );
  }

  return (
    <div className="app">
      <div className="header">
        <h1>SocialMap</h1>
        <p>Yerlerinizi keşfedin, paylaşın ve keşfedin</p>
        <nav className="nav">
          <button
            className={activeTab === 'home' ? 'active' : ''}
            onClick={() => { setActiveTab('home'); setSelectedUserId(null); }}
          >
            🏠 Anasayfa
          </button>
          <button
            className={activeTab === 'search' ? 'active' : ''}
            onClick={() => { setActiveTab('search'); setSelectedUserId(null); }}
          >
            🔍 Arama
          </button>
          <button
            className={activeTab === 'places' ? 'active' : ''}
            onClick={() => { setActiveTab('places'); setSelectedUserId(null); }}
          >
            📍 Yerler
          </button>
          <button
            className={activeTab === 'map' ? 'active' : ''}
            onClick={() => { setActiveTab('map'); setSelectedUserId(null); }}
          >
            🗺️ Harita
          </button>
          <button
            className={activeTab === 'notifications' ? 'active' : ''}
            onClick={() => { setActiveTab('notifications'); setSelectedUserId(null); }}
          >
            🔔 Bildirimler {unreadCount > 0 && <span style={{ background: '#e74c3c', color: 'white', padding: '2px 6px', borderRadius: '10px', fontSize: '12px', marginLeft: '5px' }}>{unreadCount}</span>}
          </button>
          <button
            className={activeTab === 'myprofile' ? 'active' : ''}
            onClick={() => { setActiveTab('myprofile'); setSelectedUserId(null); }}
          >
            👤 Profilim
          </button>
          <button
            className="btn btn-secondary"
            onClick={handleLogout}
            style={{ marginLeft: 'auto' }}
          >
            Çıkış Yap
          </button>
        </nav>
      </div>

      <div className="content">
        {activeTab === 'home' && <Home user={user} onUserClick={handleUserClick} />}
        {activeTab === 'search' && <Search onUserClick={handleUserClick} />}
        {activeTab === 'places' && <Places />}
        {activeTab === 'map' && <MapView user={user} onUserClick={handleUserClick} />}
        {activeTab === 'notifications' && <Notifications userId={user.id} />}
        {activeTab === 'myprofile' && <MyProfile user={user} onUserClick={handleUserClick} />}
        {activeTab === 'profile' && selectedUserId && <Profile userId={selectedUserId} currentUserId={user.id} onUserClick={handleUserClick} />}
      </div>
    </div>
  );
}

export default App;
