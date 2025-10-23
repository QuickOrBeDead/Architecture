using Ardalis.SharedKernel;
using GoalManager.Core.GoalManagement;
using GoalManager.Core.GoalManagement.Specifications;
using GoalManager.UseCases.GoalManagement.SendGoalSetToApproval;
using NSubstitute;
using Xunit;

namespace GoalManager.UseCases.Tests.GoalManagement.SendGoalSetToApproval;

[Trait("Category", "GoalManagement/SendGoalSetToApproval")]
public sealed class SendGoalSetToApprovalCommandHandlerTests
{
  [Fact]
  public async Task Handle_Returns_error_when_goalset_not_found()
  {
    // Arrange
    var repo = Substitute.For<IRepository<GoalSet>>();
    repo.SingleOrDefaultAsync(Arg.Any<GoalSetWithGoalsByGoalSetIdSpec>(), Arg.Any<CancellationToken>())
        .Returns((GoalSet?)null);
    var sut = CreateHandler(repo);
    var cmd = new SendGoalSetToApprovalCommand(GoalSetId: 999);

    // Act
    var result = await sut.Handle(cmd, CancellationToken.None);

    // Assert
    Assert.False(result.IsSuccess);
    Assert.Contains(result.Errors, e => e.Contains("GoalSet not found", StringComparison.OrdinalIgnoreCase));
    await repo.DidNotReceive().UpdateAsync(Arg.Any<GoalSet>(), Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task Handle_Returns_error_when_not_all_goals_approved()
  {
    // Arrange
    var goalSet = GoalSet.Create(teamId: 10, periodId: 2030, userId: 5).Value;
    var value = GoalValue.Create(10, 50, 100, GoalValueType.Percentage).Value; // fixed: min cannot be 0
    Assert.True(goalSet.AddGoal("G1", GoalType.Team, value, 100).IsSuccess); // goal added but no approved progress

    var repo = Substitute.For<IRepository<GoalSet>>();
    repo.SingleOrDefaultAsync(Arg.Any<GoalSetWithGoalsByGoalSetIdSpec>(), Arg.Any<CancellationToken>())
        .Returns(goalSet);
    var sut = CreateHandler(repo);
    var cmd = new SendGoalSetToApprovalCommand(goalSet.Id);

    // Act
    var result = await sut.Handle(cmd, CancellationToken.None);

    // Assert
    Assert.False(result.IsSuccess);
    Assert.Contains(result.Errors, e => e.Contains("Cannot send goal set to approval", StringComparison.OrdinalIgnoreCase));
    await repo.DidNotReceive().UpdateAsync(Arg.Any<GoalSet>(), Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task Handle_Returns_error_when_goal_percentages_not_equal_100()
  {
    // Arrange
    var goalSet = GoalSet.Create(teamId: 12, periodId: 2031, userId: 6).Value;
    var goalValueResult = GoalValue.Create(10, 50, 100, GoalValueType.Percentage);
    Assert.True(goalValueResult.IsSuccess); // guard
    var value = goalValueResult.Value;
    Assert.True(goalSet.AddGoal("G1", GoalType.Team, value, 90).IsSuccess);
    var goal = goalSet.Goals.First();
    Assert.True(goalSet.UpdateGoalProgress(goal.Id, 80, "progress").IsSuccess);
    Assert.True(goalSet.ApproveGoalProgress(goal.Id).IsSuccess);

    var repo = Substitute.For<IRepository<GoalSet>>();
    repo.SingleOrDefaultAsync(Arg.Any<GoalSetWithGoalsByGoalSetIdSpec>(), Arg.Any<CancellationToken>())
        .Returns(goalSet);
    var sut = CreateHandler(repo);
    var cmd = new SendGoalSetToApprovalCommand(goalSet.Id);

    // Act
    var result = await sut.Handle(cmd, CancellationToken.None);

    // Assert
    Assert.False(result.IsSuccess);
    Assert.Contains(result.Errors, e => e.Contains("sum of all goal percentages", StringComparison.OrdinalIgnoreCase));
    await repo.DidNotReceive().UpdateAsync(Arg.Any<GoalSet>(), Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task Handle_Succeeds_and_updates_repository()
  {
    // Arrange
    var goalSet = BuildGoalSetAllGoalsApproved(teamId: 22, periodId: 2026, ownerUserId: 7);
    Assert.Null(goalSet.Status); // sanity: not yet sent for approval

    var repo = Substitute.For<IRepository<GoalSet>>();
    repo.SingleOrDefaultAsync(Arg.Any<GoalSetWithGoalsByGoalSetIdSpec>(), Arg.Any<CancellationToken>())
        .Returns(goalSet);
    var sut = CreateHandler(repo);
    var cmd = new SendGoalSetToApprovalCommand(goalSet.Id);

    // Act
    var result = await sut.Handle(cmd, CancellationToken.None);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.Empty(result.Errors);
    Assert.Equal(GoalSetStatus.WaitingForApproval, goalSet.Status);
    await repo.Received(1).UpdateAsync(Arg.Is<GoalSet>(g => g == goalSet && g.Status == GoalSetStatus.WaitingForApproval), Arg.Any<CancellationToken>());
  }

  private static SendGoalSetToApprovalCommandHandler CreateHandler(IRepository<GoalSet>? repository = null)
    => new(repository ?? Substitute.For<IRepository<GoalSet>>());

  private static GoalSet BuildGoalSetAllGoalsApproved(int teamId, int periodId, int ownerUserId)
  {
    var gs = GoalSet.Create(teamId, periodId, ownerUserId).Value;

    var goalValue = GoalValue.Create(10, 50, 100, GoalValueType.Percentage).Value;
    Assert.True(gs.AddGoal("Goal 1", GoalType.Team, goalValue, 100).IsSuccess);

    var goal = gs.Goals.First();
    Assert.True(gs.UpdateGoalProgress(goal.Id, 80, "Initial").IsSuccess);
    Assert.True(gs.ApproveGoalProgress(goal.Id).IsSuccess);

    return gs; // not yet sent to approval
  }
}
