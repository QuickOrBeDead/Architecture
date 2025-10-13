using Ardalis.SharedKernel;
using GoalManager.UseCases.GoalManagement; // added for IGoalManagementQueryService
using GoalManager.UseCases.GoalManagement.GetGoalSet;
using GoalManager.UseCases.Identity;
using GoalManager.UseCases.Organisation;
using NSubstitute;
using Xunit;

namespace GoalManager.UseCases.Tests.GoalManagement.GetGoalSet;

[Trait("Category", "GoalManagement/GetGoalSet")]
public sealed class GetGoalSetQueryHandlerTests
{
  [Fact]
  public async Task Handle_Returns_success_with_null_when_goalset_not_found()
  {
    // Arrange
    var goalMgmt = Substitute.For<global::GoalManager.UseCases.GoalManagement.IGoalManagementQueryService>();
    goalMgmt.GetGoalSet(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int>())
      .Returns((GoalSetDto?)null);
    var org = Substitute.For<IOrganisationQueryService>();
    var id = Substitute.For<IIdentityQueryService>();
    var sut = CreateHandler(goalMgmt, org, id);
    var query = new GetGoalSetQuery(TeamId: 10, Year: 2030, UserId: 5);

    // Act
    var result = await sut.Handle(query, CancellationToken.None);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.Null(result.Value);
    await goalMgmt.Received(1).GetGoalSet(query.TeamId, query.Year, query.UserId);
    await org.DidNotReceive().GetTeamNameAsync(Arg.Any<int>());
    await id.DidNotReceive().GetUserName(Arg.Any<int>());
  }

  [Fact]
  public async Task Handle_Succeeds_and_populates_team_and_user_names()
  {
    // Arrange
    var goalSetDto = new GoalSetDto
    {
      Id = 42,
      TeamId = 77,
      UserId = 9001,
      Goals = new List<GoalDto>()
    };
    var goalMgmt = Substitute.For<global::GoalManager.UseCases.GoalManagement.IGoalManagementQueryService>();
    goalMgmt.GetGoalSet(goalSetDto.TeamId, 2040, goalSetDto.UserId)
      .Returns(goalSetDto);

    var org = Substitute.For<IOrganisationQueryService>();
    var teamName = "Team-Alpha";
    org.GetTeamNameAsync(goalSetDto.TeamId).Returns(teamName);

    var id = Substitute.For<IIdentityQueryService>();
    var userName = "user@example.com"; // could be email per other handlers
    id.GetUserName(goalSetDto.UserId).Returns(userName);

    var sut = CreateHandler(goalMgmt, org, id);
    var query = new GetGoalSetQuery(goalSetDto.TeamId, Year: 2040, goalSetDto.UserId);

    // Act
    var result = await sut.Handle(query, CancellationToken.None);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.NotNull(result.Value);
    Assert.Same(goalSetDto, result.Value);
    Assert.Equal(teamName, result.Value!.TeamName);
    Assert.Equal(userName, result.Value.User);
    await goalMgmt.Received(1).GetGoalSet(goalSetDto.TeamId, 2040, goalSetDto.UserId);
    await org.Received(1).GetTeamNameAsync(goalSetDto.TeamId);
    await id.Received(1).GetUserName(goalSetDto.UserId);
  }

  private static GetGoalSetQueryHandler CreateHandler(
    global::GoalManager.UseCases.GoalManagement.IGoalManagementQueryService? g = null,
    IOrganisationQueryService? o = null,
    IIdentityQueryService? i = null)
    => new(g ?? Substitute.For<global::GoalManager.UseCases.GoalManagement.IGoalManagementQueryService>(),
           o ?? Substitute.For<IOrganisationQueryService>(),
           i ?? Substitute.For<IIdentityQueryService>());
}
