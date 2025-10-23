namespace RestaurantManagement.Api.ArchTests;

using NetArchTest.Rules;
using NUnit.Framework;
using System.Reflection;

[TestFixture]
public class NamingConventionTests
{
    private const string RestaurantApiAssembly = "RestaurantManagement.Api";
    private const string FeaturesNamespace = "RestaurantManagement.Api.Features";

    [Test]
    public void Handlers_ShouldHaveCorrectNaming()
    {
        var assembly = Assembly.Load(RestaurantApiAssembly);
        var handlerTypes = Types.InAssembly(assembly)
            .That()
            .ResideInNamespace(FeaturesNamespace)
            .And()
            .ImplementInterface(typeof(Mediator.IRequestHandler<,>))
            .GetTypes();

        var incorrectlyNamedHandlers = handlerTypes
            .Where(type => !type.Name.EndsWith("Handler"))
            .ToList();

        Assert.That(incorrectlyNamedHandlers, Is.Empty, 
            $"All request handlers should end with 'Handler'. " +
            $"Incorrectly named: {string.Join(", ", incorrectlyNamedHandlers.Select(t => t.Name))}");
    }

    [Test]
    public void Endpoints_ShouldHaveCorrectNaming()
    {
        var assembly = Assembly.Load(RestaurantApiAssembly);
        var endpointTypes = Types.InAssembly(assembly)
            .That()
            .ResideInNamespace(FeaturesNamespace)
            .And()
            .AreStatic()
            .GetTypes()
            .Where(type => type.GetMethods(BindingFlags.Public | BindingFlags.Static)
                              .Any(method => method.Name.StartsWith("Map")))
            .ToList();

        var incorrectlyNamedEndpoints = endpointTypes
            .Where(type => !type.Name.EndsWith("Endpoint"))
            .ToList();

        Assert.That(incorrectlyNamedEndpoints, Is.Empty, 
            $"All endpoint classes should end with 'Endpoint'. " +
            $"Incorrectly named: {string.Join(", ", incorrectlyNamedEndpoints.Select(t => t.Name))}");
    }

    [Test]
    public void Queries_ShouldHaveCorrectNaming()
    {
        var assembly = Assembly.Load(RestaurantApiAssembly);
        var queryTypes = Types.InAssembly(assembly)
            .That()
            .ResideInNamespace(FeaturesNamespace)
            .And()
            .ImplementInterface(typeof(Mediator.IRequest<>))
            .GetTypes()
            .Where(type => type.Name.Contains("Get") || type.Name.Contains("List") || type.Name.Contains("Find"))
            .ToList();

        var incorrectlyNamedQueries = queryTypes
            .Where(type => !type.Name.EndsWith("Query"))
            .ToList();

        Assert.That(incorrectlyNamedQueries, Is.Empty, 
            $"All query classes should end with 'Query'. " +
            $"Incorrectly named: {string.Join(", ", incorrectlyNamedQueries.Select(t => t.Name))}");
    }

    [Test]
    public void Commands_ShouldHaveCorrectNaming()
    {
        var assembly = Assembly.Load(RestaurantApiAssembly);
        var commandTypes = Types.InAssembly(assembly)
            .That()
            .ResideInNamespace(FeaturesNamespace)
            .And()
            .ImplementInterface(typeof(Mediator.IRequest<>))
            .GetTypes()
            .Where(type => type.Name.Contains("Create") || 
                          type.Name.Contains("Update") || 
                          type.Name.Contains("Delete") ||
                          type.Name.Contains("Add") ||
                          type.Name.Contains("Remove"))
            .ToList();

        var incorrectlyNamedCommands = commandTypes
            .Where(type => !type.Name.EndsWith("Command"))
            .ToList();

        Assert.That(incorrectlyNamedCommands, Is.Empty, 
            $"All command classes should end with 'Command'. " +
            $"Incorrectly named: {string.Join(", ", incorrectlyNamedCommands.Select(t => t.Name))}");
    }

    [Test]
    public void Validators_ShouldHaveCorrectNaming()
    {
        var result = Types.InAssembly(Assembly.Load(RestaurantApiAssembly))
            .That()
            .ResideInNamespace(FeaturesNamespace)
            .And()
            .Inherit(typeof(FluentValidation.AbstractValidator<>))
            .Should()
            .HaveNameEndingWith("Validator")
            .GetResult();

        Assert.That(result.IsSuccessful, 
            $"All validator classes should end with 'Validator'. " +
            $"Violations: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }

    [Test]
    public void Responses_ShouldHaveCorrectNaming()
    {
        var result = Types.InAssembly(Assembly.Load(RestaurantApiAssembly))
            .That()
            .ResideInNamespace(FeaturesNamespace)
            .And()
            .AreNotInterfaces()
            .And()
            .AreNotAbstract()
            .Should()
            .HaveNameEndingWith("Response")
            .Or()
            .NotHaveNameMatching(@".*Response.*")
            .GetResult();

        // Get types that should be responses but are not named correctly
        var assembly = Assembly.Load(RestaurantApiAssembly);
        var potentialResponseTypes = Types.InAssembly(assembly)
            .That()
            .ResideInNamespace(FeaturesNamespace)
            .GetTypes()
            .Where(type => type.Name.Contains("Response") && !type.Name.EndsWith("Response"))
            .ToList();

        Assert.That(potentialResponseTypes, Is.Empty, 
            $"Response types should end with 'Response'. " +
            $"Incorrectly named: {string.Join(", ", potentialResponseTypes.Select(t => t.Name))}");
    }

    [Test]
    public void DTOs_ShouldHaveCorrectNaming()
    {
        var assembly = Assembly.Load(RestaurantApiAssembly);
        var potentialDtoTypes = Types.InAssembly(assembly)
            .That()
            .ResideInNamespace(FeaturesNamespace)
            .GetTypes()
            .Where(type => type.Name.Contains("Dto") && !type.Name.EndsWith("Dto"))
            .ToList();

        Assert.That(potentialDtoTypes, Is.Empty, 
            $"DTO types should end with 'Dto'. " +
            $"Incorrectly named: {string.Join(", ", potentialDtoTypes.Select(t => t.Name))}");
    }

    [Test]
    public void EndpointMethods_ShouldFollowNamingConvention()
    {
        var assembly = Assembly.Load(RestaurantApiAssembly);
        var endpointTypes = Types.InAssembly(assembly)
            .That()
            .ResideInNamespace(FeaturesNamespace)
            .And()
            .AreStatic()
            .GetTypes()
            .Where(type => type.Name.EndsWith("Endpoint"))
            .ToList();

        var violations = new List<string>();

        foreach (var endpointType in endpointTypes)
        {
            var mapMethods = endpointType.GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Where(method => method.Name.StartsWith("Map"))
                .ToList();

            foreach (var method in mapMethods)
            {
                // Method should be named Map{FeatureName} where FeatureName matches the operation
                var expectedMethodName = endpointType.Name.Replace("Endpoint", "");
                if (!method.Name.Equals($"Map{expectedMethodName}", StringComparison.OrdinalIgnoreCase))
                {
                    violations.Add($"{endpointType.Name}.{method.Name} should be named 'Map{expectedMethodName}'");
                }
            }
        }

        Assert.That(violations, Is.Empty, 
            $"Endpoint mapping methods should follow naming convention. " +
            $"Violations: {string.Join(", ", violations)}");
    }
}