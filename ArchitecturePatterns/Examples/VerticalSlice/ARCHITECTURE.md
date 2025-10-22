# Vertical Slice Architecture - Proje Yapısı

## 📁 Klasör Yapısı

```
RestaurantManagement.Api/
│
├── Features/                          # Her feature bağımsız bir "slice"
│   │
│   ├── Tables/                        # 🪑 Masa Yönetimi Feature
│   │   ├── Table.cs                   # Domain entity
│   │   ├── TablesController.cs        # API endpoints
│   │   ├── GetAllTables/
│   │   │   └── GetAllTablesQuery.cs   # Query + Handler + DTO
│   │   └── UpdateTableStatus/
│   │       └── UpdateTableStatusCommand.cs  # Command + Handler + Validator + DTO
│   │
│   ├── MenuItems/                     # 📋 Menü Yönetimi Feature
│   │   ├── MenuItem.cs                # Domain entity
│   │   ├── MenuItemsController.cs     # API endpoints
│   │   └── GetMenuItems/
│   │       └── GetMenuItemsQuery.cs   # Query + Handler + DTO
│   │
│   └── Orders/                        # 🍽️ Sipariş Yönetimi Feature
│       ├── Order.cs                   # Domain entities
│       ├── OrdersController.cs        # API endpoints
│       ├── CreateOrder/
│       │   └── CreateOrderCommand.cs  # Command + Handler + Validator + DTO
│       ├── UpdateOrderStatus/
│       │   └── UpdateOrderStatusCommand.cs
│       └── GetKitchenOrders/
│           └── GetKitchenOrdersQuery.cs  # Query + Handler + DTO
│
├── Data/
│   └── RestaurantDbContext.cs         # Shared database context
│
└── Program.cs                         # Application startup
```

## 🎯 Vertical Slice Prensibi

Her feature (slice) kendi içinde:

- ✅ Entity (domain model)
- ✅ Controller (API endpoints)
- ✅ Commands (veri değiştiren işlemler)
- ✅ Queries (veri okuyan işlemler)
- ✅ Handlers (iş mantığı)
- ✅ Validators (validation)
- ✅ DTOs (data transfer objects)

barındırır.

## 🔄 İstek Akışı

### Örnek: Sipariş Oluşturma

```
1. HTTP POST /api/orders
   ↓
2. OrdersController.CreateOrder()
   ↓
3. Mediator Send(CreateOrderCommand)
   ↓
4. CreateOrderValidator (FluentValidation)
   ↓
5. CreateOrderHandler (Business Logic)
   ↓
6. RestaurantDbContext (Database)
   ↓
7. Return OrderDto
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

- **Command:** Veri değiştiren işlemler
- **Query:** Veri okuyan işlemler
- Mediator ile implement edilmiş

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
2. Entity oluştur
3. Controller oluştur
4. Gerekli Command/Query'leri oluştur
5. Handler'ları implement et
6. Validator ekle (gerekiyorsa)
7. Çalıştır ve test et!

**Bu kadar basit!** 🎉
