using GoalManager.UseCases.GoalManagement; // for IGoalManagementQueryService & DTOs
using GoalManager.UseCases.GoalManagement.GetTeamGoalSetListsOfTeamLeader;
using GoalManager.UseCases.Identity;
using GoalManager.UseCases.Organisation;
using GoalManager.UseCases.PerformanceEvaluation;
using GoalManager.UseCases.Organisation.ListUserTeams; // TeamLookupItemDto namespace
using GoalManager.UseCases.PerformanceEvaluation.GetRelatedGoalSetEvaluations; // GoalSetEvaluationPairDto namespace
using NSubstitute;
using Xunit;

namespace GoalManager.UseCases.Tests.GoalManagement.GetTeamGoalSetListsOfTeamLeader;

[Trait("Category", "GoalManagement/GetTeamGoalSetListsOfTeamLeader")]
public sealed class GetTeamGoalSetListsOfTeamLeaderQueryHandlerTests
{
  [Fact]
  public async Task Handle_Returns_empty_list_when_team_leader_has_no_teams()
  {
    // Arrange
    var goalMgmt = Substitute.For<IGoalManagementQueryService>();
    var org = Substitute.For<IOrganisationQueryService>();
    var perf = Substitute.For<IPerformanceEvaluationQueryService>();
    var id = Substitute.For<IIdentityQueryService>();

    org.ListTeamLeaderTeams(Arg.Any<int>())
       .Returns(Task.FromResult(new List<TeamLookupItemDto>())); // no teams
    goalMgmt.GetTeamMemberGoalSetsList(Arg.Any<IList<int>>())
            .Returns(Task.FromResult(new List<TeamMemberGoalSetListItemDto>())); // no goal sets
    org.GetTeamNamesAsync(Arg.Any<List<int>>())
       .Returns(Task.FromResult(new Dictionary<int, string>()));
    id.GetUserEmails(Arg.Any<IList<int>>())
      .Returns(Task.FromResult(new Dictionary<int, string>()));
    perf.GetRelatedGoalSetEvaluationsAsync(Arg.Any<List<int>>(), Arg.Any<CancellationToken>())
        .Returns(Task.FromResult(new List<GoalSetEvaluationPairDto>()));

    var sut = CreateHandler(goalMgmt, org, perf, id);
    var query = new GetTeamGoalSetListsOfTeamLeaderQuery(TeamLeaderUserId: 1234);

    // Act
    var result = await sut.Handle(query, CancellationToken.None);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.NotNull(result.Value);
    Assert.Empty(result.Value);
    await org.Received(1).ListTeamLeaderTeams(query.TeamLeaderUserId);
    await goalMgmt.Received(1).GetTeamMemberGoalSetsList(Arg.Is<IList<int>>(l => l.Count == 0));
    await org.Received(1).GetTeamNamesAsync(Arg.Is<List<int>>(l => l.Count == 0));
    await id.Received(1).GetUserEmails(Arg.Is<IList<int>>(l => l.Count == 0));
    await perf.Received(1).GetRelatedGoalSetEvaluationsAsync(Arg.Is<List<int>>(l => l.Count == 0), Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task Handle_Groups_goal_sets_and_populates_lookup_fields()
  {
    // Arrange
    var goalMgmt = Substitute.For<IGoalManagementQueryService>();
    var org = Substitute.For<IOrganisationQueryService>();
    var perf = Substitute.For<IPerformanceEvaluationQueryService>();
    var id = Substitute.For<IIdentityQueryService>();

    var leaderTeams = new List<TeamLookupItemDto>
    {
      new() { TeamId = 1, TeamName = "T1"},
      new() { TeamId = 2, TeamName = "T2"}
    };
    org.ListTeamLeaderTeams(Arg.Any<int>())
       .Returns(Task.FromResult(leaderTeams));

    var teamGoalSets = new List<TeamMemberGoalSetListItemDto>
    {
      new() { GoalSetId = 101, TeamId = 1, UserId = 10 },
      new() { GoalSetId = 102, TeamId = 1, UserId = 11 },
      new() { GoalSetId = 201, TeamId = 2, UserId = 12 }
    };
    goalMgmt.GetTeamMemberGoalSetsList(Arg.Is<IList<int>>(l => l.SequenceEqual(new[] {1,2})))
            .Returns(Task.FromResult(teamGoalSets));

    org.GetTeamNamesAsync(Arg.Is<List<int>>(l => l.SequenceEqual(new[] {1,2})))
       .Returns(Task.FromResult(new Dictionary<int,string>{{1,"Team-One"},{2,"Team-Two"}}));

    id.GetUserEmails(Arg.Is<IList<int>>(l => l.OrderBy(x=>x).SequenceEqual(new[] {10,11,12})))
      .Returns(Task.FromResult(new Dictionary<int,string>{{10,"u10@example.com"},{11,"u11@example.com"},{12,"u12@example.com"}}));

    perf.GetRelatedGoalSetEvaluationsAsync(Arg.Is<List<int>>(l => l.OrderBy(x=>x).SequenceEqual(new[] {101,102,201})), Arg.Any<CancellationToken>())
        .Returns(Task.FromResult(new List<GoalSetEvaluationPairDto>
        {
          new() { GoalSetId = 101, GoalSetEvaluationId = 5000 },
          new() { GoalSetId = 201, GoalSetEvaluationId = 6000 }
        })); // note: 102 intentionally missing

    var sut = CreateHandler(goalMgmt, org, perf, id);
    var query = new GetTeamGoalSetListsOfTeamLeaderQuery(TeamLeaderUserId: 900);

    // Act
    var result = await sut.Handle(query, CancellationToken.None);

    // Assert
    Assert.True(result.IsSuccess);
    var value = result.Value;
    Assert.Equal(2, value.Count); // two teams with goal sets

    var team1 = Assert.Single(value, t => t.TeamId == 1);
    Assert.Equal("Team-One", team1.TeamName);
    Assert.Equal(2, team1.GoalSets.Count);
    var gs101 = team1.GoalSets.Single(gs => gs.Id == 101);
    Assert.Equal(5000, gs101.GoalSetEvaluationId); // evaluation present
    Assert.Equal("u10@example.com", gs101.User);
    var gs102 = team1.GoalSets.Single(gs => gs.Id == 102);
    Assert.Null(gs102.GoalSetEvaluationId); // missing evaluation
    Assert.Equal("u11@example.com", gs102.User);

    var team2 = Assert.Single(value, t => t.TeamId == 2);
    Assert.Equal("Team-Two", team2.TeamName);
    var gs201 = Assert.Single(team2.GoalSets);
    Assert.Equal(6000, gs201.GoalSetEvaluationId);
    Assert.Equal("u12@example.com", gs201.User);

    await perf.Received(1).GetRelatedGoalSetEvaluationsAsync(Arg.Any<List<int>>(), Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task Handle_Assigns_empty_strings_and_null_when_lookup_data_missing()
  {
    // Arrange
    var goalMgmt = Substitute.For<IGoalManagementQueryService>();
    var org = Substitute.For<IOrganisationQueryService>();
    var perf = Substitute.For<IPerformanceEvaluationQueryService>();
    var id = Substitute.For<IIdentityQueryService>();

    var leaderTeams = new List<TeamLookupItemDto> { new() { TeamId = 1, TeamName = "Ignored" } }; // team name dictionary will be empty -> expect empty string
    org.ListTeamLeaderTeams(Arg.Any<int>())
       .Returns(Task.FromResult(leaderTeams));

    var teamGoalSets = new List<TeamMemberGoalSetListItemDto>
    {
      new() { GoalSetId = 555, TeamId = 1, UserId = 999 }
    };
    goalMgmt.GetTeamMemberGoalSetsList(Arg.Any<IList<int>>())
            .Returns(Task.FromResult(teamGoalSets));

    org.GetTeamNamesAsync(Arg.Any<List<int>>())
       .Returns(Task.FromResult(new Dictionary<int,string>())); // missing team name
    id.GetUserEmails(Arg.Any<IList<int>>())
      .Returns(Task.FromResult(new Dictionary<int,string>())); // missing user email
    perf.GetRelatedGoalSetEvaluationsAsync(Arg.Any<List<int>>(), Arg.Any<CancellationToken>())
        .Returns(Task.FromResult(new List<GoalSetEvaluationPairDto>())); // no evaluation entries

    var sut = CreateHandler(goalMgmt, org, perf, id);
    var query = new GetTeamGoalSetListsOfTeamLeaderQuery(TeamLeaderUserId: 42);

    // Act
    var result = await sut.Handle(query, CancellationToken.None);

    // Assert
    Assert.True(result.IsSuccess);
    var value = result.Value;
    var team = Assert.Single(value);
    Assert.Equal(1, team.TeamId);
    Assert.Equal(string.Empty, team.TeamName); // missing -> empty string
    var gs = Assert.Single(team.GoalSets);
    Assert.Null(gs.GoalSetEvaluationId);
    Assert.Equal(string.Empty, gs.User); // missing -> empty string
  }

  private static GetTeamGoalSetListsOfTeamLeaderQueryHandler CreateHandler(
    IGoalManagementQueryService? goalMgmt = null,
    IOrganisationQueryService? org = null,
    IPerformanceEvaluationQueryService? perf = null,
    IIdentityQueryService? id = null)
    => new(goalMgmt ?? Substitute.For<IGoalManagementQueryService>(),
           org ?? Substitute.For<IOrganisationQueryService>(),
           perf ?? Substitute.For<IPerformanceEvaluationQueryService>(),
           id ?? Substitute.For<IIdentityQueryService>());
}
