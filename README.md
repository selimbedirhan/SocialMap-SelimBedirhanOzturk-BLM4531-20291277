# 🗺️ SocialMap

**SocialMap**, konum tabanlı sosyal medya uygulamasıdır. Kullanıcıların gönderilerini harita üzerinde görselleştirmesine, yer etiketlemesi yapmasına ve dünyayı keşfetmesine olanak tanır. Instagram benzeri bir arayüzle, gönderilerinizi harita üzerinde paylaşın ve keşfedin!

## 📋 İçindekiler

- [Özellikler](#-özellikler)
- [Teknolojiler](#-teknolojiler)
- [Kurulum](#-kurulum)
- [Kullanım](#-kullanım)
- [Proje Yapısı](#-proje-yapısı)
- [API Dokümantasyonu](#-api-dokümantasyonu)
- [Ekran Görüntüleri](#-ekran-görüntüleri)
- [Katkıda Bulunma](#-katkıda-bulunma)
- [Lisans](#-lisans)

## ✨ Özellikler

### 🎯 Temel Özellikler

- **📍 Instagram Benzeri Yer Etiketleme**: Yer adını yazarak arama yapın veya harita üzerinden doğrudan konum seçin
- **🗺️ İnteraktif Harita Görünümü**: Gönderilerinizi harita üzerinde cluster'lar halinde görüntüleyin
- **📸 Gönderi Paylaşımı**: Fotoğraf ve açıklama ile gönderiler oluşturun
- **❤️ Beğeni Sistemi**: Gönderileri beğenin ve beğenileri görüntüleyin
- **💬 Yorum Sistemi**: Gönderilere yorum yapın ve yorumları görüntüleyin
- **👥 Kullanıcı Takip Sistemi**: Diğer kullanıcıları takip edin ve takipçilerinizi yönetin
- **🔔 Gerçek Zamanlı Bildirimler**: SignalR ile anlık bildirimler alın
- **🔍 Gelişmiş Arama**: Gönderiler, kullanıcılar ve yerler arasında arama yapın
- **👤 Profil Yönetimi**: Profil fotoğrafı ve bio bilgilerinizi güncelleyin

### 🗺️ Harita Özellikleri

- **Geohash Tabanlı Clustering**: Performanslı harita görselleştirmesi için geohash algoritması
- **Zoom Seviyesine Göre Clustering**: Farklı zoom seviyelerinde optimize edilmiş cluster görünümü
- **Konum Bazlı Gönderi Filtreleme**: Belirli bir bölgedeki gönderileri görüntüleyin
- **OpenStreetMap Entegrasyonu**: Ücretsiz ve açık kaynak harita servisi

## 🛠️ Teknolojiler

### Backend
- **.NET 9.0** - Modern C# framework
- **PostgreSQL** - İlişkisel veritabanı
- **Entity Framework Core** - ORM
- **SignalR** - Gerçek zamanlı iletişim
- **Swagger/OpenAPI** - API dokümantasyonu

### Frontend
- **React 19** - UI framework
- **Vite** - Build tool
- **React Router** - Routing
- **Leaflet** - Harita görselleştirme
- **React Leaflet** - React için Leaflet wrapper
- **SignalR Client** - Gerçek zamanlı bildirimler

### Harita ve Konum
- **Geohash** - Konum kodlama algoritması
- **OpenStreetMap** - Harita tile servisi
- **Nominatim API** - Yer arama ve ters geocoding

## 📦 Kurulum

### Gereksinimler

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Node.js](https://nodejs.org/) (v18 veya üzeri)
- [PostgreSQL](https://www.postgresql.org/download/) (v12 veya üzeri)
- npm veya yarn

### 1. Repository'yi Klonlayın

```bash
git clone https://github.com/kullaniciadi/SocialMap.git
cd SocialMap
```

### 2. Veritabanı Kurulumu

PostgreSQL'de yeni bir veritabanı oluşturun:

```sql
CREATE DATABASE SocialMapDB;
```

### 3. Backend Kurulumu

```bash
cd SocialMap.WebAPI
```

`appsettings.Development.json` dosyasında veritabanı bağlantı bilgilerinizi güncelleyin:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=SocialMapDB;Username=postgres;Password=yourpassword"
  }
}
```

Bağımlılıkları yükleyin ve projeyi çalıştırın:

```bash
dotnet restore
dotnet run
```

Backend `http://localhost:5280` adresinde çalışacaktır.

### 4. Frontend Kurulumu

Yeni bir terminal açın:

```bash
cd frontend
npm install
npm run dev
```

Frontend `http://localhost:5173` adresinde çalışacaktır.

## 🚀 Kullanım

### İlk Giriş

1. Uygulamayı açın ve kayıt olun
2. Kullanıcı adı, e-posta ve şifre ile hesap oluşturun
3. Giriş yaptıktan sonra ana sayfaya yönlendirileceksiniz

### Gönderi Paylaşma

1. **"Yeni Gönderi"** butonuna tıklayın
2. **Yer Etiketi** bölümünde:
   - Yer adını yazın (örn: "Anıtkabir", "İstanbul")
   - Arama sonuçlarından birini seçin VEYA
   - **"Haritadan Konum Seç"** butonuna tıklayıp harita üzerinden konum seçin
3. İsteğe bağlı olarak fotoğraf yükleyin
4. Açıklama ekleyin
5. **"Paylaş"** butonuna tıklayın

### Harita Görünümü

1. **"Harita"** sekmesine gidin
2. Harita üzerinde cluster'ları görüntüleyin
3. Zoom yaparak daha detaylı görünüm elde edin
4. Cluster'lara tıklayarak o bölgedeki gönderileri görüntüleyin

### Diğer Kullanıcıları Takip Etme

1. Bir kullanıcının profil sayfasına gidin
2. **"Takip Et"** butonuna tıklayın
3. Takip ettiğiniz kullanıcıların gönderilerini ana sayfada göreceksiniz

## 📁 Proje Yapısı

```
SocialMap/
├── SocialMap.Core/              # Domain entities, DTOs, interfaces
│   ├── Entities/                 # Veritabanı entity'leri
│   ├── DTOs/                     # Data Transfer Objects
│   └── Interfaces/               # Service ve Repository interface'leri
│
├── SocialMap.Repository/         # Veri erişim katmanı
│   ├── Data/                     # DbContext ve migration helper'lar
│   └── Repositories/             # Repository implementasyonları
│
├── SocialMap.Business/            # İş mantığı katmanı
│   ├── Services/                 # Business service'leri
│   └── Utils/                     # Yardımcı sınıflar (GeohashUtil)
│
├── SocialMap.WebAPI/              # API katmanı
│   ├── Controllers/              # API controller'ları
│   ├── Hubs/                     # SignalR hub'ları
│   └── Services/                 # API servisleri
│
└── frontend/                      # React frontend
    ├── src/
    │   ├── components/           # React component'leri
    │   ├── services/             # API servisleri
    │   └── App.jsx               # Ana uygulama component'i
    └── package.json
```

## 📚 API Dokümantasyonu

Backend çalıştığında Swagger UI'ya şu adresten erişebilirsiniz:
```
http://localhost:5280/swagger
```

### Önemli Endpoint'ler

#### Gönderiler
- `GET /api/posts` - Tüm gönderileri listele
- `POST /api/posts` - Yeni gönderi oluştur
- `GET /api/posts/{id}` - Belirli bir gönderiyi getir
- `GET /api/posts/user/{userId}` - Kullanıcının gönderilerini getir

#### Harita
- `GET /api/map/clusters` - Harita cluster'larını getir
  - Query params: `north`, `south`, `east`, `west`, `zoom`

#### Kullanıcılar
- `GET /api/users` - Tüm kullanıcıları listele
- `POST /api/users` - Yeni kullanıcı oluştur
- `GET /api/users/{id}` - Kullanıcı bilgilerini getir

#### Takip
- `POST /api/follows/{followerId}/follow/{followingId}` - Kullanıcıyı takip et
- `DELETE /api/follows/{followerId}/unfollow/{followingId}` - Takibi bırak
- `GET /api/follows/{userId}/followers` - Takipçileri listele
- `GET /api/follows/{userId}/following` - Takip edilenleri listele

#### Bildirimler
- `GET /api/notifications/{userId}` - Kullanıcının bildirimlerini getir
- `PUT /api/notifications/{id}/read` - Bildirimi okundu olarak işaretle

## 🎨 Ekran Görüntüleri

> **Not:** Ekran görüntüleri eklenecek

## 🤝 Katkıda Bulunma

Katkılarınızı bekliyoruz! Lütfen şu adımları izleyin:

1. Fork edin
2. Feature branch oluşturun (`git checkout -b feature/amazing-feature`)
3. Değişikliklerinizi commit edin (`git commit -m 'Add some amazing feature'`)
4. Branch'inizi push edin (`git push origin feature/amazing-feature`)
5. Pull Request oluşturun

## 📝 Lisans

Bu proje [MIT Lisansı](LICENSE) altında lisanslanmıştır.

## 👤 Yazar

**Selim Bedirhan Öztürk**

- GitHub: [@kullaniciadi](https://github.com/kullaniciadi)

## 🙏 Teşekkürler

- [OpenStreetMap](https://www.openstreetmap.org/) - Harita verileri için
- [Nominatim](https://nominatim.org/) - Yer arama API'si için
- [Leaflet](https://leafletjs.com/) - Harita kütüphanesi için

---

⭐ Bu projeyi beğendiyseniz yıldız vermeyi unutmayın!
