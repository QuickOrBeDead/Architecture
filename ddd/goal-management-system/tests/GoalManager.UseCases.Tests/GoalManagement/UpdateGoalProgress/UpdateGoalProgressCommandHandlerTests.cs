using System.Reflection;
using Ardalis.SharedKernel;
using GoalManager.Core.GoalManagement;
using GoalManager.Core.GoalManagement.Specifications;
using GoalManager.UseCases.GoalManagement.UpdateGoalProgress;
using NSubstitute;
using Xunit;

namespace GoalManager.UseCases.Tests.GoalManagement.UpdateGoalProgress;

[Trait("Category", "GoalManagement/UpdateGoalProgress")]
public sealed class UpdateGoalProgressCommandHandlerTests
{
  [Fact]
  public async Task Handle_Returns_error_when_goalset_not_found()
  {
    // Arrange
    var cmd = new UpdateGoalProgressCommand(GoalSetId: 999, GoalId: 10, ActualValue: 25, Comment: "init");
    var repo = Substitute.For<IRepository<GoalSet>>();
    repo.SingleOrDefaultAsync(Arg.Any<GoalSetWithGoalsByGoalSetIdSpec>(), Arg.Any<CancellationToken>())
        .Returns((GoalSet?)null);
    var sut = CreateHandler(repo);

    // Act
    var result = await sut.Handle(cmd, CancellationToken.None);

    // Assert
    Assert.False(result.IsSuccess);
    Assert.Contains(result.Errors, e => e.Contains("GoalSet not found", StringComparison.OrdinalIgnoreCase));
    await repo.DidNotReceive().UpdateAsync(Arg.Any<GoalSet>(), Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task Handle_Returns_error_when_goal_not_found()
  {
    // Arrange
    var goalSet = GoalSet.Create(teamId: 1, periodId: 2025, userId: 5).Value;
    SetId(goalSet, 500);
    var repo = Substitute.For<IRepository<GoalSet>>();
    repo.SingleOrDefaultAsync(Arg.Any<GoalSetWithGoalsByGoalSetIdSpec>(), Arg.Any<CancellationToken>())
        .Returns(goalSet);
    var cmd = new UpdateGoalProgressCommand(GoalSetId: goalSet.Id, GoalId: 12345, ActualValue: 10, Comment: null); // olmayan goal id
    var sut = CreateHandler(repo);

    // Act
    var result = await sut.Handle(cmd, CancellationToken.None);

    // Assert
    Assert.False(result.IsSuccess);
    Assert.Contains(result.Errors, e => e.Contains("Goal not found", StringComparison.OrdinalIgnoreCase));
    await repo.DidNotReceive().UpdateAsync(Arg.Any<GoalSet>(), Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task Handle_Succeeds_and_updates_goal_progress()
  {
    // Arrange
    var goalSet = GoalSet.Create(teamId: 2, periodId: 2030, userId: 7).Value;
    SetId(goalSet, 777);
    var goalValue = GoalValue.Create(10, 50, 100, GoalValueType.Percentage).Value;
    var addGoalResult = goalSet.AddGoal("Increase Sales", GoalType.Team, goalValue, percentage: 100);
    Assert.True(addGoalResult.IsSuccess); // Guard
    var goal = goalSet.Goals.First();
    SetId(goal, 321);

    var repo = Substitute.For<IRepository<GoalSet>>();
    repo.SingleOrDefaultAsync(Arg.Any<GoalSetWithGoalsByGoalSetIdSpec>(), Arg.Any<CancellationToken>())
        .Returns(goalSet);
    var cmd = new UpdateGoalProgressCommand(goalSet.Id, goal.Id, ActualValue: 40, Comment: "Q1");
    var sut = CreateHandler(repo);

    // Act
    var result = await sut.Handle(cmd, CancellationToken.None);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.Empty(result.Errors);
    Assert.Equal(goalSet.Id, result.Value.GoalSetId);
    Assert.Equal(goal.Id, result.Value.GoalId);
    await repo.Received(1).UpdateAsync(goalSet, Arg.Any<CancellationToken>());
  }

  private static void SetId(object entity, int id)
  {
    var prop = entity.GetType().GetProperty("Id", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
    if (prop?.CanWrite == true)
    {
      prop.SetValue(entity, id);
    }
  }

  private static UpdateGoalProgressCommandHandler CreateHandler(IRepository<GoalSet>? repo = null)
    => new(repo ?? Substitute.For<IRepository<GoalSet>>());
}
