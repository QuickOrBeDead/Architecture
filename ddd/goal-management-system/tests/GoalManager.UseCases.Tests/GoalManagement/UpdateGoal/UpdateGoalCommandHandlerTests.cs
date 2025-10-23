using System.Reflection;
using Ardalis.SharedKernel;
using GoalManager.Core.GoalManagement;
using GoalManager.Core.GoalManagement.Specifications;
using GoalManager.Core.Exceptions;
using GoalManager.UseCases.GoalManagement.UpdateGoal;
using NSubstitute;
using Xunit;

namespace GoalManager.UseCases.Tests.GoalManagement.UpdateGoal;

[Trait("Category", "GoalManagement/UpdateGoal")]
public sealed class UpdateGoalCommandHandlerTests
{
  [Fact]
  public async Task Handle_Returns_error_when_goalset_not_found()
  {
    // Arrange
    var repo = Substitute.For<IRepository<GoalSet>>();
    repo.SingleOrDefaultAsync(Arg.Any<GoalSetWithGoalsByGoalSetIdSpec>(), Arg.Any<CancellationToken>())
      .Returns((GoalSet?)null);
    var sut = CreateHandler(repo);
    var cmd = new UpdateGoalCommand(GoalSetId: 123, GoalId: 10, Title: "New Title", GoalType.Team, GoalValueType.Percentage, Percentage: 50);

    // Act
    var result = await sut.Handle(cmd, CancellationToken.None);

    // Assert
    Assert.False(result.IsSuccess);
    Assert.Contains(result.Errors, e => e.Contains("Goal set not found", StringComparison.OrdinalIgnoreCase));
    await repo.DidNotReceive().UpdateAsync(Arg.Any<GoalSet>(), Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task Handle_Returns_error_when_goal_not_found_in_goalset()
  {
    // Arrange
    var goalSet = GoalSet.Create(teamId: 1, periodId: 2025, userId: 10).Value;
    SetId(goalSet, 500);
    var repo = Substitute.For<IRepository<GoalSet>>();
    repo.SingleOrDefaultAsync(Arg.Any<GoalSetWithGoalsByGoalSetIdSpec>(), Arg.Any<CancellationToken>())
      .Returns(goalSet);
    var cmd = new UpdateGoalCommand(goalSet.Id, GoalId: 999, Title: "Title", GoalType.Team, GoalValueType.Percentage, Percentage: 30);
    var sut = CreateHandler(repo);

    // Act
    var result = await sut.Handle(cmd, CancellationToken.None);

    // Assert
    Assert.False(result.IsSuccess);
    Assert.Contains(result.Errors, e => e.Contains("Goal not found", StringComparison.OrdinalIgnoreCase));
    await repo.DidNotReceive().UpdateAsync(Arg.Any<GoalSet>(), Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task Handle_Returns_error_when_goalset_status_prevents_update()
  {
    // Arrange
    var goalSet = GoalSet.Create(teamId: 2, periodId: 2030, userId: 7).Value;
    SetId(goalSet, 700);
    var goalValue = GoalValue.Create(10, 50, 100, GoalValueType.Percentage).Value;
    Assert.True(goalSet.AddGoal("G1", GoalType.Team, goalValue, 100).IsSuccess);
    var goal = goalSet.Goals.First();
    SetId(goal, 111);

    // Force status to WaitingForApproval (private setter) -> cannot update
    SetStatus(goalSet, GoalSetStatus.WaitingForApproval);

    var repo = Substitute.For<IRepository<GoalSet>>();
    repo.SingleOrDefaultAsync(Arg.Any<GoalSetWithGoalsByGoalSetIdSpec>(), Arg.Any<CancellationToken>())
      .Returns(goalSet);
    var cmd = new UpdateGoalCommand(goalSet.Id, goal.Id, Title: "Updated", GoalType.Team, GoalValueType.Percentage, Percentage: 100);
    var sut = CreateHandler(repo);

    // Act
    var result = await sut.Handle(cmd, CancellationToken.None);

    // Assert
    Assert.False(result.IsSuccess);
    Assert.Contains(result.Errors, e => e.Contains("Cannot update goal", StringComparison.OrdinalIgnoreCase));
    await repo.DidNotReceive().UpdateAsync(Arg.Any<GoalSet>(), Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task Handle_Returns_error_when_total_percentage_would_exceed_100()
  {
    // Arrange
    var goalSet = GoalSet.Create(teamId: 3, periodId: 2031, userId: 8).Value;
    SetId(goalSet, 800);
    var value = GoalValue.Create(10, 50, 100, GoalValueType.Percentage).Value;
    Assert.True(goalSet.AddGoal("G1", GoalType.Team, value, 60).IsSuccess);
    Assert.True(goalSet.AddGoal("G2", GoalType.Team, value, 40).IsSuccess);
    var goal = goalSet.Goals.First();
    SetId(goal, 222);

    var repo = Substitute.For<IRepository<GoalSet>>();
    repo.SingleOrDefaultAsync(Arg.Any<GoalSetWithGoalsByGoalSetIdSpec>(), Arg.Any<CancellationToken>())
      .Returns(goalSet);
    // Attempt to change first goal from 60 -> 70 => (100 - 60) + 70 = 110 > 100
    var cmd = new UpdateGoalCommand(goalSet.Id, goal.Id, Title: "G1 Updated", GoalType.Team, GoalValueType.Percentage, Percentage: 70);
    var sut = CreateHandler(repo);

    // Act
    var result = await sut.Handle(cmd, CancellationToken.None);

    // Assert
    Assert.False(result.IsSuccess);
    Assert.Contains(result.Errors, e => e.Contains("Total percentage", StringComparison.OrdinalIgnoreCase));
    await repo.DidNotReceive().UpdateAsync(Arg.Any<GoalSet>(), Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task Handle_Succeeds_and_updates_repository()
  {
    // Arrange
    var goalSet = GoalSet.Create(teamId: 4, periodId: 2032, userId: 9).Value;
    SetId(goalSet, 900);
    var value = GoalValue.Create(10, 50, 100, GoalValueType.Percentage).Value;
    Assert.True(goalSet.AddGoal("Original", GoalType.Team, value, 100).IsSuccess);
    var goal = goalSet.Goals.First();
    SetId(goal, 333);

    var repo = Substitute.For<IRepository<GoalSet>>();
    repo.SingleOrDefaultAsync(Arg.Any<GoalSetWithGoalsByGoalSetIdSpec>(), Arg.Any<CancellationToken>())
      .Returns(goalSet);

    var cmd = new UpdateGoalCommand(goalSet.Id, goal.Id, Title: "Updated Title", GoalType.Team, GoalValueType.Percentage, Percentage: 100);
    var sut = CreateHandler(repo);

    // Act
    var result = await sut.Handle(cmd, CancellationToken.None);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.Empty(result.Errors);
    Assert.Equal((goalSet.Id, goal.Id), result.Value);
    Assert.Equal("Updated Title", goal.Title);
    Assert.Equal(GoalType.Team, goal.GoalType);
    Assert.Equal(GoalValueType.Percentage, goal.GoalValue.GoalValueType);
    await repo.Received(1).UpdateAsync(goalSet, Arg.Any<CancellationToken>());
  }

  private static UpdateGoalCommandHandler CreateHandler(IRepository<GoalSet>? repo = null)
    => new(repo ?? Substitute.For<IRepository<GoalSet>>());

  private static void SetId(object entity, int id)
  {
    var prop = entity.GetType().GetProperty("Id", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
    prop?.SetValue(entity, id);
  }

  private static void SetStatus(GoalSet goalSet, GoalSetStatus status)
  {
    var prop = typeof(GoalSet).GetProperty("Status", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
    prop?.SetValue(goalSet, status);
  }
}
