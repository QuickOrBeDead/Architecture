# Restoran Sipariş ve Mutfak Yönetim Sistemi - Vertical Slice Architecture

Bu proje, **Vertical Slice Architecture** kullanılarak geliştirilmiş bir restoran sipariş ve mutfak yönetim sistemidir. .NET 9 ve modern yazılım geliştirme prensipleri ile oluşturulmuştur.

## 🎯 Proje Hakkında

Vertical Slice Architecture, uygulamayı teknik katmanlar yerine **özellikler (features)** bazında organize eden bir mimari yaklaşımdır. Her özellik, kendi içinde tüm katmanları (UI, business logic, data access) barındırır ve diğer özelliklerden bağımsız olarak geliştirilebilir.

## 🏗️ Mimari Yapı

### Vertical Slice Architecture Prensipleri

```
RestaurantManagement.Api/
├── Features/
│   ├── Tables/              # Masa Yönetimi Slice
│   │   ├── Table.cs         # Entity
│   │   ├── TablesController.cs
│   │   ├── GetAllTables/
│   │   │   └── GetAllTablesQuery.cs
│   │   └── UpdateTableStatus/
│   │       └── UpdateTableStatusCommand.cs
│   │
│   ├── MenuItems/           # Menü Yönetimi Slice
│   │   ├── MenuItem.cs
│   │   ├── MenuItemsController.cs
│   │   └── GetMenuItems/
│   │       └── GetMenuItemsQuery.cs
│   │
│   └── Orders/              # Sipariş Yönetimi Slice
│       ├── Order.cs
│       ├── OrdersController.cs
│       ├── CreateOrder/
│       │   └── CreateOrderCommand.cs
│       ├── UpdateOrderStatus/
│       │   └── UpdateOrderStatusCommand.cs
│       └── GetKitchenOrders/
│           └── GetKitchenOrdersQuery.cs
│
└── Data/
    └── RestaurantDbContext.cs   # Paylaşılan veritabanı bağlamı
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
https://localhost:5001/scalar
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
  "status": "Occupied"  // Available, Occupied, Reserved, Cleaning
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
  "status": "Preparing"  // Pending, Confirmed, Preparing, Ready, Served, Completed, Cancelled
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
Features/Orders/
├── Order.cs                      # Domain model
├── OrdersController.cs           # API endpoint
├── CreateOrder/
│   └── CreateOrderCommand.cs     # Command + Handler + Validator
└── GetKitchenOrders/
    └── GetKitchenOrdersQuery.cs  # Query + Handler
```

### 2. CQRS with Mediator

```csharp
// Command
public record CreateOrderCommand(...) : IRequest<OrderDto>;

// Handler
public class CreateOrderHandler : IRequestHandler<CreateOrderCommand, OrderDto>
{
    public async ValueTask<OrderDto> Handle(CreateOrderCommand request, CancellationToken ct)
    {
        // Business logic here
    }
}

// Usage in Controller
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

## 🤝 Katkıda Bulunma

Bu örnek proje eğitim amaçlıdır. Geliştirmeler ve öneriler için pull request göndermekten çekinmeyin.

## 📄 Lisans

Bu proje MIT lisansı altında lisanslanmıştır.
