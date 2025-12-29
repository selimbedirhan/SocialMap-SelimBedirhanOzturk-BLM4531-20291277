import { useState, useEffect } from 'react';
import { api } from '../services/api';
import { getConnection } from '../services/signalr';

export default function Notifications({ userId }) {
  const [notifications, setNotifications] = useState([]);
  const [unreadCount, setUnreadCount] = useState(0);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    if (userId) {
      loadNotifications();
      loadUnreadCount();

      // SignalR bağlantısı kur ve bildirimleri dinle
      const connection = getConnection(userId);

      const handleNewNotification = (notification) => {
        console.log('Yeni bildirim alındı:', notification);

        // Bildirimi API formatına uygun hale getir
        const formattedNotification = {
          id: notification.id,
          type: notification.type,
          message: notification.message,
          relatedPostId: notification.relatedPostId,
          relatedUserId: notification.relatedUserId,
          isRead: notification.isRead || false,
          createdAt: notification.createdAt
        };

        // Duplicate kontrolü - aynı bildirim zaten listede varsa ekleme
        setNotifications(prev => {
          const exists = prev.some(n => n.id === formattedNotification.id);
          if (exists) {
            return prev; // Zaten listede varsa değişiklik yapma
          }
          // Yeni bildirimi listenin başına ekle
          return [formattedNotification, ...prev];
        });

        // Okunmamış sayısını artır (sadece okunmamışsa)
        if (!formattedNotification.isRead) {
          setUnreadCount(prev => prev + 1);
        }
      };

      connection.on('ReceiveNotification', handleNewNotification);

      return () => {
        connection.off('ReceiveNotification', handleNewNotification);
      };
    }
  }, [userId]);

  const loadNotifications = async () => {
    try {
      const data = await api.getNotifications(userId);
      setNotifications(data);
    } catch (err) {
      console.error('Bildirimler yüklenemedi:', err);
    } finally {
      setLoading(false);
    }
  };

  const loadUnreadCount = async () => {
    try {
      const count = await api.getUnreadCount(userId);
      setUnreadCount(count);
    } catch (err) {
      console.error('Okunmamış sayısı yüklenemedi:', err);
    }
  };

  const handleMarkAsRead = async (notificationId) => {
    try {
      await api.markAsRead(notificationId);
      // State'i güncelle - API'yi tekrar çağırmak yerine direkt güncelle
      setNotifications(prev =>
        prev.map(n => n.id === notificationId ? { ...n, isRead: true } : n)
      );
      setUnreadCount(prev => Math.max(0, prev - 1));
    } catch (err) {
      console.error('Bildirim okundu olarak işaretlenemedi:', err);
    }
  };

  const handleMarkAllAsRead = async () => {
    try {
      await api.markAllAsRead(userId);
      // State'i güncelle - API'yi tekrar çağırmak yerine direkt güncelle
      setNotifications(prev =>
        prev.map(n => ({ ...n, isRead: true }))
      );
      setUnreadCount(0);
    } catch (err) {
      console.error('Tüm bildirimler okundu olarak işaretlenemedi:', err);
    }
  };

  const getNotificationIcon = (type) => {
    switch (type) {
      case 'like': return '❤️';
      case 'comment': return '💬';
      case 'follow': return '👤';
      default: return '🔔';
    }
  };

  if (loading) {
    return <div className="loading">Yükleniyor...</div>;
  }

  return (
    <div>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '20px' }}>
        <h2>Bildirimler {unreadCount > 0 && <span style={{ background: '#e74c3c', color: 'white', padding: '2px 8px', borderRadius: '10px', fontSize: '14px' }}>{unreadCount}</span>}</h2>
        {unreadCount > 0 && (
          <button className="btn btn-secondary" onClick={handleMarkAllAsRead}>
            Tümünü Okundu İşaretle
          </button>
        )}
      </div>

      {notifications.length === 0 ? (
        <div style={{ textAlign: 'center', padding: '40px', color: '#7f8c8d' }}>
          Bildirim yok
        </div>
      ) : (
        <div>
          {notifications.map((notification) => (
            <div
              key={notification.id}
              className={`notification-card ${!notification.isRead ? 'unread' : ''}`}
              onClick={() => !notification.isRead && handleMarkAsRead(notification.id)}
            >
              <div className="notification-icon">
                {getNotificationIcon(notification.type)}
              </div>
              <div style={{ flex: 1 }}>
                <div className="notification-message">{notification.message}</div>
                <div className="notification-time">
                  {new Date(notification.createdAt).toLocaleString('tr-TR')}
                </div>
              </div>
              {!notification.isRead && (
                <div className="unread-dot"></div>
              )}
            </div>
          ))}
        </div>
      )}
    </div>
  );
}

