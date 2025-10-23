# Vertical Slice Architecture - Proje Yapısı

## 📁 Klasör Yapısı

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

## 🎯 Vertical Slice Prensibi

Her feature (slice) kendi içinde:

- ✅ API Endpoints (HTTP endpoint'leri)
- ✅ Commands (veri değiştiren request'ler)
- ✅ Queries (veri okuyan request'ler)
- ✅ Handlers (asıl iş mantığını yapan sınıflar)
- ✅ Request/Response DTOs (veri transfer nesneleri)
- ✅ Validators (input validation kuralları)

barındırır.

**📝 Not:** 

- **Commands:** Veri değiştiren işlemler için request nesneleri (Create, Update, Delete)
- **Queries:** Veri okuyan işlemler için request nesneleri (Get, List)
- **Handlers:** Commands ve Queries'i işleyerek asıl iş mantığını gerçekleştiren sınıflar
- **DTOs:** API'ye gelen ve dönen veri yapıları

**📋 Shared Components:**

- ✅ Entities (domain models) - `Entities/` klasöründe ortak kullanım
- ✅ Common utilities - `Common/` klasöründe paylaşılan helper'lar
- ✅ Database context - `Data/` klasöründe merkezi DB erişimi

## 🔄 İstek Akışı

### Örnek: Sipariş Oluşturma

```
1. HTTP POST /api/orders
   ↓
2. CreateOrderEndpoint
   ↓
3. Mediator Send(CreateOrderRequest) → Command
   ↓
4. CreateOrderValidator (FluentValidation)
   ↓
5. CreateOrderHandler (Business Logic)
   ↓
6. RestaurantDbContext (Database)
   ↓
7. Return CreateOrderResponse
   ↓
8. HTTP 201 Created Response
```

## 💡 Avantajlar

### ✅ Bağımsızlık

- Her feature diğerlerinden bağımsız
- Paralel geliştirme mümkün
- Bir feature'daki değişiklik diğerlerini etkilemez

### ✅ Basitlik

- Feature anlamak için tek bir klasöre bakmak yeterli
- Tüm ilgili kod bir arada
- Minimum boilerplate

### ✅ Ölçeklenebilirlik

- Yeni feature eklemek çok kolay
- Feature silinmesi de kolay
- Microservice'e dönüştürme kolay

### ✅ Hibrit Yaklaşım

- Domain entities ortak kullanım için merkezi konumda
- Feature-specific logic her feature'da ayrı
- Paylaşılan utilities `Common/` klasöründe

## 🆚 Geleneksel Katmanlı Mimari ile Karşılaştırma

### Geleneksel (Layered Architecture):

```
Controllers/
├── TablesController.cs
├── MenuItemsController.cs
└── OrdersController.cs

Services/
├── TableService.cs
├── MenuItemService.cs
└── OrderService.cs

Repositories/
├── TableRepository.cs
├── MenuItemRepository.cs
└── OrderRepository.cs

Models/
├── Table.cs
├── MenuItem.cs
└── Order.cs
```

**Sorun:** Bir feature için 4 farklı klasöre bakmak gerekir!

### Vertical Slice:

```
Features/
├── Tables/          # Tüm table işlemleri burada
├── MenuItems/       # Tüm menu işlemleri burada
└── Orders/          # Tüm order işlemleri burada
```

**Çözüm:** Her şey feature bazında organize edilmiş!

## 🔧 Kullanılan Teknolojiler ve Desenler

### CQRS (Command Query Responsibility Segregation)

- **Commands:** Veri değiştiren işlemler için request nesneleri (CreateOrder, UpdateTableStatus)
- **Queries:** Veri okuyan işlemler için request nesneleri (GetAllTables, GetMenuItems)
- **Command Handlers:** Command'ları işleyerek veri değiştiren sınıflar
- **Query Handlers:** Query'leri işleyerek veri okuyan sınıflar
- Mediator ile Command/Query → Handler yönlendirmesi

### Mediator Pattern

- Request → Handler yönlendirmesi
- Loose coupling sağlar
- Mediator kütüphanesi kullanılmış

### Validation

- FluentValidation ile input validation
- Her command'in kendi validator'ı var

### DTO Pattern

- API response'lar için özel nesneler
- Domain entity'leri doğrudan expose etmeyiz

### Result Pattern

- `Result.cs` ve `ResultHelper.cs` ile başarı/hata yönetimi
- Exception throwing yerine functional approach

### Validation Behavior

- `ValidationBehavior.cs` ile MediatR pipeline'ında otomatik validation
- Tüm command'lar için merkezi validation işlemi

## 📊 Feature'lar ve Endpoint'ler

| Feature | Endpoint | Method | Açıklama |
|---------|----------|--------|----------|
| **Tables** | `/api/tables` | GET | Tüm masaları listele |
| | `/api/tables/{id}/status` | PUT | Masa durumunu güncelle |
| **MenuItems** | `/api/menuitems` | GET | Menü öğelerini listele |
| **Orders** | `/api/orders` | POST | Yeni sipariş oluştur |
| | `/api/orders/{id}/status` | PUT | Sipariş durumunu güncelle |
| | `/api/orders/kitchen` | GET | Mutfak siparişlerini listele |

## 🎓 Öğrenme Önerileri

1. **Tables Feature'ını incele** - En basit feature
2. **Orders Feature'ını incele** - Daha karmaşık business logic
3. **CQRS pattern'ini gözlemle** - Command vs Query ayrımı
4. **Mediator kullanımını öğren** - Request/Handler pattern
5. **Yeni bir feature ekle** - Örneğin: Reservations

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

**Bu kadar basit!** 🎉
