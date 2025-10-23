namespace RestaurantManagement.Api.ArchTests;

using NetArchTest.Rules;
using NUnit.Framework;
using System.Reflection;

[TestFixture]
public class StructuralTests
{
    private const string RestaurantApiAssembly = "RestaurantManagement.Api";
    private const string FeaturesNamespace = "RestaurantManagement.Api.Features";

    [Test]
    public void EachFeature_ShouldHaveAtLeastOneEndpoint()
    {
        var assembly = Assembly.Load(RestaurantApiAssembly);
        var featureNamespaces = GetFeatureNamespaces(assembly);
        var violations = new List<string>();

        foreach (var featureNamespace in featureNamespaces)
        {
            var hasEndpoint = Types.InAssembly(assembly)
                .That()
                .ResideInNamespace(featureNamespace)
                .And()
                .HaveNameEndingWith("Endpoint")
                .GetTypes()
                .Any();

            if (!hasEndpoint)
            {
                violations.Add(featureNamespace);
            }
        }

        Assert.That(violations, Is.Empty, 
            $"Each feature should have at least one endpoint. " +
            $"Features without endpoints: {string.Join(", ", violations)}");
    }

    [Test]
    public void EachFeature_ShouldHaveAtLeastOneHandler()
    {
        var assembly = Assembly.Load(RestaurantApiAssembly);
        var featureNamespaces = GetFeatureNamespaces(assembly);
        var violations = new List<string>();

        foreach (var featureNamespace in featureNamespaces)
        {
            var hasHandler = Types.InAssembly(assembly)
                .That()
                .ResideInNamespace(featureNamespace)
                .And()
                .HaveNameEndingWith("Handler")
                .GetTypes()
                .Any();

            if (!hasHandler)
            {
                violations.Add(featureNamespace);
            }
        }

        Assert.That(violations, Is.Empty, 
            $"Each feature should have at least one handler. " +
            $"Features without handlers: {string.Join(", ", violations)}");
    }

    [Test]
    public void EachFeature_ShouldHaveRequestOrQueryOrCommand()
    {
        var assembly = Assembly.Load(RestaurantApiAssembly);
        var featureNamespaces = GetFeatureNamespaces(assembly);
        var violations = new List<string>();

        foreach (var featureNamespace in featureNamespaces)
        {
            var hasRequest = Types.InAssembly(assembly)
                .That()
                .ResideInNamespace(featureNamespace)
                .And()
                .ImplementInterface(typeof(Mediator.IRequest<>))
                .GetTypes()
                .Any();

            if (!hasRequest)
            {
                violations.Add(featureNamespace);
            }
        }

        Assert.That(violations, Is.Empty, 
            $"Each feature should have at least one request (Query or Command). " +
            $"Features without requests: {string.Join(", ", violations)}");
    }

    [Test]
    public void Features_ShouldNotHaveEmptyOperations()
    {
        var assembly = Assembly.Load(RestaurantApiAssembly);
        var featureNamespaces = GetFeatureNamespaces(assembly);
        var emptyOperations = new List<string>();

        foreach (var featureNamespace in featureNamespaces)
        {
            var typesInFeature = Types.InAssembly(assembly)
                .That()
                .ResideInNamespace(featureNamespace)
                .GetTypes();

            if (!typesInFeature.Any())
            {
                emptyOperations.Add(featureNamespace);
            }
        }

        Assert.That(emptyOperations, Is.Empty, 
            $"Feature operations should not be empty. " +
            $"Empty operations: {string.Join(", ", emptyOperations)}");
    }

    [Test]
    public void Handlers_ShouldHaveMatchingRequestType()
    {
        var assembly = Assembly.Load(RestaurantApiAssembly);
        var handlerTypes = Types.InAssembly(assembly)
            .That()
            .ResideInNamespace(FeaturesNamespace)
            .And()
            .HaveNameEndingWith("Handler")
            .GetTypes();

        var violations = new List<string>();

        foreach (var handlerType in handlerTypes)
        {
            var handlerInterfaces = handlerType.GetInterfaces()
                .Where(i => i.IsGenericType && 
                           i.GetGenericTypeDefinition().Name.StartsWith("IRequestHandler"))
                .ToList();

            foreach (var handlerInterface in handlerInterfaces)
            {
                var requestType = handlerInterface.GetGenericArguments()[0];
                var expectedRequestName = handlerType.Name.Replace("Handler", "Query")
                    .Replace("Handler", "Command");
                
                // Check if there's a matching request type in the same namespace
                var matchingRequest = Types.InAssembly(assembly)
                    .That()
                    .ResideInNamespace(handlerType.Namespace ?? "")
                    .And()
                    .HaveName(requestType.Name)
                    .GetTypes()
                    .FirstOrDefault();

                if (matchingRequest == null)
                {
                    violations.Add($"{handlerType.Name} should have a matching request type {requestType.Name} in the same namespace");
                }
            }
        }

        Assert.That(violations, Is.Empty, 
            $"Handlers should have matching request types in the same namespace. " +
            $"Violations: {string.Join(", ", violations)}");
    }

    [Test]
    public void Operations_ShouldFollowVerticalSliceStructure()
    {
        var assembly = Assembly.Load(RestaurantApiAssembly);
        var featureNamespaces = GetFeatureNamespaces(assembly);
        var structuralViolations = new List<string>();

        foreach (var featureNamespace in featureNamespaces)
        {
            var typesInFeature = Types.InAssembly(assembly)
                .That()
                .ResideInNamespace(featureNamespace)
                .GetTypes()
                .ToList();

            // Check that all types in the feature are related to the same operation
            var operationName = GetOperationNameFromNamespace(featureNamespace);
            
            var unrelatedTypes = typesInFeature
                .Where(type => !type.Name.StartsWith(operationName) && 
                              !IsAllowedSuffixType(type.Name))
                .ToList();

            foreach (var unrelatedType in unrelatedTypes)
            {
                structuralViolations.Add($"{unrelatedType.Name} in {featureNamespace} doesn't follow vertical slice naming");
            }
        }

        Assert.That(structuralViolations, Is.Empty, 
            $"All types in a feature should be related to the same operation. " +
            $"Violations: {string.Join(", ", structuralViolations)}");
    }

    [Test]
    public void Features_ShouldNotShareTypes()
    {
        var assembly = Assembly.Load(RestaurantApiAssembly);
        var featureNamespaces = GetFeatureNamespaces(assembly);
        var sharedTypeViolations = new List<string>();

        var allFeatureTypes = new Dictionary<string, List<Type>>();

        foreach (var featureNamespace in featureNamespaces)
        {
            var typesInFeature = Types.InAssembly(assembly)
                .That()
                .ResideInNamespace(featureNamespace)
                .GetTypes()
                .ToList();

            allFeatureTypes[featureNamespace] = typesInFeature;
        }

        // Check for duplicate type names across features (which would indicate sharing)
        var typeNameCounts = allFeatureTypes
            .SelectMany(kvp => kvp.Value.Select(type => new { TypeName = type.Name, Namespace = kvp.Key }))
            .GroupBy(x => x.TypeName)
            .Where(group => group.Count() > 1)
            .ToList();

        foreach (var duplicateGroup in typeNameCounts)
        {
            sharedTypeViolations.Add($"Type '{duplicateGroup.Key}' appears in multiple features: {string.Join(", ", duplicateGroup.Select(x => x.Namespace))}");
        }

        Assert.That(sharedTypeViolations, Is.Empty, 
            $"Features should not share type names (indicates potential coupling). " +
            $"Violations: {string.Join("; ", sharedTypeViolations)}");
    }

    private static List<string> GetFeatureNamespaces(Assembly assembly)
    {
        return Types.InAssembly(assembly)
            .That()
            .ResideInNamespace(FeaturesNamespace)
            .GetTypes()
            .Select(type => type.Namespace)
            .Where(ns => ns != null)
            .Cast<string>()
            .Where(ns => ns.Split('.').Length >= 5) // At least Features.FeatureName.OperationName
            .Distinct()
            .ToList();
    }

    private static string GetOperationNameFromNamespace(string featureNamespace)
    {
        var parts = featureNamespace.Split('.');
        return parts.Length >= 5 ? parts[4] : string.Empty; // e.g., "GetMenuItems" from "RestaurantManagement.Api.Features.MenuItems.GetMenuItems"
    }

    private static bool IsAllowedSuffixType(string typeName)
    {
        var allowedSuffixes = new[] { "Dto", "Response", "Validator", "Endpoint", "Handler", "Query", "Command", "Request" };
        return allowedSuffixes.Any(typeName.EndsWith);
    }
}