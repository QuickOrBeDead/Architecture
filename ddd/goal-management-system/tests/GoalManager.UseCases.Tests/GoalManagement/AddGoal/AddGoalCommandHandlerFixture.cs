using System.Reflection;
using Ardalis.SharedKernel;
using Ardalis.Specification;
using GoalManager.Core.Exceptions;
using GoalManager.Core.GoalManagement;
using GoalManager.Core.GoalManagement.Specifications;
using GoalManager.UseCases.GoalManagement.AddGoal;
using NSubstitute;
using Xunit;

namespace GoalManager.UseCases.Tests.GoalManagement.AddGoal;

[Trait("Category", "AddGoalCommandHandler")]
public sealed class AddGoalCommandHandlerTests
{
  // GoalValue.Create branch 1: min >= mid
  [Fact]
  public async Task Handle_Returns_error_when_min_not_less_than_mid()
  {
    var cmd = CreateCommand(min: 5, mid: 5, max: 10); // triggers first check
    var goalSetRepository = Substitute.For<IRepository<GoalSet>>();
    var sut = CreateAddGoalCommandHandler(goalSetRepository);

    var result = await sut.Handle(cmd, CancellationToken.None);

    Assert.False(result.IsSuccess);
    Assert.Contains(result.Errors, e => e == "Min value must be less than mid value");
    await goalSetRepository.DidNotReceive().SingleOrDefaultAsync(Arg.Any<ISingleResultSpecification<GoalSet>>(), Arg.Any<CancellationToken>());
  }

  // GoalValue.Create branch 2: mid >= max (with valid first condition)
  [Fact]
  public async Task Handle_Returns_error_when_mid_not_less_than_max()
  {
    var cmd = CreateCommand(min: 3, mid: 7, max: 7); // min < mid but mid == max
    var goalSetRepository = Substitute.For<IRepository<GoalSet>>();
    var sut = CreateAddGoalCommandHandler(goalSetRepository);

    var result = await sut.Handle(cmd, CancellationToken.None);

    Assert.False(result.IsSuccess);
    Assert.Contains(result.Errors, e => e == "Mid value must be less than max value");
    await goalSetRepository.DidNotReceive().SingleOrDefaultAsync(Arg.Any<ISingleResultSpecification<GoalSet>>(), Arg.Any<CancellationToken>());
  }

  // GoalValue.Create branch 3 (percentage type) : out-of-range low (min <= 0)
  [Fact]
  public async Task Handle_Returns_error_when_percentage_values_below_range()
  {
    var cmd = CreateCommand(min: 0, mid: 10, max: 20, valueType: GoalValueType.Percentage); // ordering ok, min invalid
    var goalSetRepository = Substitute.For<IRepository<GoalSet>>();
    var sut = CreateAddGoalCommandHandler(goalSetRepository);

    var result = await sut.Handle(cmd, CancellationToken.None);

    Assert.False(result.IsSuccess);
    Assert.Contains(result.Errors, e => e == "Values must be between 1 and 100 for percentage goal type");
    await goalSetRepository.DidNotReceive().SingleOrDefaultAsync(Arg.Any<ISingleResultSpecification<GoalSet>>(), Arg.Any<CancellationToken>());
  }

  // GoalValue.Create branch 3 (percentage type) : out-of-range high (max > 100)
  [Fact]
  public async Task Handle_Returns_error_when_percentage_values_above_range()
  {
    var cmd = CreateCommand(min: 10, mid: 50, max: 150, valueType: GoalValueType.Percentage); // ordering ok, max invalid
    var goalSetRepository = Substitute.For<IRepository<GoalSet>>();
    var sut = CreateAddGoalCommandHandler(goalSetRepository);

    var result = await sut.Handle(cmd, CancellationToken.None);

    Assert.False(result.IsSuccess);
    Assert.Contains(result.Errors, e => e == "Values must be between 1 and 100 for percentage goal type");
    await goalSetRepository.DidNotReceive().SingleOrDefaultAsync(Arg.Any<ISingleResultSpecification<GoalSet>>(), Arg.Any<CancellationToken>());
  }

  // Success case for percentage type
  [Fact]
  public async Task Handle_Succeeds_with_valid_percentage_goal_value()
  {
    var goalSet = CreateGoalSet(teamId: 55, periodId: 901, userId: 700);
    var goalSetRepository = Substitute.For<IRepository<GoalSet>>();
    goalSetRepository.SingleOrDefaultAsync(Arg.Any<GoalSetWithGoalsByGoalSetIdSpec>(), Arg.Any<CancellationToken>())
      .Returns(goalSet);

    var sut = CreateAddGoalCommandHandler(goalSetRepository);
    var result = await sut.Handle(CreateCommand(goalSet.Id, min: 10, mid: 30, max: 60, percentage: 20, valueType: GoalValueType.Percentage), CancellationToken.None);

    Assert.True(result.IsSuccess);
    Assert.Empty(result.Errors);
    await goalSetRepository.Received(1).UpdateAsync(goalSet, Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task Handle_Returns_error_when_GoalSet_not_found()
  {
    var goalSetRepository = Substitute.For<IRepository<GoalSet>>();
    goalSetRepository.SingleOrDefaultAsync(Arg.Any<GoalSetWithGoalsByGoalSetIdSpec>(), Arg.Any<CancellationToken>())
      .Returns(Task.FromResult<GoalSet?>(null));

    var sut = CreateAddGoalCommandHandler(goalSetRepository);
    var result = await sut.Handle(CreateCommand(goalSetId: 42), CancellationToken.None);

    Assert.False(result.IsSuccess);
    Assert.Contains(result.Errors, e => e.Contains("Goal set not found", StringComparison.OrdinalIgnoreCase));
    await goalSetRepository.DidNotReceive().UpdateAsync(Arg.Any<GoalSet>(), Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task Handle_Returns_error_when_AddGoal_business_rule_fails()
  {
    var goalSet = CreateGoalSet(teamId: 10, periodId: 2025, userId: 1000, status: GoalSetStatus.Approved);
    var goalSetRepository = Substitute.For<IRepository<GoalSet>>();
    goalSetRepository.SingleOrDefaultAsync(Arg.Any<GoalSetWithGoalsByGoalSetIdSpec>(), Arg.Any<CancellationToken>())
      .Returns(goalSet);

    var sut = CreateAddGoalCommandHandler(goalSetRepository);
    var result = await sut.Handle(CreateCommand(goalSet.Id), CancellationToken.None);

    Assert.False(result.IsSuccess);
    Assert.Contains(result.Errors, e => e.Contains("Cannot add goals", StringComparison.OrdinalIgnoreCase));
    await goalSetRepository.DidNotReceive().UpdateAsync(Arg.Any<GoalSet>(), Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task Handle_Succeeds_and_updates_repository_for_number_type()
  {
    var goalSet = CreateGoalSet(teamId: 7, periodId: 300, userId: 555);
    var goalSetRepository = Substitute.For<IRepository<GoalSet>>();
    goalSetRepository.SingleOrDefaultAsync(Arg.Any<GoalSetWithGoalsByGoalSetIdSpec>(), Arg.Any<CancellationToken>())
      .Returns(goalSet);

    var sut = CreateAddGoalCommandHandler(goalSetRepository);
    var result = await sut.Handle(CreateCommand(goalSetId: goalSet.Id, percentage: 40), CancellationToken.None);

    Assert.True(result.IsSuccess);
    Assert.Empty(result.Errors);
    Assert.Equal(goalSet.TeamId, result.Value.TeamId);
    Assert.Equal(goalSet.PeriodId, result.Value.PeriodId);
    await goalSetRepository.Received(1).UpdateAsync(goalSet, Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task Handle_Retries_on_Concurrency_and_finally_returns_error_after_max_attempts()
  {
    var goalSetRepository = Substitute.For<IRepository<GoalSet>>();
    goalSetRepository
      .When(x => x.SingleOrDefaultAsync(Arg.Any<GoalSetWithGoalsByGoalSetIdSpec>(), Arg.Any<CancellationToken>()))
      .Do(_ => throw new ConcurrencyException());

    var sut = CreateAddGoalCommandHandler(goalSetRepository);
    var result = await sut.Handle(CreateCommand(goalSetId: 77), CancellationToken.None);

    Assert.False(result.IsSuccess);
    Assert.Contains(result.Errors, e => e.Contains("Another user modified", StringComparison.OrdinalIgnoreCase));
    await goalSetRepository.Received(4).SingleOrDefaultAsync(Arg.Any<GoalSetWithGoalsByGoalSetIdSpec>(), Arg.Any<CancellationToken>()); // 1 + 3 retries
    await goalSetRepository.DidNotReceive().UpdateAsync(Arg.Any<GoalSet>(), Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task Handle_Retries_on_initial_Concurrency_then_succeeds()
  {
    var goalSet = CreateGoalSet(teamId: 20, periodId: 909, userId: 1);
    var call = 0;
    var goalSetRepository = Substitute.For<IRepository<GoalSet>>();
    goalSetRepository.SingleOrDefaultAsync(Arg.Any<GoalSetWithGoalsByGoalSetIdSpec>(), Arg.Any<CancellationToken>())
      .Returns(ci =>
      {
        call++;
        if (call == 1) throw new ConcurrencyException();
        return goalSet;
      });

    var sut = CreateAddGoalCommandHandler(goalSetRepository);
    var result = await sut.Handle(CreateCommand(goalSet.Id), CancellationToken.None);

    Assert.True(result.IsSuccess);
    Assert.Empty(result.Errors);
    Assert.Equal(goalSet.TeamId, result.Value.TeamId);
    Assert.Equal(goalSet.PeriodId, result.Value.PeriodId);
    Assert.Equal(2, call);
    await goalSetRepository.Received(1).UpdateAsync(goalSet, Arg.Any<CancellationToken>());
  }

  private static AddGoalCommand CreateCommand(
    int goalSetId = 1,
    string title = "New Goal",
    int min = 1, int mid = 2, int max = 3,
    int percentage = 25,
    GoalValueType? valueType = null) =>
    new(goalSetId, title, GoalType.Team, min, mid, max, valueType ?? GoalValueType.Number, percentage);

  private static GoalSet CreateGoalSet(int teamId, int periodId, int userId, GoalSetStatus? status = null)
  {
    var gs = (GoalSet)Activator.CreateInstance(typeof(GoalSet), nonPublic: true)!;
    SetProp(gs, nameof(GoalSet.TeamId), teamId);
    SetProp(gs, "PeriodId", periodId);
    SetProp(gs, nameof(GoalSet.UserId), userId);
    if (status != null) SetProp(gs, "Status", status);

    var idProp = gs.GetType().GetProperty("Id");
    if (idProp?.CanWrite == true)
    {
      idProp.SetValue(gs, teamId * 1000 + periodId);
    }
    return gs;
  }

  private static void SetProp(object target, string name, object? value)
  {
    var prop = target.GetType().GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
               ?? throw new InvalidOperationException($"Property {name} not found on {target.GetType().Name}");
    prop.SetValue(target, value);
  }

  private static AddGoalCommandHandler CreateAddGoalCommandHandler(IRepository<GoalSet>? goalSetRepository = null)
  {
    return new AddGoalCommandHandler(goalSetRepository ?? Substitute.For<IRepository<GoalSet>>());
  }
}
