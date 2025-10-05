using System.Reflection;
using Ardalis.SharedKernel;
using GoalManager.Core.GoalManagement;
using GoalManager.Core.GoalManagement.Specifications;
using GoalManager.UseCases.GoalManagement.AddGoalPeriod;
using NSubstitute;
using Xunit;

namespace GoalManager.UseCases.Tests.GoalManagement.AddGoalPeriod;

[Trait("Category", "AddGoalPeriodCommandHandler")]
public sealed class AddGoalPeriodCommandHandlerTests
{
  [Fact]
  public async Task Handle_Returns_error_when_period_already_exists()
  {
    // Arrange
    var cmd = new AddGoalPeriodCommand(TeamId: 10, UserId: 1, Year: 2025);
    var goalPeriodRepository = Substitute.For<IRepository<GoalPeriod>>();
    goalPeriodRepository.AnyAsync(Arg.Any<GoalPeriodByTeamIdAndYearSpec>(), Arg.Any<CancellationToken>())
      .Returns(true);

    var sut = CreateAddGoalPeriodCommandHandler(goalPeriodRepository);

    // Act
    var result = await sut.Handle(cmd, CancellationToken.None);

    // Assert
    Assert.False(result.IsSuccess);
    Assert.Contains(result.Errors, e => e.Contains("already exists", StringComparison.OrdinalIgnoreCase));
    await goalPeriodRepository.DidNotReceive().AddAsync(Arg.Any<GoalPeriod>(), Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task Handle_Succeeds_and_adds_new_period()
  {
    // Arrange
    var cmd = new AddGoalPeriodCommand(TeamId: 22, UserId: 5, Year: 2030);
    const int expectedId = 9876;
    var goalPeriodRepository = Substitute.For<IRepository<GoalPeriod>>();
    goalPeriodRepository.AnyAsync(Arg.Any<GoalPeriodByTeamIdAndYearSpec>(), Arg.Any<CancellationToken>())
      .Returns(false);

    goalPeriodRepository.AddAsync(Arg.Any<GoalPeriod>(), Arg.Any<CancellationToken>())
      .Returns(ci =>
      {
        var period = (GoalPeriod)ci[0]!;
        SetId(period, expectedId);
        return period;
      });

    var sut = CreateAddGoalPeriodCommandHandler(goalPeriodRepository);

    // Act
    var result = await sut.Handle(cmd, CancellationToken.None);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.Empty(result.Errors);
    Assert.Equal(expectedId, result.Value);
    await goalPeriodRepository.Received(1).AnyAsync(Arg.Any<GoalPeriodByTeamIdAndYearSpec>(), Arg.Any<CancellationToken>());
    await goalPeriodRepository.Received(1).AddAsync(Arg.Is<GoalPeriod>(p => GetProp<int>(p,(nameof(GoalPeriod.TeamId))) == cmd.TeamId && GetProp<int>(p, nameof(GoalPeriod.Year)) == cmd.Year), Arg.Any<CancellationToken>());
  }

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

  private static AddGoalPeriodCommandHandler CreateAddGoalPeriodCommandHandler(IRepository<GoalPeriod>? goalPeriodRepository = null)
  {
    return new AddGoalPeriodCommandHandler(goalPeriodRepository ?? Substitute.For<IRepository<GoalPeriod>>());
  }
}
