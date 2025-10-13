using Ardalis.SharedKernel;
using GoalManager.Core.GoalManagement;
using GoalManager.Core.GoalManagement.Specifications;
using GoalManager.UseCases.GoalManagement.ApproveGoalSet;
using NSubstitute;
using Xunit;

namespace GoalManager.UseCases.Tests.GoalManagement.ApproveGoalSet;

[Trait("Category", "GoalManagement/ApproveGoalSet")]
public sealed class ApproveGoalSetCommandHandlerTests
{
  [Fact]
  public async Task Handle_Returns_error_when_goalset_not_found()
  {
    // Arrange
    var repo = Substitute.For<IRepository<GoalSet>>();
    repo.SingleOrDefaultAsync(Arg.Any<GoalSetWithGoalsByGoalSetIdSpec>(), Arg.Any<CancellationToken>())
        .Returns((GoalSet?)null);
    var sut = CreateHandler(repo);
    var cmd = new ApproveGoalSetCommand(GoalSetId: 123, UserId: 42);

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
    var goalSet = GoalSet.Create(teamId: 10, periodId: 2030, userId: 5).Value; // New status -> cannot approve
    var repo = Substitute.For<IRepository<GoalSet>>();
    repo.SingleOrDefaultAsync(Arg.Any<GoalSetWithGoalsByGoalSetIdSpec>(), Arg.Any<CancellationToken>())
        .Returns(goalSet);
    var sut = CreateHandler(repo);
    var cmd = new ApproveGoalSetCommand(goalSet.Id, UserId: 99);

    // Act
    var result = await sut.Handle(cmd, CancellationToken.None);

    // Assert
    Assert.False(result.IsSuccess);
    Assert.Contains(result.Errors, e => e.Contains("Cannot approve goal set", StringComparison.OrdinalIgnoreCase));
    await repo.DidNotReceive().UpdateAsync(Arg.Any<GoalSet>(), Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task Handle_Succeeds_and_updates_repository()
  {
    // Arrange
    var goalSet = BuildGoalSetReadyForApproval(teamId: 22, periodId: 2026, ownerUserId: 7);
    var repo = Substitute.For<IRepository<GoalSet>>();
    repo.SingleOrDefaultAsync(Arg.Any<GoalSetWithGoalsByGoalSetIdSpec>(), Arg.Any<CancellationToken>())
        .Returns(goalSet);
    var sut = CreateHandler(repo);
    var approverUserId = 1001;
    var cmd = new ApproveGoalSetCommand(goalSet.Id, approverUserId);

    // Act
    var result = await sut.Handle(cmd, CancellationToken.None);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.Empty(result.Errors);
    Assert.Equal("Goal set is approved", result.SuccessMessage);
    await repo.Received(1).UpdateAsync(Arg.Is<GoalSet>(g => g == goalSet && g.Status == GoalSetStatus.Approved), Arg.Any<CancellationToken>());
  }

  private static ApproveGoalSetCommandHandler CreateHandler(IRepository<GoalSet>? repository = null)
    => new(repository ?? Substitute.For<IRepository<GoalSet>>());

  private static GoalSet BuildGoalSetReadyForApproval(int teamId, int periodId, int ownerUserId)
  {
    var gs = GoalSet.Create(teamId, periodId, ownerUserId).Value;

    // Add a single goal with 100% so percentages sum = 100 after creation
    var goalValue = GoalValue.Create(10, 50, 100, GoalValueType.Percentage).Value; // valid percentage values
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
