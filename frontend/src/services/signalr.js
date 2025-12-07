import * as signalR from '@microsoft/signalr';

let connection = null;

export const getConnection = (userId) => {
  if (connection && connection.state === signalR.HubConnectionState.Connected) {
    return connection;
  }

  connection = new signalR.HubConnectionBuilder()
    .withUrl('http://localhost:5280/notificationHub')
    .withAutomaticReconnect()
    .build();

  // Kullanıcı grubuna katıl
  connection.start()
    .then(() => {
      if (userId) {
        connection.invoke('JoinUserGroup', userId);
      }
    })
    .catch(err => console.error('SignalR connection error:', err));

  return connection;
};

export const disconnect = () => {
  if (connection) {
    connection.stop();
    connection = null;
  }
};

