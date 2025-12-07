import { useState, useEffect, useRef } from 'react';
import { MapContainer, TileLayer, Marker, Popup, useMapEvents, useMap } from 'react-leaflet';
import 'leaflet/dist/leaflet.css';
import L from 'leaflet';
import { api } from '../services/api';
import PostDetailModal from './PostDetailModal';

// Leaflet marker icon fix
delete L.Icon.Default.prototype._getIconUrl;
L.Icon.Default.mergeOptions({
  iconRetinaUrl: 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.7.1/images/marker-icon-2x.png',
  iconUrl: 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.7.1/images/marker-icon.png',
  shadowUrl: 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.7.1/images/marker-shadow.png',
});

// Cluster marker icon oluştur
function createClusterIcon(count, isCluster) {
  const size = isCluster ? Math.min(50, 30 + count * 2) : 30;
  const color = isCluster ? '#e74c3c' : '#3498db';
  
  return L.divIcon({
    className: 'custom-cluster-icon',
    html: `<div style="
      background: ${color};
      width: ${size}px;
      height: ${size}px;
      border-radius: 50%;
      border: 3px solid white;
      box-shadow: 0 2px 8px rgba(0,0,0,0.3);
      display: flex;
      align-items: center;
      justify-content: center;
      color: white;
      font-weight: bold;
      font-size: ${size > 40 ? '14px' : '12px'};
    ">${count}</div>`,
    iconSize: [size, size],
    iconAnchor: [size / 2, size / 2]
  });
}

function MapClustersLayer({ clusters, onClusterClick, currentZoom }) {
  return (
    <>
      {clusters.map((cluster, index) => {
        const isCluster = cluster.isCluster !== false && cluster.postsCount > 1;
        const icon = createClusterIcon(cluster.postsCount, isCluster);
        
        return (
          <Marker
            key={cluster.placeId ?? `cluster-${cluster.latitude}-${cluster.longitude}-${index}`}
            position={[cluster.latitude, cluster.longitude]}
            icon={icon}
            eventHandlers={{
              click: () => onClusterClick && onClusterClick(cluster),
            }}
          >
            <Popup>
              <div>
                {cluster.placeName ? (
                  <>
                    <strong>{cluster.placeName}</strong>
                    {cluster.city && <><br /><span>{cluster.city}</span></>}
                  </>
                ) : (
                  <strong>Bölge</strong>
                )}
                <br />
                <small>{cluster.postsCount} {cluster.postsCount === 1 ? 'gönderi' : 'gönderi'}</small>
                {cluster.samplePostIds && cluster.samplePostIds.length > 0 && (
                  <><br /><small style={{ color: '#7f8c8d' }}>Tıklayarak detayları görüntüleyin</small></>
                )}
              </div>
            </Popup>
          </Marker>
        );
      })}
    </>
  );
}

// Map event handler component
function MapEventHandler({ onBoundsChange, onZoomChange }) {
  const map = useMap();
  const lastZoomRef = useRef(map.getZoom());

  useEffect(() => {
    const updateBounds = () => {
      const bounds = map.getBounds();
      const zoom = map.getZoom();
      
      // Zoom değiştiyse callback'i çağır
      if (zoom !== lastZoomRef.current) {
        lastZoomRef.current = zoom;
        onZoomChange?.(zoom);
      }
      
      onBoundsChange({
        north: bounds.getNorth(),
        south: bounds.getSouth(),
        east: bounds.getEast(),
        west: bounds.getWest(),
      }, zoom);
    };

    // İlk yükleme
    updateBounds();

    // Event listener'ları ekle
    map.on('moveend', updateBounds);
    map.on('zoomend', updateBounds);

    return () => {
      map.off('moveend', updateBounds);
      map.off('zoomend', updateBounds);
    };
  }, [map, onBoundsChange, onZoomChange]);

  return null;
}

export default function MapView({ user, onUserClick }) {
  const [clusters, setClusters] = useState([]);
  const [loading, setLoading] = useState(true);
  const [center, setCenter] = useState([39.9334, 32.8597]); // Ankara default
  const [userLocation, setUserLocation] = useState(null);
  const [selectedPost, setSelectedPost] = useState(null);
  const [selectedCluster, setSelectedCluster] = useState(null);
  const [selectedClusterPosts, setSelectedClusterPosts] = useState([]);
  const [loadingPosts, setLoadingPosts] = useState(false);
  const [currentZoom, setCurrentZoom] = useState(6);
  const loadTimeoutRef = useRef(null);

  useEffect(() => {
    // İlk yüklemede merkez ve kullanıcı konumunu al
    if (navigator.geolocation) {
      navigator.geolocation.getCurrentPosition(
        (position) => {
          const location = [position.coords.latitude, position.coords.longitude];
          setCenter(location);
          setUserLocation(location);
        },
        () => {
          console.log('Konum alınamadı, varsayılan konum kullanılıyor.');
        }
      );
    }
  }, []);

  const loadClusters = async (mapBounds, zoom) => {
    // Debounce: çok sık istek göndermeyi önle
    if (loadTimeoutRef.current) {
      clearTimeout(loadTimeoutRef.current);
    }

    loadTimeoutRef.current = setTimeout(async () => {
      try {
        setLoading(true);
        const data = await api.getMapClusters(mapBounds, zoom);
        setClusters(data || []);
      } catch (err) {
        console.error('Harita cluster verisi yüklenemedi:', err);
      } finally {
        setLoading(false);
      }
    }, 300); // 300ms debounce
  };

  const handleBoundsChange = (bounds, zoom) => {
    setCurrentZoom(zoom);
    loadClusters(bounds, zoom);
  };

  const handleZoomChange = (zoom) => {
    setCurrentZoom(zoom);
  };

  const handleClusterClick = async (cluster) => {
    setSelectedCluster(cluster);
    setLoadingPosts(true);
    
    try {
      // Eğer samplePostIds varsa, o postları yükle
      if (cluster.samplePostIds && cluster.samplePostIds.length > 0) {
        const posts = await Promise.all(
          cluster.samplePostIds.map(id => api.getPostById(id))
        );
        setSelectedClusterPosts(posts.filter(p => p != null));
      } 
      // Eğer placeId varsa, o yerin tüm gönderilerini yükle
      else if (cluster.placeId) {
        const posts = await api.getPostsByPlace(cluster.placeId);
        setSelectedClusterPosts(posts || []);
      } else {
        setSelectedClusterPosts([]);
      }
    } catch (err) {
      console.error('Gönderiler yüklenemedi:', err);
      setSelectedClusterPosts([]);
    } finally {
      setLoadingPosts(false);
    }
  };

  return (
    <div>
      <h2>Yerler Haritası</h2>
      <div style={{ marginBottom: '10px', color: '#7f8c8d', fontSize: '14px' }}>
        Zoom seviyesi: {currentZoom} • {clusters.length} {clusters.length === 1 ? 'cluster' : 'cluster'} gösteriliyor
      </div>
      <div style={{ height: '600px', width: '100%', marginTop: '10px', borderRadius: '8px', overflow: 'hidden', border: '1px solid #ddd' }}>
        <MapContainer
          center={center}
          zoom={currentZoom}
          style={{ height: '100%', width: '100%' }}
        >
          <TileLayer
            url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
            attribution='&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
          />
          <MapEventHandler 
            onBoundsChange={handleBoundsChange}
            onZoomChange={handleZoomChange}
          />
          {userLocation && (
            <Marker
              position={userLocation}
              icon={L.divIcon({
                className: 'custom-user-marker',
                html: '<div style="background: #3498db; width: 20px; height: 20px; border-radius: 50%; border: 3px solid white; box-shadow: 0 2px 4px rgba(0,0,0,0.3);"></div>',
                iconSize: [20, 20],
                iconAnchor: [10, 10]
              })}
            >
              <Popup>
                <div>
                  <strong>📍 Konumunuz</strong>
                  {user && (
                    <>
                      <br />
                      <small>{user.username}</small>
                    </>
                  )}
                </div>
              </Popup>
            </Marker>
          )}
          {!loading && (
            <MapClustersLayer 
              clusters={clusters} 
              onClusterClick={handleClusterClick}
              currentZoom={currentZoom}
            />
          )}
        </MapContainer>
      </div>
      {loading && (
        <div style={{ marginTop: '10px', textAlign: 'center', color: '#7f8c8d' }}>
          Yükleniyor...
        </div>
      )}
      {selectedCluster && (
        <div style={{ marginTop: '20px', padding: '15px', backgroundColor: '#f8f9fa', borderRadius: '8px' }}>
          <h3>
            {selectedCluster.placeName || 'Bölge'} 
            {selectedCluster.city && ` - ${selectedCluster.city}`}
            {' '}
            <span style={{ fontSize: '14px', color: '#7f8c8d' }}>
              ({selectedCluster.postsCount} {selectedCluster.postsCount === 1 ? 'gönderi' : 'gönderi'})
            </span>
          </h3>
          {loadingPosts ? (
            <div className="loading">Gönderiler yükleniyor...</div>
          ) : selectedClusterPosts.length === 0 ? (
            <div style={{ padding: '20px', color: '#7f8c8d' }}>Bu yerde henüz gönderi yok.</div>
          ) : (
            <div className="posts-grid" style={{ marginTop: '15px' }}>
              {selectedClusterPosts.map((post) => (
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
          user={user}
          onClose={() => setSelectedPost(null)}
          onUserClick={onUserClick}
          onLike={async (postId, isLiked) => {
            // Post listesini güncelle
            setSelectedClusterPosts(prev =>
              prev.map(p =>
                p.id === postId
                  ? { ...p, likesCount: isLiked ? p.likesCount + 1 : Math.max(0, p.likesCount - 1) }
                  : p
              )
            );
          }}
        />
      )}
    </div>
  );
}

