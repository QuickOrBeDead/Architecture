# Copilot Instructions

## 1. Genel Mimari
- Katmanlar: Core (Domain + Abstraction), UseCases (Application), Infrastructure (Persistence), Web (Razor Pages UI), Tests (Unit / Integration / Functional)
- Domain mantýðý Core içinde; UseCases sadece orchestration yapar, EF Core eriþimini Infrastructure üstlenir.

### 1.1 Katman / Proje Eþleþmesi
Katman | Uygulama Proje(leri) | Unit Test Proje(leri) | Notlar
-------|----------------------|------------------------|-------
Core | `GoalManager.Core` | (ayrý yok) | Domain modelleri, value objects, domain servisleri. Domain kurallarý çoðunlukla handler testleri ile dolaylý test edilir.
UseCases | `GoalManager.UseCases` | `GoalManager.UseCases.Tests` | Command/Query + Handler orchestration. Ýþ kurallarý için birincil unit test hedefi.
Infrastructure | `GoalManager.Infrastructure` | (ayrý yok) | Repository & EF Core implementasyonlarý. Davranýþ Integration Tests ile doðrulanýr.
Web (UI) | `GoalManager.Web` | (ayrý yok) | Razor Pages. UI doðrulamasý Functional (E2E) testlerle.
Tests (Integration) | `GoalManager.IntegrationTests` | - | Gerçek (test) DB ve boundary etkileþimleri.
Tests (Functional) | `GoalManager.FunctionalTests` | - | HTTP pipeline & uçtan uca senaryolar.

---
## 2. Unit Test Kurallarý
1. Framework: xUnit.
2. Mock: NSubstitute (`Substitute.For<T>()`).
3. Test sýnýf adý: `<SUTClassName>Tests` (örn: `AddGoalPeriodCommandHandlerTests`).
4. Metot adý pattern: `Handle_<Senaryo>_<BeklenenSonuc>` veya `Should_<Beklenen>_When_<Koþul>`.
5. Parametrik test: `[Theory]` + `[InlineData(...)]` yalnýz davranýþ farklýysa. Ayný sonucu tekrar eden varyasyon ekleme.
6. AAA Düzeni: Her test explicit ``// Arrange``, ``// Act``, ``// Assert`` bloklarý içerir (gerekirse Arrange-Act veya Act-Assert birleþik tek satýr).
7. Guard, baþarý (happy path) ve her iþ kuralý ihlali ayrý test.
8. Tarih/saat baðýmlýlýðý: Provider mock => sabit deðer (`_time.Today().Returns(fixedDate);`).
9. Arg doðrulama: `Arg.Is<T>(x => x.Prop == val && x.Other > 0)`. Arg.Is kullanýmý tercih edilir, aþýrý Arg.Any kullanma.
10. Gereksiz async kullanma; senkron davranýþ için `Task.FromResult` yeterli.
11. Yeni `.cs` dosyalarý UTF-8.
12. Yeni eklenen iþ kuralý / branch test edilmeli (gereksiz %100 kovalanmaz; kritik dallar kapsanýr).
13. Tüm testler build pipeline'da çalýþýp geçmeli.
14. Test yazýlan kod %100 kapsanmalý (kodda cover olmayan satýr varsa test ekle).
15. Category için: `[Trait("Category", "GoalManagement/AddGoalPeriod")]` formatýný kullan.
16. Hata mesajý assertion: Tam eþleþme gerekirse `Assert.Contains(result.Errors, e => e == "..." )`; parçalý için `e.Contains("fragment", StringComparison.OrdinalIgnoreCase)`.

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

### 2.2 Parametrik (Theory) Örneði
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

### 2.3 Guard Test Örneði
```csharp
[Fact]
public void Ctor_Throws_ArgumentNullException_when_repo_null()
{
  // Arrange / Act & Assert
  Assert.Throws<ArgumentNullException>(() => new AddGoalPeriodCommandHandler(null!));
}
```

---
## 3. UseCases Katmaný Factory Örneði
Basit handler factory gerekirse (tekrar eden setup aðýrlaþýrsa) test sýnýfý içinde tutulur.
```csharp
private static AddGoalPeriodCommandHandler CreateHandler(IRepository<GoalPeriod>? repo = null)
  => new(repo ?? Substitute.For<IRepository<GoalPeriod>>());
```
Gereksiz abstraksiyon ekleme; doðrudan inline kullanmak tercih edilir.

---
## 4. Yeni Test Yazým Akýþý (Checklist)
1. SUT public API & baðýmlýlýklar incelendi
2. Gerekli mock'lar oluþturuldu
3. Guard senaryolarý yazýldý
4. En az bir baþarýlý senaryo
5. Her iþ kuralý ihlali ayrý test
6. Kenar deðer(ler) (min / max / 0 / negatif) test edildi
7. Mock Received / DidNotReceive doðrulamalarý eklendi
8. Hata mesajý assertion uygun
9. Gereksiz parametrik test yok
10. Paralel çalýþmayý bozan paylaþýlan mutable static yok
11. Build + tüm testler geçti

---
## 5. Kenar Durum Örnekleri (Kýsa Rehber)
- Sayýsal: 0, 1, negatif, maksimum (int.MaxValue)
- Koleksiyon: boþ, tek eleman, limit üstü
- Tarih: yýl sonu, ay sonu, 29 Þubat
- Concurrency: Ýlk deneme çakýþma (örn. özel exception), ikinci baþarý
- Idempotent: Ayný komut ikinci kez => ek yan etki olmamalý
- Cache/Lookup: bulunamadý -> eklendi -> tekrar çaðrý vs.

Mini concurrency pattern:
```csharp
var call = 0;
repo.AnyAsync(...).Returns(_ => ++call == 1 ? throw new ConcurrencyException() : false);
```

---
## 6. NSubstitute Ýpuçlarý
- Çaðrý sayýsý: `await repo.Received(1).AddAsync(Arg.Any<GoalPeriod>(), Arg.Any<CancellationToken>());`
- Parametre doðrulama: `Arg.Is<GoalPeriod>(p => p.TeamId == cmd.TeamId && p.Year == cmd.Year)`
- Guard sonrasý: `repo.DidNotReceive().AddAsync(Arg.Any<GoalPeriod>(), Arg.Any<CancellationToken>());`
- Sýra (gerekirse):
```csharp
Received.InOrder(() => {
  repo.Received().AnyAsync(Arg.Any<GoalPeriodByTeamIdAndYearSpec>(), Arg.Any<CancellationToken>());
  repo.Received().AddAsync(Arg.Any<GoalPeriod>(), Arg.Any<CancellationToken>());
});
```

Yaygýn hatalar: Act yapýlmadan Received çaðýrmak, aþýrý `Arg.Any`, test içinde mock davranýþý sonradan deðiþtirmek, tek testte birden fazla iþ kuralý doðrulamak.

---
## 7. Result -> HTTP (Razor Pages) Mapping (Referans)
Status | HTTP | Not
-------|------|----
Success | 200/204 | Value varsa 200, yoksa 204
NotFound | 404 | Kayýt yok
Invalid | 400 | Validation / iþ kuralý ihlali
Conflict | 409 | Çakýþma / concurrency
Unauthorized | 401 | Kimlik doðrulama yok
Forbidden | 403 | Yetki yok
Error | 500 | Log + generic mesaj

Basit handler kullaným örneði (PageModel OnPost):
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