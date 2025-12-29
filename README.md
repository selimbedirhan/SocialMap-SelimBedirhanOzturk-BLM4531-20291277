# 🗺️ SocialMap

**SocialMap**, konum tabanlı, modern ve interaktif bir sosyal medya platformudur. Kullanıcıların anılarını harita üzerinde ölümsüzleştirmesine, dünyayı keşfetmesine ve diğer kullanıcılarla etkileşime girmesine olanak tanır. Instagram benzeri kullanıcı deneyimini, gelişmiş harita özellikleriyle birleştirir.

![SocialMap Banner](https://via.placeholder.com/1200x400.png?text=SocialMap+Project)

## 🌟 Proje Durumu

Bu proje aşağıdaki fazları başarıyla tamamlamıştır:

- ✅ **Faz 1: Güvenlik** (JWT, Rate Limiting, HTTPS, Secure Headers)
- ✅ **Faz 2: Mimari** (Clean Architecture, UnitOfWork, Serilog, CQRS altyapısı)
- ✅ **Faz 3: Özellikler** (Pagination, Admin Paneli, Raporlama Sistemi)
- ✅ **Faz 4: DevOps** (Docker, Docker Compose, Health Checks, CI/CD)
- ✅ **Faz 5: Test** (Kapsamlı Unit Testler)
- ✅ **Faz 6: Yeni Özellikler** (Hashtag Sistemi, Koleksiyonlar/Favoriler)

---

## ✨ Özellikler

### 👤 Kullanıcı İşlemleri
- **Güvenli Kimlik Doğrulama**: JWT tabanlı kayıt ve giriş
- **Profil Yönetimi**: Biyografi, profil fotoğrafı ve kişisel ayarlar
- **Takip Sistemi**: Takip etme, takibi bırakma ve takipçi/takip edilen listeleri

### 📸 Sosyal Etkileşim
- **Gönderi Paylaşımı**: Fotoğraf, açıklama ve konum etiketi ile gönderi oluşturma
- **İnteraktif Akış**: Takip edilenlerin ve popüler gönderilerin akışı
- **Beğeni ve Yorum**: Gönderilere etkileşim verme
- **Hashtag Sistemi**: `#etiket` ile gönderileri kategorize etme ve arama
- **Koleksiyonlar**: Gönderileri favorilere kaydetme ve saklama
- **Bildirimler**: Anlık etkileşim bildirimleri (SignalR)

### 🗺️ Harita ve Keşfet
- **Konum Bazlı Gönderiler**: Harita üzerinde gönderileri görüntüleme
- **Akıllı Clustering**: Yoğun bölgelerdeki gönderileri gruplama (Geohash)
- **Yer Arama**: Nominatim API ile detaylı yer ve mekan arama
- **Yakınındakiler**: Konumunuza yakın gönderileri keşfetme

### 🛡️ Yönetim ve Güvenlik
- **Admin Paneli**: Kullanıcı, gönderi ve rapor yönetimi
- **Raporlama Sistemi**: Uygunsuz içerikleri raporlama ve moderasyon
- **Yasaklama (Ban)**: Kural ihlali yapan kullanıcıları engelleme
- **Güvenlik Önlemleri**: XSS koruması, Rate Limiting, IP bloklama

---

## 🛠️ Teknolojiler

### Backend (.NET 9.0)
- **Mimari**: Clean Architecture (Core, Repository, Business, WebAPI)
- **Veritabanı**: PostgreSQL 16 (Entity Framework Core)
- **API**: ASP.NET Core Web API
- **Gerçek Zamanlı**: SignalR
- **Loglama**: Serilog (Dosya ve Konsol)
- **Validasyon**: FluentValidation
- **Mapping**: AutoMapper
- **Test**: xUnit, Moq, FluentAssertions

### Frontend (React 19)
- **Build Tool**: Vite
- **Harita**: Leaflet & React Leaflet
- **Styling**: Modern CSS3, Glassmorphism UI, Dark Mode
- **State**: React Hooks & Context API
- **Routing**: React Router v7

### DevOps & Altyapı
- **Container**: Docker & Docker Compose (Multi-stage builds)
- **Web Server**: Nginx (Frontend & Reverse Proxy)
- **pipeline**: GitHub Actions (CI/CD)
- **Health Checks**: Database ve API durum kontrolü

---

## 📦 Kurulum ve Çalıştırma

### Ön Gereksinimler
- Docker Desktop
- Git

### 🚀 Hızlı Başlangıç (Docker ile)

En kolay kurulum yöntemidir. Tüm servisler (API, Frontend, Database) otomatik olarak ayağa kalkar.

1. **Projeyi klonlayın:**
   ```bash
   git clone https://github.com/kullaniciadi/SocialMap.git
   cd SocialMap
   ```

2. **Uygulamayı başlatın:**
   ```bash
   docker-compose up -d --build
   ```

3. **Erişim:**
   - Frontend: `http://localhost:80`
   - Backend API: `http://localhost:5280`
   - Swagger UI: `http://localhost:5280/swagger`
   - Health Check: `http://localhost:5280/health`

### 💻 Lokal Geliştirme Ortamı

Eğer Docker kullanmadan geliştirmek isterseniz:

**Backend:**
```bash
cd SocialMap.WebAPI
# appsettings.Development.json dosyasındaki DB bağlantısını düzenleyin
dotnet restore
dotnet run
```

**Frontend:**
```bash
cd frontend
npm install
npm run dev
```

---

## 🏛️ Proje Mimarisi

Proje, sürdürülebilirlik ve test edilebilirlik için **Onion Architecture** (Clean Architecture) prensiplerine göre tasarlanmıştır.

```
SocialMap/
├── SocialMap.Core/           # Varlıklar, Arayüzler, DTO'lar (Bağımlılıksız)
├── SocialMap.Repository/     # Veri Erişimi, EF Core, Migrations
├── SocialMap.Business/       # İş Mantığı, Servisler, Validasyonlar
├── SocialMap.WebAPI/         # Controller'lar, Middleware'ler, CI/CD
└── frontend/                 # React Uygulaması
```

### Tasarım Desenleri
- **Repository Pattern**: Veri erişim soyutlaması (`IReadRepository`, `IWriteRepository`)
- **Unit of Work**: Transaction yönetimi ve atomik işlemler
- **Dependency Injection**: Gevşek bağlı bileşenler

---

## 📚 API Dokümantasyonu

Backend çalıştığında Swagger arayüzü üzerinden tüm endpoint'leri test edebilirsiniz: `http://localhost:5280/swagger`

**Öne Çıkan Endpoint'ler:**
- `GET /api/hashtags/trending` - Popüler etiketler
- `GET /api/posts/paged` - Sayfalı gönderi akışı
- `GET /api/admin/stats` - Admin dashboard istatistikleri
- `POST /api/savedposts` - Gönderiyi koleksiyona ekle

---

## 🤝 Katkıda Bulunma

1. Forklayın
2. Feature branch oluşturun (`git checkout -b feature/harika-ozellik`)
3. Commit leyin (`git commit -m 'Harika özellik eklendi'`)
4. Pushlayın (`git push origin feature/harika-ozellik`)
5. Pull Request gönderin

---

## 📝 Lisans

Bu proje MIT lisansı altındadır. Detaylar için [LICENSE](LICENSE) dosyasına bakınız.

---
**Geliştirici**: Selim Bedirhan Öztürk
