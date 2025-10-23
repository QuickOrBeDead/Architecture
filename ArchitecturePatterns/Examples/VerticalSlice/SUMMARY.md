# 🎯 Vertical Slice Architecture ile Restoran Yönetim Sistemi

## ✅ Tamamlanan İşler

### 1. ✨ Proje Yapısı

- ✅ .NET 9 solution ve Web API projesi oluşturuldu
- ✅ Gerekli NuGet paketleri eklendi (Mediator, EF Core, FluentValidation, Scalar)
- ✅ Vertical Slice klasör yapısı kuruldu

### 2. 🪑 Tables (Masa Yönetimi) Feature

- ✅ Table entity ve TableStatus enum
- ✅ GetAllTables query (tüm masaları listele)
- ✅ UpdateTableStatus command (masa durumunu güncelle)
- ✅ GetAllTablesEndpoint, UpdateTableStatusEndpoint (API endpoints)
- ✅ FluentValidation ile input validation

### 3. 📋 MenuItems (Menü Yönetimi) Feature

- ✅ MenuItem entity
- ✅ GetMenuItems query (kategori filtrelemeli)
- ✅ GetMenuItemsEndpoint (API endpoints)

### 4. 🍽️ Orders (Sipariş Yönetimi) Feature

- ✅ Order ve OrderItem entity'leri
- ✅ OrderStatus enum (Pending → Confirmed → Preparing → Ready → Served → Completed)
- ✅ CreateOrder command (sipariş oluşturma + business logic)
- ✅ UpdateOrderStatus command (status transition validation)
- ✅ GetKitchenOrders query (mutfak ekranı için)
- ✅ CreateOrderEndpoint, UpdateOrderStatusEndpoint, GetKitchenOrdersEndpoint (API endpoints)
- ✅ FluentValidation ile complex validation

### 5. 🗄️ Database & Infrastructure

- ✅ RestaurantDbContext (Entity Framework Core)
- ✅ In-Memory database yapılandırması
- ✅ Seed data (5 masa, 8 menü öğesi)
- ✅ Entity konfigürasyonları

### 6. 📚 Dokümantasyon

- ✅ Ana README.md (detaylı kullanım kılavuzu)
- ✅ ARCHITECTURE.md (mimari açıklaması ve diyagramlar)
- ✅ ARCHITECTURE_TESTS.md (mimari testleri açıklaması)
- ✅ API endpoint dokümantasyonu
- ✅ Scalar/OpenAPI entegrasyonu

## 🚀 Projeyi Çalıştırma

```bash
cd ArchitecturePatterns/Examples/VerticalSlice
dotnet run --project src/RestaurantManagement.Api/RestaurantManagement.Api.csproj
```

API çalışıyor: **http://localhost:5143**
Scalar UI: **http://localhost:5143/scalar**

## 🎯 Öne Çıkan Özellikler

### 1. **Vertical Slice Organization**

Her feature kendi klasöründe tamamen bağımsız:

```
Features/Tables/    # Masa işlemleri
Features/MenuItems/ # Menü işlemleri
Features/Orders/    # Sipariş işlemleri
```

### 2. **CQRS Pattern**

- Commands: Veri değiştiren işlemler
- Queries: Veri okuyan işlemler
- Mediator ile clean separation

### 3. **Business Logic**

- Masa durumu kontrolü (Available, Occupied, Reserved, Cleaning)
- Sipariş status transition validation
- Fiyat hesaplama ve toplam tutar
- Menü item availability kontrolü

### 4. **Validation**

- FluentValidation ile declarative validation
- Business rule validation (masa durumu, sipariş geçişleri)
- Input validation (ID kontrolü, quantity kontrolü)

## 📊 API Endpoints

### Tables

- `GET /api/tables` - Tüm masaları listele
- `PUT /api/tables/{id}/status` - Masa durumunu değiştir

### MenuItems

- `GET /api/menuitems?category=Pizza` - Menü öğelerini listele (kategoriye göre filtrele)

### Orders

- `POST /api/orders` - Yeni sipariş oluştur
- `PUT /api/orders/{id}/status` - Sipariş durumunu güncelle
- `GET /api/orders/kitchen` - Mutfak ekranı için aktif siparişler

## 💡 Vertical Slice Avantajları (Bu Projede Gözlemlenenler)

### ✅ Yüksek Cohesion

- Table feature'ı için tüm kod `Features/Tables/` klasöründe
- Bir özelliği anlamak için tek bir yere bakmak yeterli

### ✅ Düşük Coupling

- Orders feature, Tables feature'dan sadece entity'yi kullanıyor
- Feature'lar birbirinden bağımsız geliştirilebilir

### ✅ Hızlı Feature Ekleme

- Yeni bir feature (örn: Reservations) eklemek çok kolay
- Sadece yeni bir klasör oluştur, controller ve handler'ları ekle
- Diğer feature'lara dokunmana gerek yok

### ✅ Kolay Test Edilebilirlik

- Her handler bağımsız test edilebilir
- Mock'lama minimum seviyede

## 🎓 Öğrenme Notları

### 1. Mediator Usage

```csharp
// Endpointler'de
await _mediator.Send(new CreateOrderCommand(...));

// Handler
public class CreateOrderHandler : IRequestHandler<CreateOrderCommand, CreateOrderResponse>
{
    public async Task<CreateOrderResponse> Handle(...)
    {
        // Business logic
    }
}
```

### 2. FluentValidation

```csharp
public class CreateOrderValidator : AbstractValidator<CreateOrderCommand>
{
    RuleFor(x => x.Items).NotEmpty();
    RuleFor(x => x.TableId).GreaterThan(0);
}
```

### 3. Feature Organization

```
CreateOrder/
    │   ├── CreateOrderEndpoint.cs      # API endpoint
    │   ├── CreateOrderHandler.cs       # İş mantığı handler
    │   ├── CreateOrderRequest.cs       # Request DTO
    │   ├── CreateOrderResponse.cs      # Response DTO
    │   └── CreateOrderValidator.cs     # Validation kuralları
```

## 🔄 Sonraki Adımlar (Opsiyonel Geliştirmeler)

1. **Reservations Feature** - Rezervasyon yönetimi
2. **Payments Feature** - Ödeme işlemleri
3. **Reports Feature** - Raporlama ve analitik
4. **Authentication** - Kullanıcı kimlik doğrulama
5. **Real-time Updates** - SignalR ile mutfak ekranı güncellemeleri

## 📚 Kaynaklar

- [Vertical Slice Architecture - Jimmy Bogard](https://jimmybogard.com/vertical-slice-architecture/)
- [Mediator GitHub](https://github.com/martinothamar/Mediator)
- [CQRS Pattern](https://martinfowler.com/bliki/CQRS.html)
