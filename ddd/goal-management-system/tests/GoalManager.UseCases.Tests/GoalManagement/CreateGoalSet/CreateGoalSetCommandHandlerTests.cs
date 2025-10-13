using System.Reflection;
using Ardalis.SharedKernel;
using GoalManager.Core.GoalManagement;
using GoalManager.Core.GoalManagement.Specifications;
using GoalManager.UseCases.GoalManagement.CreateGoalSet;
using NSubstitute;
using Xunit;

namespace GoalManager.UseCases.Tests.GoalManagement.CreateGoalSet;

[Trait("Category", "GoalManagement/CreateGoalSet")]
public sealed class CreateGoalSetCommandHandlerTests
{
  [Fact]
  public async Task Handle_Returns_error_when_goal_period_not_found()
  {
    // Arrange
    var periodRepo = Substitute.For<IRepository<GoalPeriod>>();
    periodRepo.SingleOrDefaultAsync(Arg.Any<GoalPeriodByTeamIdAndYearSpec>(), Arg.Any<CancellationToken>())
      .Returns((GoalPeriod?)null);

    var goalSetRepo = Substitute.For<IRepository<GoalSet>>();
    var sut = CreateHandler(periodRepo, goalSetRepo);
    var cmd = new CreateGoalSetCommand(TeamId: 10, UserId: 5, Year: 2035);

    // Act
    var result = await sut.Handle(cmd, CancellationToken.None);

    // Assert
    Assert.False(result.IsSuccess);
    Assert.Contains(result.Errors, e => e.Contains("goal period is not created", StringComparison.OrdinalIgnoreCase));
    await goalSetRepo.DidNotReceive().AddAsync(Arg.Any<GoalSet>(), Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task Handle_Succeeds_and_creates_goalset()
  {
    // Arrange
    var cmd = new CreateGoalSetCommand(TeamId: 22, UserId: 99, Year: 2040);

    var periodRepo = Substitute.For<IRepository<GoalPeriod>>();
    var goalSetRepo = Substitute.For<IRepository<GoalSet>>();

    var period = GoalPeriod.Create(cmd.TeamId, cmd.Year).Value;
    SetId(period, 5555);

    periodRepo.SingleOrDefaultAsync(Arg.Any<GoalPeriodByTeamIdAndYearSpec>(), Arg.Any<CancellationToken>())
      .Returns(period);

    goalSetRepo.AddAsync(Arg.Any<GoalSet>(), Arg.Any<CancellationToken>())
      .Returns(ci => Task.FromResult((GoalSet)ci[0]!));

    var sut = CreateHandler(periodRepo, goalSetRepo);

    // Act
    var result = await sut.Handle(cmd, CancellationToken.None);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.Empty(result.Errors);

    await periodRepo.Received(1).SingleOrDefaultAsync(Arg.Any<GoalPeriodByTeamIdAndYearSpec>(), Arg.Any<CancellationToken>());
    await goalSetRepo.Received(1).AddAsync(Arg.Is<GoalSet>(gs =>
        GetProp<int>(gs, nameof(GoalSet.TeamId)) == cmd.TeamId &&
        GetProp<int>(gs, nameof(GoalSet.PeriodId)) == period.Id &&
        GetProp<int>(gs, nameof(GoalSet.UserId)) == cmd.UserId
      ), Arg.Any<CancellationToken>());
  }

  private static CreateGoalSetCommandHandler CreateHandler(IRepository<GoalPeriod>? periodRepo = null, IRepository<GoalSet>? goalSetRepo = null)
    => new(periodRepo ?? Substitute.For<IRepository<GoalPeriod>>(), goalSetRepo ?? Substitute.For<IRepository<GoalSet>>());

  private static void SetId(object entity, int id)
  {
    var prop = entity.GetType().GetProperty("Id", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
    if (prop?.CanWrite == true)
    {
      prop.SetValue(entity, id);
    }
  }

  private static T GetProp<T>(object entity, string name)
  {
    var prop = entity.GetType().GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
               ?? throw new InvalidOperationException($"Property {name} not found on {entity.GetType().Name}");
    return (T)prop.GetValue(entity)!;
  }
}
