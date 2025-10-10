using GoalManager.Core.GoalManagement;
using GoalManager.UseCases.GoalManagement.GetGoalValueTypeLookup;
using Xunit;

namespace GoalManager.UseCases.Tests.GoalManagement.GetGoalValueTypeLookup;

[Trait("Category", "GoalManagement/GetGoalValueTypeLookup")]
public sealed class GetGoalValueTypeLookupQueryHandlerTests
{
  [Fact]
  public async Task Handle_Returns_all_goal_value_types()
  {
    // Arrange
    var sut = CreateHandler();
    var query = new GetGoalValueTypeLookupQuery();

    // Act
    var result = await sut.Handle(query, CancellationToken.None);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.Empty(result.Errors);
    Assert.NotNull(result.Value);
    var expected = GoalValueType.List; // SmartEnum list
    Assert.Equal(expected.Count, result.Value.Count);
    foreach (var gvt in expected)
    {
      Assert.Contains(result.Value, dto => dto.Id == gvt.Value && dto.Name == gvt.Name);
    }
  }

  private static GetGoalValueTypeLookupQueryHandler CreateHandler() => new();
}
