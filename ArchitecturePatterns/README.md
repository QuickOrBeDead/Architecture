# Architecture Patterns

Bu depo, farklı yazılım mimari desenlerinin ilkelerini, faydalarını ve uygulama farklılıklarını anlamaya yardımcı olmak için pratik örnekler içerir.

## Genel Bakış

Her klasör, ilgili mimari desenin temel kavramlarını ve yapısını gösteren eksiksiz bir örnek uygulama içerir.

## Mimari Desenler

### 1. Vertical Slice Architecture

**Konum:** `Examples/VerticalSlice/`

Vertical Slice Architecture, kodu teknik katmanlar yerine özelliklere göre organize eder. Her özellik dilimi, ihtiyaç duyduğu tüm katmanları (UI, iş mantığı, veri erişimi) tek bir dikey bileşen içinde barındırır.

**Temel Özellikler:**

- Özellikler izole ve kendi kendine yeterlidir
- Dilimler arasında minimum bağlantı
- Özellikleri ekleme, değiştirme veya kaldırma kolaydır
- Özellikler içinde yüksek bağlılığı (cohesion) teşvik eder

### 2. Clean Architecture

**Konum:** `Examples/Clean/`

Clean Architecture (Robert C. Martin tarafından), bağımlılık kurallarının domain'e doğru içe doğru akmasıyla katmanlar aracılığıyla endişelerin ayrılmasını vurgular.

**Temel Özellikler:**

- Framework'lerden bağımsız
- Test edilebilir iş mantığı
- UI ve veritabanından bağımsız
- Bağımlılık kuralı: dış katmanlar iç katmanlara bağımlıdır

**Katmanlar:**

- Entities (Kurumsal İş Kuralları)
- Use Cases (Uygulama İş Kuralları)
- Interface Adapters (Controller'lar, Presenter'lar, Gateway'ler)
- Frameworks & Drivers (UI, Veritabanı, Harici Servisler)

### 3. Onion Architecture

**Konum:** `Examples/Onion/`

Onion Architecture (Jeffrey Palermo tarafından), domain modelini merkeze yerleştirmeye ve katmanların soğan gibi etrafını sarmasına odaklanır.

**Temel Özellikler:**

- Merkezde domain modeli
- Bağımlılıklar içe doğru işaret eder
- Infrastructure en dış katmandadır
- Dependency inversion kullanır

**Katmanlar:**

- Domain Model (Çekirdek)
- Domain Services
- Application Services
- Infrastructure (Dış katman)

### 4. Hexagonal Architecture

**Konum:** `Examples/Hexagonal/`

Hexagonal Architecture (Alistair Cockburn tarafından), çekirdek iş mantığını port'lar (arayüzler) ve adapter'lar (uygulamalar) aracılığıyla dış endişelerden izole eder.

**Temel Özellikler:**

- Uygulama çekirdeği izole edilmiştir
- Port'lar arayüzleri tanımlar
- Adapter'lar arayüzleri uygular
- Harici bağımlılıklar takılabilir (pluggable)
- Tüm harici sistemlerin simetrik olarak ele alınması

**Bileşenler:**

- Core/Domain (İş mantığı)
- Ports (Arayüzler)
- Adapters (UI, Veritabanı, Harici Servisler için Uygulamalar)

## Karşılaştırma

| Yön | Vertical Slice | Clean | Onion | Hexagonal |
|-----|---------------|-------|-------|-----------|
| **Organizasyon** | Özellik Bazlı | Katman Bazlı | Katman Bazlı | Çekirdek + Ports/Adapters |
| **Coupling** | Dilimler arası düşük | Düşük (bağımlılık kuralı) | Düşük (içe dönük bağımlılıklar) | Düşük (izole çekirdek) |
| **Kullanım Alanı** | Özellik odaklı uygulamalar | Karmaşık iş mantığı | Domain-driven tasarım | Takılabilir sistemler |
| **Öğrenme Eğrisi** | Düşük | Orta | Orta | Orta-Yüksek |
| **Esneklik** | Özellik başına yüksek | Yüksek | Yüksek | Çok Yüksek |

## İş Senaryosu Örneği

### Restoran Sipariş ve Mutfak Yönetim Sistemi

**İş İhtiyacı:** Garsonların sipariş alabileceği, mutfağın siparişleri görebileceği, kasanın hesap kesebileceği ve yöneticinin raporlara erişebileceği entegre sistem.

**Temel Özellikler:**

- Masa ve rezervasyon yönetimi
- Menü ve fiyat yönetimi
- Sipariş alma (garson tablet uygulaması)
- Mutfak ekran sistemi (sipariş durumları)
- Adisyon ve ödeme işlemleri
- Stok takibi
- Personel yönetimi
- Günlük/aylık raporlar

## Başlangıç

Her örnek klasörüne giderek şunları görebilirsiniz:

- Uygulama detayları
- Örnek kod
- Mimari diyagramlar
- Best practices
- Yaygın hatalar

## Kaynaklar

- **Vertical Slice Architecture:** [Jimmy Bogard'ın Blogu](https://jimmybogard.com/vertical-slice-architecture/)
- **Clean Architecture:** [Robert C. Martin'in Kitabı](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- **Onion Architecture:** [Jeffrey Palermo'nun Blogu](https://jeffreypalermo.com/2008/07/the-onion-architecture-part-1/)
- **Hexagonal Architecture:** [Alistair Cockburn'ün Makalesi](https://alistair.cockburn.us/hexagonal-architecture/)

## Katkıda Bulunma

Daha fazla örnek eklemek, mevcut örnekleri geliştirmek veya başkalarının bu desenleri öğrenmesine yardımcı olmak için dokümantasyon eklemekten çekinmeyin.

## Lisans

Detaylar için LICENSE dosyasına bakınız.
