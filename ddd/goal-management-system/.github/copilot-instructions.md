# Copilot Instructions

## 1. Genel Mimari
- Katmanlar: Core (Domain + Abstraction), UseCases (Application), Infrastructure (Persistence), Web (Razor Pages UI), Tests (Unit / Integration / Functional)
- Domain mantığı Core içinde; UseCases sadece orchestration yapar, EF Core erişimini Infrastructure üstlenir.

### 1.1 Katman / Proje Eşleşmesi
Katman | Uygulama Proje(leri) | Unit Test Proje(leri) | Notlar
-------|----------------------|------------------------|-------
Core | `GoalManager.Core` | (ayrı yok) | Domain modelleri, value objects, domain servisleri. Domain kuralları çoğunlukla handler testleri ile dolaylı test edilir.
UseCases | `GoalManager.UseCases` | `GoalManager.UseCases.Tests` | Command/Query + Handler orchestration. İş kuralları için birincil unit test hedefi.
Infrastructure | `GoalManager.Infrastructure` | (ayrı yok) | Repository & EF Core implementasyonları. Davranış Integration Tests ile doğrulanır.
Web (UI) | `GoalManager.Web` | (ayrı yok) | Razor Pages. UI doğrulaması Functional (E2E) testlerle.
Tests (Integration) | `GoalManager.IntegrationTests` | - | Gerçek (test) DB ve boundary etkileşimleri.
Tests (Functional) | `GoalManager.FunctionalTests` | - | HTTP pipeline & uçtan uca senaryolar.

---
## 2. Unit Test Kuralları
1. Framework: xUnit.
2. Mock: NSubstitute (`Substitute.For<T>()`).
3. Test sınıf adı: `<SUTClassName>Tests` (örn: `AddGoalPeriodCommandHandlerTests`).
4. Metot adı pattern: `Handle_<Senaryo>_<BeklenenSonuc>` veya `Should_<Beklenen>_When_<Koşul>`.
5. Parametrik test: `[Theory]` + `[InlineData(...)]` yalnız davranış farklıysa. Aynı sonucu tekrar eden varyasyon ekleme.
6. AAA Düzeni: Her test explicit ``// Arrange``, ``// Act``, ``// Assert`` blokları içerir (gerekirse Arrange-Act veya Act-Assert birleşik tek satır).
7. Guard, başarı (happy path) ve her iş kuralı ihlali ayrı test.
8. Tarih/saat bağımlılığı: Provider mock => sabit değer (`_time.Today().Returns(fixedDate);`).
9. Arg doğrulama: `Arg.Is<T>(x => x.Prop == val && x.Other > 0)`. Arg.Is kullanımı tercih edilir, aşırı Arg.Any kullanma.
10. Gereksiz async kullanma; senkron davranış için `Task.FromResult` yeterli.
11. Yeni `.cs` dosyaları UTF-8.
12. Yeni eklenen iş kuralı / branch test edilmeli (gereksiz %100 kovalanmaz; kritik dallar kapsanır).
13. Tüm testler build pipeline'da çalışıp geçmeli.
14. Test yazılan kod %100 kapsanmalı (kodda cover olmayan satır varsa test ekle).
15. Category için: `[Trait("Category", "GoalManagement/AddGoalPeriod")]` formatını kullan.
16. Hata mesajı assertion: Tam eşleşme gerekirse `Assert.Contains(result.Errors, e => e == "..." )`; parçalı için `e.Contains("fragment", StringComparison.OrdinalIgnoreCase)`.

### 2.1 Örnek
```csharp
[Fact]
public async Task Handle_Returns_error_when_period_already_exists()
{
  // Arrange
  var repo = Substitute.For<IRepository<GoalPeriod>>();
  repo.AnyAsync(Arg.Any<GoalPeriodByTeamIdAndYearSpec>(), Arg.Any<CancellationToken>())
      .Returns(true);
  var sut = new AddGoalPeriodCommandHandler(repo);
  var cmd = new AddGoalPeriodCommand(TeamId: 10, UserId: 1, Year: 2025);

  // Act
  var result = await sut.Handle(cmd, CancellationToken.None);

  // Assert
  Assert.False(result.IsSuccess);
  Assert.Contains(result.Errors, e => e.Contains("already exists", StringComparison.OrdinalIgnoreCase));
  await repo.DidNotReceive().AddAsync(Arg.Any<GoalPeriod>(), Arg.Any<CancellationToken>());
}
```

### 2.2 Parametrik (Theory) Örneği
```csharp
[Theory]
[InlineData(0)]
[InlineData(-1)]
public async Task Handle_Returns_invalid_for_non_positive_year(int invalidYear)
{
  // Arrange
  var repo = Substitute.For<IRepository<GoalPeriod>>();
  var sut = new AddGoalPeriodCommandHandler(repo);
  var cmd = new AddGoalPeriodCommand(TeamId: 1, UserId: 1, Year: invalidYear);

  // Act
  var result = await sut.Handle(cmd, CancellationToken.None);

  // Assert
  Assert.False(result.IsSuccess);
  Assert.Contains(result.Errors, e => e.Contains("year", StringComparison.OrdinalIgnoreCase));
}
```

### 2.3 Guard Test Örneği
```csharp
[Fact]
public void Ctor_Throws_ArgumentNullException_when_repo_null()
{
  // Arrange / Act & Assert
  Assert.Throws<ArgumentNullException>(() => new AddGoalPeriodCommandHandler(null!));
}
```

---
## 3. UseCases Katmanı Factory Örneği
Basit handler factory gerekirse (tekrar eden setup ağırlaşırsa) test sınıfı içinde tutulur. CommandHandler ve QueryHandler için factory ekle.
```csharp
private static AddGoalPeriodCommandHandler CreateHandler(IRepository<GoalPeriod>? repo = null)
  => new(repo ?? Substitute.For<IRepository<GoalPeriod>>());
```
Gereksiz abstraksiyon ekleme; doğrudan inline kullanmak tercih edilir.

---
## 4. Yeni Test Yazım Akışı (Checklist)
1. SUT public API & bağımlılıklar incelendi
2. Gerekli mock'lar oluşturuldu
3. Guard senaryoları yazıldı
4. En az bir başarılı senaryo
5. Her iş kuralı ihlali ayrı test
6. Kenar değer(ler) (min / max / 0 / negatif) test edildi
7. Mock Received / DidNotReceive doğrulamaları eklendi
8. Hata mesajı assertion uygun
9. Gereksiz parametrik test yok
10. Paralel çalışmayı bozan paylaşılan mutable static yok
11. Build olsun ve tüm testlerin geçtiğinden emin ol

---
## 5. Kenar Durum Örnekleri (Kısa Rehber)
- Sayısal: 0, 1, negatif, maksimum (int.MaxValue)
- Koleksiyon: boş, tek eleman, limit üstü
- Tarih: yıl sonu, ay sonu, 29 Şubat
- Concurrency: İlk deneme çakışma (örn. özel exception), ikinci başarı
- Idempotent: Aynı komut ikinci kez => ek yan etki olmamalı
- Cache/Lookup: bulunamadı -> eklendi -> tekrar çağrı vs.

Mini concurrency pattern:
```csharp
var call = 0;
repo.AnyAsync(...).Returns(_ => ++call == 1 ? throw new ConcurrencyException() : false);
```

---
## 6. NSubstitute İpuçları
- Çağrı sayısı: `await repo.Received(1).AddAsync(Arg.Any<GoalPeriod>(), Arg.Any<CancellationToken>());`
- Parametre doğrulama: `Arg.Is<GoalPeriod>(p => p.TeamId == cmd.TeamId && p.Year == cmd.Year)`
- Guard sonrası: `repo.DidNotReceive().AddAsync(Arg.Any<GoalPeriod>(), Arg.Any<CancellationToken>());`
- Sıra (gerekirse):
```csharp
Received.InOrder(() => {
  repo.Received().AnyAsync(Arg.Any<GoalPeriodByTeamIdAndYearSpec>(), Arg.Any<CancellationToken>());
  repo.Received().AddAsync(Arg.Any<GoalPeriod>(), Arg.Any<CancellationToken>());
});
```

Yaygın hatalar: Act yapılmadan Received çağırmak, aşırı `Arg.Any`, test içinde mock davranışı sonradan değiştirmek, tek testte birden fazla iş kuralı doğrulamak.

---
## 7. Result -> HTTP (Razor Pages) Mapping (Referans)
Status | HTTP | Not
-------|------|----
Success | 200/204 | Value varsa 200, yoksa 204
NotFound | 404 | Kayıt yok
Invalid | 400 | Validation / iş kuralı ihlali
Conflict | 409 | Çakışma / concurrency
Unauthorized | 401 | Kimlik doğrulama yok
Forbidden | 403 | Yetki yok
Error | 500 | Log + generic mesaj

Basit handler kullanım örneği (PageModel OnPost):
```csharp
var result = await _mediator.Send(command);
return result.Status switch
{
  ResultStatus.Success    => RedirectToPage("Detail", new { id = result.Value }),
  ResultStatus.NotFound   => NotFound(),
  ResultStatus.Invalid    => BadRequest(result.Errors),
  ResultStatus.Conflict   => StatusCode(409, result.Errors),
  _                       => StatusCode(500)
};
```

---