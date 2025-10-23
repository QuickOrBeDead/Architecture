# Restoran Sipariş ve Mutfak Yönetim Sistemi - Vertical Slice Architecture

Bu proje, **Vertical Slice Architecture** kullanılarak geliştirilmiş bir restoran sipariş ve mutfak yönetim sistemidir. .NET 9 ve modern yazılım geliştirme prensipleri ile oluşturulmuştur.

## 🎯 Proje Hakkında

Vertical Slice Architecture, uygulamayı teknik katmanlar yerine **özellikler (features)** bazında organize eden bir mimari yaklaşımdır. Her özellik, kendi içinde tüm katmanları (UI, business logic, data access) barındırır ve diğer özelliklerden bağımsız olarak geliştirilebilir.

## 🏗️ Mimari Yapı

### 📁 Klasör Yapısı

```
RestaurantManagement.Api/
│
├── Features/                          # Her feature bağımsız bir "slice"
│   │
│   ├── Tables/                        # 🪑 Masa Yönetimi Feature
│   │   ├── GetAllTables/
│   │   │   ├── GetAllTablesEndpoint.cs     # API endpoint
│   │   │   ├── GetAllTablesHandler.cs      # İş mantığı handler
│   │   │   └── GetAllTablesResponse.cs     # Response DTO
│   │   └── UpdateTableStatus/
│   │       ├── UpdateTableStatusEndpoint.cs    # API endpoint
│   │       ├── UpdateTableStatusHandler.cs     # İş mantığı handler
│   │       ├── UpdateTableStatusRequest.cs     # Request DTO
│   │       ├── UpdateTableStatusResponse.cs    # Response DTO
│   │       └── UpdateTableStatusValidator.cs   # Validation kuralları
│   │
│   ├── MenuItems/                     # 📋 Menü Yönetimi Feature
│   │   └── GetMenuItems/
│   │       ├── GetMenuItemsEndpoint.cs     # API endpoint
│   │       ├── GetMenuItemsHandler.cs      # İş mantığı handler
│   │       ├── GetMenuItemsResponse.cs     # Response DTO
│   │       └── GetMenuItemsValidator.cs    # Validation kuralları
│   │
│   └── Orders/                        # 🍽️ Sipariş Yönetimi Feature
│       ├── CreateOrder/
│       │   ├── CreateOrderEndpoint.cs      # API endpoint
│       │   ├── CreateOrderHandler.cs       # İş mantığı handler
│       │   ├── CreateOrderRequest.cs       # Request DTO
│       │   ├── CreateOrderResponse.cs      # Response DTO
│       │   └── CreateOrderValidator.cs     # Validation kuralları
│       ├── UpdateOrderStatus/
│       │   ├── UpdateOrderStatusEndpoint.cs    # API endpoint
│       │   ├── UpdateOrderStatusHandler.cs     # İş mantığı handler
│       │   ├── UpdateOrderStatusRequest.cs     # Request DTO
│       │   ├── UpdateOrderStatusResponse.cs    # Response DTO
│       │   └── UpdateOrderStatusValidator.cs   # Validation kuralları
│       └── GetKitchenOrders/
│           ├── GetKitchenOrdersEndpoint.cs     # API endpoint
│           ├── GetKitchenOrdersHandler.cs      # İş mantığı handler
│           └── GetKitchenOrdersResponse.cs     # Response DTO
│
├── Entities/                          # 🏗️ Shared Domain Entities
│   ├── Table.cs                       # Masa entity'si
│   ├── TableStatus.cs                 # Masa durumu enum
│   ├── MenuItem.cs                    # Menü öğesi entity'si
│   ├── Order.cs                       # Sipariş entity'si
│   ├── OrderItem.cs                   # Sipariş kalemi entity'si
│   └── OrderStatus.cs                 # Sipariş durumu enum
│
├── Common/                            # 🔧 Ortak Utilities ve Behaviors
│   ├── Result.cs                      # Result pattern implementation
│   ├── ResultHelper.cs               # Result helper methods
│   └── Behaviors/                     # MediatR behaviors
│       └── ValidationBehavior.cs      # Otomatik validation behavior
│
├── Data/
│   └── RestaurantDbContext.cs         # Shared database context
│
└── Program.cs                         # Application startup
```

### Temel Özellikler

#### 1. **Bağımsız Feature'lar (Slices)**

- Her feature kendi dizininde izole edilmiştir
- Feature içindeki tüm kod aynı yerdedir (controller, commands, queries, DTOs)
- Yeni bir feature eklemek diğerlerini etkilemez

#### 2. **CQRS Pattern**

- **Commands:** Veri değiştiren işlemler (Create, Update, Delete)
- **Queries:** Veri okuyan işlemler (Get, List)
- Mediator kütüphanesi ile implement edilmiştir (source generator based, high performance)

#### 3. **Request/Response Pattern**

- Her işlem bir request nesnesi alır
- Her işlem bir response (DTO) döner
- İş mantığı handler'larda encapsulate edilmiştir

## 🚀 Teknolojiler

- **.NET 9** - Modern web API framework
- **ASP.NET Core** - Web API
- **Entity Framework Core 9** - ORM (In-Memory Database)
- **Mediator** - Source generator tabanlı, yüksek performanslı CQRS implementasyonu ([martinothamar/Mediator](https://github.com/martinothamar/Mediator))
- **FluentValidation** - Input validation
- **Scalar/OpenAPI** - API dokümantasyonu

## 📦 Kurulum ve Çalıştırma

### Gereksinimler

- .NET 9 SDK
- Visual Studio 2022 / VS Code / JetBrains Rider

### Adımlar

1. **Projeyi klonlayın:**

```bash
git clone <repository-url>
cd ArchitecturePatterns/Examples/VerticalSlice
```

2. **Bağımlılıkları yükleyin:**

```bash
dotnet restore
```

3. **Projeyi çalıştırın:**

```bash
cd src/RestaurantManagement.Api
dotnet run
```

4. **Scalar UI'a gidin:**

```
http://localhost:5143/scalar
```

## 🔍 API Endpoints

### Masa Yönetimi (Tables)

#### Tüm masaları listele

```http
GET /api/tables
```

**Response:**

```json
[
  {
    "id": 1,
    "tableNumber": 1,
    "capacity": 4,
    "status": "Available",
    "reservedAt": null
  }
]
```

#### Masa durumunu güncelle

```http
PUT /api/tables/{tableId}/status
```

**Request Body:**

```json
{
  "status": 2  // Available, Occupied, Reserved, Cleaning
}
```

### Menü Yönetimi (MenuItems)

#### Menü öğelerini listele

```http
GET /api/menuitems?category=Pizza
```

**Response:**

```json
[
  {
    "id": 1,
    "name": "Margherita Pizza",
    "category": "Pizza",
    "price": 12.99,
    "description": null,
    "isAvailable": true
  }
]
```

### Sipariş Yönetimi (Orders)

#### Yeni sipariş oluştur

```http
POST /api/orders
```

**Request Body:**

```json
{
  "tableId": 1,
  "items": [
    {
      "menuItemId": 1,
      "quantity": 2,
      "specialInstructions": "Extra cheese"
    },
    {
      "menuItemId": 7,
      "quantity": 2,
      "specialInstructions": null
    }
  ],
  "notes": "Birthday celebration"
}
```

**Response:**

```json
{
  "id": 1,
  "orderNumber": "ORD-20251021-143022",
  "tableId": 1,
  "orderDate": "2025-10-21T14:30:22.123Z",
  "status": "Pending",
  "totalAmount": 31.96,
  "items": [
    {
      "id": 1,
      "menuItemName": "Margherita Pizza",
      "quantity": 2,
      "price": 12.99,
      "specialInstructions": "Extra cheese"
    }
  ]
}
```

#### Sipariş durumunu güncelle

```http
PUT /api/orders/{orderId}/status
```

**Request Body:**

```json
{
  "status": 2  // Pending, Confirmed, Preparing, Ready, Served, Completed, Cancelled
}
```

#### Mutfak siparişlerini listele

```http
GET /api/orders/kitchen
```

**Response:**

```json
[
  {
    "id": 1,
    "orderNumber": "ORD-20251021-143022",
    "tableNumber": 1,
    "orderDate": "2025-10-21T14:30:22.123Z",
    "status": "Preparing",
    "items": [
      {
        "menuItemName": "Margherita Pizza",
        "category": "Pizza",
        "quantity": 2,
        "specialInstructions": "Extra cheese"
      }
    ]
  }
]
```

## 💡 Vertical Slice Architecture'ın Avantajları

### ✅ Artıları

1. **Yüksek Cohesion (Bağlılık)**
   - İlgili tüm kod bir arada
   - Feature'ı anlamak için tek bir klasöre bakmak yeterli

2. **Düşük Coupling (Bağımlılık)**
   - Feature'lar birbirinden bağımsız
   - Bir feature'daki değişiklik diğerlerini etkilemez

3. **Hızlı Geliştirme**
   - Yeni feature eklemek çok kolay
   - Boilerplate kod minimum
   - Paralel geliştirme yapılabilir

4. **Kolay Test Edilebilirlik**
   - Her feature ayrı test edilebilir
   - Mock'lama ihtiyacı azalır

5. **Kolay Anlaşılabilirlik**
   - Yeni geliştiriciler hızlı adapte olur
   - İş gereksinimleri ile kod yapısı uyumlu

### ⚠️ Eksileri

1. **Kod Tekrarı**
   - Bazı kod parçaları feature'lar arasında tekrar edebilir
   - Ancak bu bilinçli bir trade-off'tur

2. **Cross-Cutting Concerns**
   - Ortak işlemler (logging, caching) için ek yapı gerekir
   - Middleware/behavior pattern'leri kullanılabilir

3. **Shared Data**
   - Database context gibi paylaşılan kaynaklar dikkatlice yönetilmeli

## 🎓 Öğrenme Noktaları

### 1. Feature Orgsanizasyonu

Her feature'ın kendi klasörü vardır:

```
Features/Orders/                        # 🍽️ Sipariş Yönetimi Feature
│       ├── CreateOrder/
│       │   ├── CreateOrderEndpoint.cs      # API endpoint
│       │   ├── CreateOrderHandler.cs       # İş mantığı handler
│       │   ├── CreateOrderRequest.cs       # Request DTO
│       │   ├── CreateOrderResponse.cs      # Response DTO
│       │   └── CreateOrderValidator.cs     # Validation kuralları
│       ├── UpdateOrderStatus/
│       │   ├── UpdateOrderStatusEndpoint.cs    # API endpoint
│       │   ├── UpdateOrderStatusHandler.cs     # İş mantığı handler
│       │   ├── UpdateOrderStatusRequest.cs     # Request DTO
│       │   ├── UpdateOrderStatusResponse.cs    # Response DTO
│       │   └── UpdateOrderStatusValidator.cs   # Validation kuralları
```

### 2. CQRS with Mediator

```csharp
// Command
public record CreateOrderCommand(...) : IRequest<CreateOrderResponse>;

// Handler
public class CreateOrderHandler : IRequestHandler<CreateOrderCommand, CreateOrderResponse>
{
    public async ValueTask<CreateOrderResponse> Handle(CreateOrderCommand request, CancellationToken ct)
    {
        // Business logic here
    }
}

// Usage in Endpoint
await _mediator.Send(new CreateOrderCommand(...));
```

### 3. Validation

```csharp
public class CreateOrderValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderValidator()
    {
        RuleFor(x => x.TableId).GreaterThan(0);
        RuleFor(x => x.Items).NotEmpty();
    }
}
```

## 🔄 Diğer Mimarilerle Karşılaştırma

| Özellik | Vertical Slice | Clean/Onion | Hexagonal |
|---------|---------------|-------------|-----------|
| **Organizasyon** | Feature bazlı | Katman bazlı | Katman bazlı |
| **Kod Lokasyonu** | Her şey bir arada | Katmanlara dağılmış | Core + Adapters |
| **Yeni Feature** | Çok kolay | Orta | Orta |
| **Öğrenme Eğrisi** | Düşük | Orta-Yüksek | Yüksek |
| **Boilerplate** | Minimum | Yüksek | Yüksek |
| **Test Edilebilirlik** | Yüksek | Çok yüksek | Çok yüksek |

## 📚 Kaynaklar

- [Jimmy Bogard - Vertical Slice Architecture](https://jimmybogard.com/vertical-slice-architecture/)
- [Mediator - Source Generator Based](https://github.com/martinothamar/Mediator)
- [CQRS Pattern](https://martinfowler.com/bliki/CQRS.html)
- [Feature Slices for ASP.NET Core MVC](https://docs.microsoft.com/en-us/archive/msdn-magazine/2016/september/asp-net-core-feature-slices-for-asp-net-core-mvc)

## 🚀 Yeni Feature Ekleme

Yeni bir feature eklemek için:

1. `Features/` altında yeni klasör oluştur
2. Entity oluştur (gerekiyorsa `Entities/` klasöründe)
3. API Endpoint'leri oluştur
4. Command/Query request'lerini oluştur
5. Handler'ları implement et (Command/Query Handler)
6. Request/Response DTOs oluştur
7. Validator ekle (gerekiyorsa)
8. Çalıştır ve test et!

## 🤝 Katkıda Bulunma

Bu örnek proje eğitim amaçlıdır. Geliştirmeler ve öneriler için pull request göndermekten çekinmeyin.

## 📄 Lisans

Bu proje MIT lisansı altında lisanslanmıştır.
