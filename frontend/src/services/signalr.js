import * as signalR from '@microsoft/signalr';

let connection = null;
let currentUserId = null;

export const getConnection = (userId) => {
  currentUserId = userId;

  if (connection && connection.state === signalR.HubConnectionState.Connected) {
    return connection;
  }

  if (connection && connection.state === signalR.HubConnectionState.Connecting) {
    return connection;
  }

  // Yeni bağlantı oluştur
  connection = new signalR.HubConnectionBuilder()
    .withUrl('http://localhost:5280/notificationHub')
    .withAutomaticReconnect()
    .configureLogging(signalR.LogLevel.Information)
    .build();

  // Bağlantı koptuktan sonra tekrar bağlanıldığında çalışır
  connection.onreconnected(() => {
    console.log('SignalR reconnected. Rejoining group...');
    if (currentUserId) {
      connection.invoke('JoinUserGroup', currentUserId)
        .catch(err => console.error('Error rejoining group:', err));
    }
  });

  // Bağlantıyı başlat
  startConnection();

  return connection;
};

const startConnection = async () => {
  try {
    if (connection && connection.state === signalR.HubConnectionState.Disconnected) {
      await connection.start();
      console.log('SignalR Connected.');

      if (currentUserId) {
        await connection.invoke('JoinUserGroup', currentUserId);
        console.log('Joined user group:', currentUserId);
      }
    }
  } catch (err) {
    console.error('SignalR Connection Error:', err);
    // Basit bir retry mekanizması (opsiyonel, automaticReconnect zaten var ama ilk bağlantı için)
    setTimeout(startConnection, 5000);
  }
};

export const disconnect = async () => {
  if (connection) {
    try {
      await connection.stop();
      console.log('SignalR Disconnected.');
    } catch (err) {
      console.error('Error stopping SignalR connection:', err);
    } finally {
      connection = null;
      currentUserId = null;
    }
  }
};

