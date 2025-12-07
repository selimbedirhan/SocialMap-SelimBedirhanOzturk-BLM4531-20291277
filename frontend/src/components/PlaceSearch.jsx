import { useState, useEffect, useRef } from 'react';
import { MapContainer, TileLayer, Marker, useMapEvents } from 'react-leaflet';
import 'leaflet/dist/leaflet.css';
import L from 'leaflet';

// Leaflet marker icon fix
delete L.Icon.Default.prototype._getIconUrl;
L.Icon.Default.mergeOptions({
  iconRetinaUrl: 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.7.1/images/marker-icon-2x.png',
  iconUrl: 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.7.1/images/marker-icon.png',
  shadowUrl: 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.7.1/images/marker-shadow.png',
});

// Nominatim API ile yer arama
async function searchPlaces(query) {
  if (!query || query.trim().length < 3) {
    return [];
  }

  try {
    const response = await fetch(
      `https://nominatim.openstreetmap.org/search?format=json&q=${encodeURIComponent(query)}&limit=10&addressdetails=1`,
      {
        headers: {
          'User-Agent': 'SocialMap/1.0',
        },
      }
    );
    const data = await response.json();
    return data.map((item) => ({
      id: item.place_id,
      name: item.display_name.split(',')[0], // İlk kısım genelde yer adı
      fullName: item.display_name,
      latitude: parseFloat(item.lat),
      longitude: parseFloat(item.lon),
      city: item.address?.city || item.address?.town || item.address?.village || item.address?.municipality || '',
      country: item.address?.country || '',
      address: item.address || {},
    }));
  } catch (error) {
    console.error('Yer arama hatası:', error);
    return [];
  }
}

// Harita üzerinde tıklama ile konum seçme
function LocationMarker({ position, onPositionChange }) {
  useMapEvents({
    click(e) {
      const { lat, lng } = e.latlng;
      onPositionChange(lat, lng);
    },
  });

  return position ? (
    <Marker
      position={position}
      icon={L.divIcon({
        className: 'custom-location-marker',
        html: '<div style="background: #e74c3c; width: 20px; height: 20px; border-radius: 50%; border: 3px solid white; box-shadow: 0 2px 8px rgba(0,0,0,0.3);"></div>',
        iconSize: [20, 20],
        iconAnchor: [10, 10],
      })}
    />
  ) : null;
}

export default function PlaceSearch({ onPlaceSelect, initialPlace = null }) {
  const [searchQuery, setSearchQuery] = useState('');
  const [searchResults, setSearchResults] = useState([]);
  const [selectedPlace, setSelectedPlace] = useState(initialPlace);
  const [showMap, setShowMap] = useState(false);
  const [mapCenter, setMapCenter] = useState([39.9334, 32.8597]); // Ankara default
  const [mapPosition, setMapPosition] = useState(null);
  const [isSearching, setIsSearching] = useState(false);
  const searchTimeoutRef = useRef(null);

  useEffect(() => {
    if (initialPlace) {
      setSelectedPlace(initialPlace);
      if (initialPlace.latitude && initialPlace.longitude) {
        setMapCenter([initialPlace.latitude, initialPlace.longitude]);
        setMapPosition([initialPlace.latitude, initialPlace.longitude]);
      }
    }
  }, [initialPlace]);

  useEffect(() => {
    // Debounce arama
    if (searchTimeoutRef.current) {
      clearTimeout(searchTimeoutRef.current);
    }

    if (searchQuery.trim().length >= 3) {
      setIsSearching(true);
      searchTimeoutRef.current = setTimeout(async () => {
        const results = await searchPlaces(searchQuery);
        setSearchResults(results);
        setIsSearching(false);
      }, 500);
    } else {
      setSearchResults([]);
      setIsSearching(false);
    }

    return () => {
      if (searchTimeoutRef.current) {
        clearTimeout(searchTimeoutRef.current);
      }
    };
  }, [searchQuery]);

  const handlePlaceSelect = (place) => {
    setSelectedPlace(place);
    setSearchQuery(place.fullName);
    setSearchResults([]);
    setMapCenter([place.latitude, place.longitude]);
    setMapPosition([place.latitude, place.longitude]);
    setShowMap(true);
    onPlaceSelect({
      placeName: place.name,
      fullName: place.fullName,
      latitude: place.latitude,
      longitude: place.longitude,
      city: place.city,
      country: place.country,
    });
  };

  const handleMapClick = (lat, lng) => {
    setMapPosition([lat, lng]);
    // Ters geocoding yaparak yer adını bul
    fetch(
      `https://nominatim.openstreetmap.org/reverse?format=json&lat=${lat}&lon=${lng}&addressdetails=1`,
      {
        headers: {
          'User-Agent': 'SocialMap/1.0',
        },
      }
    )
      .then((res) => res.json())
      .then((data) => {
        const place = {
          placeName: data.address?.name || data.address?.road || data.display_name.split(',')[0],
          fullName: data.display_name,
          latitude: lat,
          longitude: lng,
          city: data.address?.city || data.address?.town || data.address?.village || data.address?.municipality || '',
          country: data.address?.country || '',
        };
        setSelectedPlace(place);
        setSearchQuery(place.fullName);
        onPlaceSelect(place);
      })
      .catch((error) => {
        console.error('Ters geocoding hatası:', error);
        // Hata durumunda da konumu kaydet
        const place = {
          placeName: `Konum (${lat.toFixed(4)}, ${lng.toFixed(4)})`,
          fullName: `Konum (${lat.toFixed(4)}, ${lng.toFixed(4)})`,
          latitude: lat,
          longitude: lng,
          city: '',
          country: '',
        };
        setSelectedPlace(place);
        setSearchQuery(place.placeName);
        onPlaceSelect(place);
      });
  };

  const handleClear = () => {
    setSelectedPlace(null);
    setSearchQuery('');
    setSearchResults([]);
    setMapPosition(null);
    setShowMap(false);
    onPlaceSelect(null);
  };

  return (
    <div style={{ marginBottom: '20px' }}>
      <div className="form-group">
        <label>
          Yer Etiketi <span style={{ color: 'red' }}>*</span>
        </label>
        <div style={{ position: 'relative' }}>
          <input
            type="text"
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
            placeholder="Yer adı yazın (örn: Anıtkabir, İstanbul)"
            style={{
              width: '100%',
              padding: '10px',
              border: '1px solid #ddd',
              borderRadius: '4px',
              fontSize: '14px',
            }}
          />
          {isSearching && (
            <div
              style={{
                position: 'absolute',
                right: '10px',
                top: '50%',
                transform: 'translateY(-50%)',
                color: '#7f8c8d',
                fontSize: '12px',
              }}
            >
              Aranıyor...
            </div>
          )}
        </div>
        <small style={{ color: '#7f8c8d' }}>
          Yer adını yazın veya harita üzerinden konum seçin
        </small>

        {/* Arama sonuçları */}
        {searchResults.length > 0 && (
          <div
            style={{
              marginTop: '5px',
              border: '1px solid #ddd',
              borderRadius: '4px',
              maxHeight: '200px',
              overflowY: 'auto',
              backgroundColor: 'white',
              boxShadow: '0 2px 8px rgba(0,0,0,0.1)',
            }}
          >
            {searchResults.map((place) => (
              <div
                key={place.id}
                onClick={() => handlePlaceSelect(place)}
                style={{
                  padding: '10px',
                  cursor: 'pointer',
                  borderBottom: '1px solid #eee',
                  transition: 'background-color 0.2s',
                }}
                onMouseEnter={(e) => (e.target.style.backgroundColor = '#f5f5f5')}
                onMouseLeave={(e) => (e.target.style.backgroundColor = 'white')}
              >
                <div style={{ fontWeight: 'bold', color: '#2c3e50' }}>{place.name}</div>
                <div style={{ fontSize: '12px', color: '#7f8c8d', marginTop: '2px' }}>
                  {place.fullName}
                </div>
              </div>
            ))}
          </div>
        )}

        {/* Seçilen yer bilgisi */}
        {selectedPlace && (
          <div
            style={{
              marginTop: '10px',
              padding: '10px',
              backgroundColor: '#e8f5e9',
              borderRadius: '4px',
              border: '1px solid #4caf50',
            }}
          >
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
              <div>
                <div style={{ fontWeight: 'bold', color: '#2c3e50' }}>
                  📍 {selectedPlace.placeName || selectedPlace.fullName}
                </div>
                {selectedPlace.city && (
                  <div style={{ fontSize: '12px', color: '#7f8c8d', marginTop: '2px' }}>
                    {selectedPlace.city}
                    {selectedPlace.country && `, ${selectedPlace.country}`}
                  </div>
                )}
              </div>
              <button
                type="button"
                onClick={handleClear}
                style={{
                  padding: '5px 10px',
                  backgroundColor: '#e74c3c',
                  color: 'white',
                  border: 'none',
                  borderRadius: '4px',
                  cursor: 'pointer',
                  fontSize: '12px',
                }}
              >
                Temizle
              </button>
            </div>
          </div>
        )}

        {/* Harita göster/gizle butonu */}
        <button
          type="button"
          onClick={() => setShowMap(!showMap)}
          style={{
            marginTop: '10px',
            padding: '8px 15px',
            backgroundColor: '#3498db',
            color: 'white',
            border: 'none',
            borderRadius: '4px',
            cursor: 'pointer',
            fontSize: '14px',
          }}
        >
          {showMap ? '📍 Haritayı Gizle' : '📍 Haritadan Konum Seç'}
        </button>

        {/* Harita */}
        {showMap && (
          <div
            style={{
              marginTop: '10px',
              height: '300px',
              width: '100%',
              borderRadius: '8px',
              overflow: 'hidden',
              border: '1px solid #ddd',
            }}
          >
            <MapContainer
              center={mapCenter}
              zoom={13}
              style={{ height: '100%', width: '100%' }}
            >
              <TileLayer
                url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
                attribution='&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
              />
              <LocationMarker position={mapPosition} onPositionChange={handleMapClick} />
            </MapContainer>
          </div>
        )}
      </div>
    </div>
  );
}
