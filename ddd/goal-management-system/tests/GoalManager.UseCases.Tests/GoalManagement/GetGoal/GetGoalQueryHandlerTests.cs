using System.Reflection;
using Ardalis.SharedKernel;
using GoalManager.Core.GoalManagement;
using GoalManager.Core.GoalManagement.Specifications;
using GoalManager.UseCases.GoalManagement.GetGoal;
using NSubstitute;
using Xunit;

namespace GoalManager.UseCases.Tests.GoalManagement.GetGoal;

[Trait("Category", "GoalManagement/GetGoal")]
public sealed class GetGoalQueryHandlerTests
{
  [Fact]
  public async Task Handle_Returns_error_when_goalset_not_found()
  {
    // Arrange
    var repo = Substitute.For<IRepository<GoalSet>>();
    repo.SingleOrDefaultAsync(Arg.Any<GoalSetWithGoalsByGoalSetIdSpec>(), Arg.Any<CancellationToken>())
      .Returns((GoalSet?)null);
    var sut = CreateHandler(repo);
    var query = new GetGoalQuery(GoalSetId: 500, GoalId: 10);

    // Act
    var result = await sut.Handle(query, CancellationToken.None);

    // Assert
    Assert.False(result.IsSuccess);
    Assert.Contains(result.Errors, e => e.Contains("Goal set not found", StringComparison.OrdinalIgnoreCase));
  }

  [Fact]
  public async Task Handle_Returns_error_when_goal_not_found()
  {
    // Arrange
    var goalSet = GoalSet.Create(teamId: 1, periodId: 2025, userId: 9).Value;
    SetId(goalSet, 700);
    var repo = Substitute.For<IRepository<GoalSet>>();
    repo.SingleOrDefaultAsync(Arg.Any<GoalSetWithGoalsByGoalSetIdSpec>(), Arg.Any<CancellationToken>())
      .Returns(goalSet);
    var sut = CreateHandler(repo);
    var query = new GetGoalQuery(GoalSetId: goalSet.Id, GoalId: 999); // no goals added

    // Act
    var result = await sut.Handle(query, CancellationToken.None);

    // Assert
    Assert.False(result.IsSuccess);
    Assert.Contains(result.Errors, e => e.Contains("Goal not found", StringComparison.OrdinalIgnoreCase));
  }

  [Fact]
  public async Task Handle_Succeeds_and_returns_goal()
  {
    // Arrange
    var goalSet = GoalSet.Create(teamId: 5, periodId: 2030, userId: 3).Value;
    SetId(goalSet, 1234);
    var addGoalResult = goalSet.AddGoal("Increase Revenue", GoalType.Team, GoalValue.Create(1, 2, 3, GoalValueType.Number).Value, percentage: 50);
    Assert.True(addGoalResult.IsSuccess); // guard test setup
    var goal = goalSet.Goals.Single();
    SetId(goal, 4321);

    var repo = Substitute.For<IRepository<GoalSet>>();
    repo.SingleOrDefaultAsync(Arg.Any<GoalSetWithGoalsByGoalSetIdSpec>(), Arg.Any<CancellationToken>())
      .Returns(goalSet);

    var sut = CreateHandler(repo);
    var query = new GetGoalQuery(goalSet.Id, goal.Id);

    // Act
    var result = await sut.Handle(query, CancellationToken.None);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.Empty(result.Errors);
    Assert.Equal(goal.Id, result.Value.Id);
    Assert.Same(goal, result.Value);
  }

  private static GetGoalQueryHandler CreateHandler(IRepository<GoalSet>? repo = null)
    => new(repo ?? Substitute.For<IRepository<GoalSet>>());

  private static void SetId(object entity, int id)
  {
    var prop = entity.GetType().GetProperty("Id", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
               ?? throw new InvalidOperationException("Id property not found");
    prop.SetValue(entity, id);
  }
}
