using GoalManager.Core.GoalManagement;
using GoalManager.UseCases.GoalManagement.GetGoalTypeLookup;
using Xunit;

namespace GoalManager.UseCases.Tests.GoalManagement.GetGoalTypeLookup;

[Trait("Category", "GoalManagement/GetGoalTypeLookup")]
public sealed class GetGoalTypeLookupQueryHandlerTests
{
  [Fact]
  public async Task Handle_Returns_all_goal_types()
  {
    // Arrange
    var sut = CreateHandler();
    var query = new GetGoalTypeLookupQuery();

    // Act
    var result = await sut.Handle(query, CancellationToken.None);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.Empty(result.Errors);
    Assert.NotNull(result.Value);
    var expected = GoalType.List; // SmartEnum list
    Assert.Equal(expected.Count, result.Value.Count);
    foreach (var gt in expected)
    {
      Assert.Contains(result.Value, dto => dto.Id == gt.Value && dto.Name == gt.Name);
    }
  }

  private static GetGoalTypeLookupQueryHandler CreateHandler() => new();
}
