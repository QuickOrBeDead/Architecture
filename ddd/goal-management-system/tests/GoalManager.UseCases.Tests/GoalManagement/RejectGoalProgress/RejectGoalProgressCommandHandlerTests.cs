using System.Reflection;
using Ardalis.SharedKernel;
using GoalManager.Core.GoalManagement;
using GoalManager.Core.GoalManagement.Specifications;
using GoalManager.UseCases.GoalManagement.RejectGoalProgress;
using NSubstitute;
using Xunit;

namespace GoalManager.UseCases.Tests.GoalManagement.RejectGoalProgress;

[Trait("Category", "RejectGoalProgressCommandHandler")]
public sealed class RejectGoalProgressCommandHandlerTests
{
  [Fact]
  public async Task Handle_Returns_error_when_goalset_not_found()
  {
    // Arrange
    var cmd = new RejectGoalProgressCommand(GoalSetId: 500, GoalId: 10);
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
    var goalSet = GoalSet.Create(teamId: 1, periodId: 2025, userId: 42).Value;
    SetId(goalSet, 1234);
    var repo = Substitute.For<IRepository<GoalSet>>();
    repo.SingleOrDefaultAsync(Arg.Any<GoalSetWithGoalsByGoalSetIdSpec>(), Arg.Any<CancellationToken>())
        .Returns(goalSet);
    var cmd = new RejectGoalProgressCommand(GoalSetId: goalSet.Id, GoalId: 9999); // olmayan goal
    var sut = CreateHandler(repo);

    // Act
    var result = await sut.Handle(cmd, CancellationToken.None);

    // Assert
    Assert.False(result.IsSuccess);
    Assert.Contains(result.Errors, e => e.Contains("Goal not found", StringComparison.OrdinalIgnoreCase));
    await repo.DidNotReceive().UpdateAsync(Arg.Any<GoalSet>(), Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task Handle_Returns_error_when_progress_record_not_found()
  {
    // Arrange
    var goalSet = GoalSet.Create(teamId: 10, periodId: 2030, userId: 7).Value;
    SetId(goalSet, 600);
    var goalValue = GoalValue.Create(10, 50, 100, GoalValueType.Percentage).Value;
    Assert.True(goalSet.AddGoal("Revenue", GoalType.Team, goalValue, 100).IsSuccess);
    var goal = goalSet.Goals.First();
    SetId(goal, 901); // no progress added for this goal

    var repo = Substitute.For<IRepository<GoalSet>>();
    repo.SingleOrDefaultAsync(Arg.Any<GoalSetWithGoalsByGoalSetIdSpec>(), Arg.Any<CancellationToken>())
        .Returns(goalSet);
    var cmd = new RejectGoalProgressCommand(goalSet.Id, goal.Id);
    var sut = CreateHandler(repo);

    // Act
    var result = await sut.Handle(cmd, CancellationToken.None);

    // Assert
    Assert.False(result.IsSuccess);
    Assert.Contains(result.Errors, e => e.Contains("No progress record found", StringComparison.OrdinalIgnoreCase));
    await repo.DidNotReceive().UpdateAsync(Arg.Any<GoalSet>(), Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task Handle_Succeeds_and_updates_repository_with_rejected_status()
  {
    // Arrange
    var goalSet = GoalSet.Create(teamId: 2, periodId: 2035, userId: 9).Value;
    SetId(goalSet, 888);
    var goalValue = GoalValue.Create(5, 50, 95, GoalValueType.Percentage).Value;
    Assert.True(goalSet.AddGoal("Customer Satisfaction", GoalType.Team, goalValue, 100).IsSuccess);
    var goal = goalSet.Goals.First();
    SetId(goal, 4321);

    // Add a progress entry (WaitingForApproval by default)
    Assert.True(goalSet.UpdateGoalProgress(goal.Id, actualValue: 60, comment: "Initial").IsSuccess);
    Assert.NotNull(goal.GoalProgress);
    Assert.Equal(GoalProgressStatus.WaitingForApproval, goal.GoalProgress!.Status);

    var repo = Substitute.For<IRepository<GoalSet>>();
    repo.SingleOrDefaultAsync(Arg.Any<GoalSetWithGoalsByGoalSetIdSpec>(), Arg.Any<CancellationToken>())
        .Returns(goalSet);
    var sut = CreateHandler(repo);
    var cmd = new RejectGoalProgressCommand(goalSet.Id, goal.Id);

    // Act
    var result = await sut.Handle(cmd, CancellationToken.None);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.Empty(result.Errors);
    Assert.Equal(GoalProgressStatus.Rejected, goal.GoalProgress!.Status);
    await repo.Received(1).UpdateAsync(Arg.Is<GoalSet>(gs => gs == goalSet), Arg.Any<CancellationToken>());
  }

  private static RejectGoalProgressCommandHandler CreateHandler(IRepository<GoalSet>? repo = null)
    => new(repo ?? Substitute.For<IRepository<GoalSet>>());

  private static void SetId(object entity, int id)
  {
    var prop = entity.GetType().GetProperty("Id", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
    if (prop?.CanWrite == true)
    {
      prop.SetValue(entity, id);
    }
  }
}
