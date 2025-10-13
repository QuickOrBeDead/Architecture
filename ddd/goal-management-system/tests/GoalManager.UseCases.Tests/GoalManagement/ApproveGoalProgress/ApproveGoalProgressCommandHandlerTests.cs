using Ardalis.SharedKernel;
using GoalManager.Core.GoalManagement;
using GoalManager.Core.GoalManagement.Specifications;
using GoalManager.UseCases.GoalManagement.ApproveGoalProgress;
using NSubstitute;
using Xunit;

namespace GoalManager.UseCases.Tests.GoalManagement.ApproveGoalProgress;

[Trait("Category", "ApproveGoalProgressCommandHandler")]
public sealed class ApproveGoalProgressCommandHandlerTests
{
  [Fact]
  public async Task Handle_Returns_error_when_goalset_not_found()
  {
    // Arrange
    var cmd = new ApproveGoalProgressCommand(GoalSetId: 123, GoalId: 10);
    var goalSetRepository = Substitute.For<IRepository<GoalSet>>();
    goalSetRepository.SingleOrDefaultAsync(Arg.Any<GoalSetWithGoalsByGoalSetIdSpec>(), Arg.Any<CancellationToken>())
      .Returns((GoalSet?)null);

    var sut = CreateApproveGoalProgressCommandHandler(goalSetRepository);

    // Act
    var result = await sut.Handle(cmd, CancellationToken.None);

    // Assert
    Assert.False(result.IsSuccess);
    Assert.Contains(result.Errors, e => e.Contains("GoalSet not found", StringComparison.OrdinalIgnoreCase));
    await goalSetRepository.DidNotReceive().UpdateAsync(Arg.Any<GoalSet>(), Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task Handle_Returns_error_when_goal_not_found()
  {
    // Arrange
    var goalSet = GoalSet.Create(teamId: 1, periodId: 2025, userId: 5).Value; // Id default (0) in memory
    var goalSetRepository = Substitute.For<IRepository<GoalSet>>();
    goalSetRepository.SingleOrDefaultAsync(Arg.Any<GoalSetWithGoalsByGoalSetIdSpec>(), Arg.Any<CancellationToken>())
      .Returns(goalSet);

    var cmd = new ApproveGoalProgressCommand(GoalSetId: goalSet.Id, GoalId: 999); // 999 yok
    var sut = CreateApproveGoalProgressCommandHandler(goalSetRepository);

    // Act
    var result = await sut.Handle(cmd, CancellationToken.None);

    // Assert
    Assert.False(result.IsSuccess);
    Assert.Contains(result.Errors, e => e.Contains("Goal not found", StringComparison.OrdinalIgnoreCase));
    await goalSetRepository.DidNotReceive().UpdateAsync(Arg.Any<GoalSet>(), Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task Handle_Succeeds_and_updates_goal_progress_status()
  {
    // Arrange
    var goalSet = GoalSet.Create(teamId: 2, periodId: 2030, userId: 7).Value; // Id default (0)

    // Geçerli GoalValue (min < mid < max ve 1-100 aralığında)
    var goalValue = GoalValue.Create(10, 50, 100, GoalValueType.Percentage).Value;
    var addGoalResult = goalSet.AddGoal("Increase Sales", GoalType.Team, goalValue, percentage: 100);
    Assert.True(addGoalResult.IsSuccess); // Guard: test setup

    var goal = goalSet.Goals.First(); // goal.Id default (0)

    // Progress ekle (Onaylanmayı bekleyen)
    var progressResult = goalSet.UpdateGoalProgress(goal.Id, actualValue: 40, comment: "Q1 performance");
    Assert.True(progressResult.IsSuccess);
    Assert.NotNull(goal.GoalProgress); // Guard
    Assert.Equal(GoalProgressStatus.WaitingForApproval, goal.GoalProgress!.Status);
    var goalSetRepository = Substitute.For<IRepository<GoalSet>>();
    goalSetRepository.SingleOrDefaultAsync(Arg.Any<GoalSetWithGoalsByGoalSetIdSpec>(), Arg.Any<CancellationToken>())
      .Returns(goalSet);

    var cmd = new ApproveGoalProgressCommand(goalSet.Id, goal.Id);
    var sut = CreateApproveGoalProgressCommandHandler(goalSetRepository);

    // Act
    var result = await sut.Handle(cmd, CancellationToken.None);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.Empty(result.Errors);
    Assert.Equal(GoalProgressStatus.Approved, goal.GoalProgress!.Status);
    await goalSetRepository.Received(1).UpdateAsync(goalSet, Arg.Any<CancellationToken>());
  }

  private static ApproveGoalProgressCommandHandler CreateApproveGoalProgressCommandHandler(IRepository<GoalSet>? goalSetRepository = null)
  {
    return new ApproveGoalProgressCommandHandler(goalSetRepository ?? Substitute.For<IRepository<GoalSet>>());
  }
}
