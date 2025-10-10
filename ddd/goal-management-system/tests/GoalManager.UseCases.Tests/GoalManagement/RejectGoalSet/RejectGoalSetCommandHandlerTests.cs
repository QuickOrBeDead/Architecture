using Ardalis.SharedKernel;
using GoalManager.Core.GoalManagement;
using GoalManager.Core.GoalManagement.Specifications;
using GoalManager.UseCases.GoalManagement.RejectGoalSet;
using NSubstitute;
using Xunit;

namespace GoalManager.UseCases.Tests.GoalManagement.RejectGoalSet;

[Trait("Category", "GoalManagement/RejectGoalSet")]
public sealed class RejectGoalSetCommandHandlerTests
{
  [Fact]
  public async Task Handle_Returns_error_when_goalset_not_found()
  {
    // Arrange
    var repo = Substitute.For<IRepository<GoalSet>>();
    repo.SingleOrDefaultAsync(Arg.Any<GoalSetWithGoalsByGoalSetIdSpec>(), Arg.Any<CancellationToken>())
        .Returns((GoalSet?)null);
    var sut = CreateHandler(repo);
    var cmd = new RejectGoalSetCommand(GoalSetId: 123, UserId: 77);

    // Act
    var result = await sut.Handle(cmd, CancellationToken.None);

    // Assert
    Assert.False(result.IsSuccess);
    Assert.Contains(result.Errors, e => e.Contains("GoalSet not found", StringComparison.OrdinalIgnoreCase));
    await repo.DidNotReceive().UpdateAsync(Arg.Any<GoalSet>(), Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task Handle_Returns_error_when_goalset_not_waiting_for_approval()
  {
    // Arrange
    var goalSet = GoalSet.Create(teamId: 10, periodId: 2030, userId: 5).Value; // Status is null (New/None) => cannot reject
    var repo = Substitute.For<IRepository<GoalSet>>();
    repo.SingleOrDefaultAsync(Arg.Any<GoalSetWithGoalsByGoalSetIdSpec>(), Arg.Any<CancellationToken>())
        .Returns(goalSet);
    var sut = CreateHandler(repo);
    var cmd = new RejectGoalSetCommand(goalSet.Id, UserId: 99);

    // Act
    var result = await sut.Handle(cmd, CancellationToken.None);

    // Assert
    Assert.False(result.IsSuccess);
    Assert.Contains(result.Errors, e => e.Contains("Cannot reject goal set", StringComparison.OrdinalIgnoreCase));
    await repo.DidNotReceive().UpdateAsync(Arg.Any<GoalSet>(), Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task Handle_Succeeds_and_updates_repository()
  {
    // Arrange
    var goalSet = BuildGoalSetWaitingForApproval(teamId: 22, periodId: 2026, ownerUserId: 7);
    Assert.Equal(GoalSetStatus.WaitingForApproval, goalSet.Status); // sanity
    var repo = Substitute.For<IRepository<GoalSet>>();
    repo.SingleOrDefaultAsync(Arg.Any<GoalSetWithGoalsByGoalSetIdSpec>(), Arg.Any<CancellationToken>())
        .Returns(goalSet);
    var sut = CreateHandler(repo);
    var rejectorUserId = 1001;
    var cmd = new RejectGoalSetCommand(goalSet.Id, rejectorUserId);

    // Act
    var result = await sut.Handle(cmd, CancellationToken.None);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.Empty(result.Errors);
    Assert.Equal("Goal set is rejected", result.SuccessMessage);
    Assert.Equal(GoalSetStatus.None, goalSet.Status);
    await repo.Received(1).UpdateAsync(Arg.Is<GoalSet>(g => g == goalSet && g.Status == GoalSetStatus.None), Arg.Any<CancellationToken>());
  }

  private static RejectGoalSetCommandHandler CreateHandler(IRepository<GoalSet>? repository = null)
    => new(repository ?? Substitute.For<IRepository<GoalSet>>());

  private static GoalSet BuildGoalSetWaitingForApproval(int teamId, int periodId, int ownerUserId)
  {
    var gs = GoalSet.Create(teamId, periodId, ownerUserId).Value;

    // Add a single goal with 100% so percentages sum = 100 after creation
    var goalValue = GoalValue.Create(10, 50, 100, GoalValueType.Percentage).Value;
    var addResult = gs.AddGoal("Goal 1", GoalType.Team, goalValue, 100);
    Assert.True(addResult.IsSuccess); // sanity

    var goal = gs.Goals.First();

    // Add progress then approve it
    Assert.True(gs.UpdateGoalProgress(goal.Id, actualValue: 80, comment: "Initial").IsSuccess);
    Assert.True(gs.ApproveGoalProgress(goal.Id).IsSuccess);

    // Move to WaitingForApproval state
    var sendResult = gs.SendToApproval();
    Assert.True(sendResult.IsSuccess);

    return gs;
  }
}
