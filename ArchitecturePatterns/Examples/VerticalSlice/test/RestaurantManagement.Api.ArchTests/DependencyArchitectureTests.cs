namespace RestaurantManagement.Api.ArchTests;

using NetArchTest.Rules;
using NUnit.Framework;

[TestFixture]
public class DependencyArchitectureTests
{
    private const string RestaurantApiAssembly = "RestaurantManagement.Api";
    private const string FeaturesNamespace = "RestaurantManagement.Api.Features";

    [Test]
    public void Features_ShouldNotDependOnEachOther()
    {
        // Get all feature namespaces
        var featureTypes = Types.InAssembly(System.Reflection.Assembly.Load(RestaurantApiAssembly))
            .That()
            .ResideInNamespace(FeaturesNamespace)
            .GetTypes();

        var featureNamespaces = featureTypes
            .Select(t => t.Namespace)
            .Where(ns => ns != null)
            .Select(ns => ns!)
            .Where(ns => ns.StartsWith($"{FeaturesNamespace}."))
            .Select(ns => ns.Split('.')[3]) // Get feature name (e.g., "MenuItems", "Orders", "Tables")
            .Distinct()
            .ToList();

        foreach (var featureNamespace in featureNamespaces)
        {
            var otherFeatures = featureNamespaces.Where(f => f != featureNamespace).ToArray();
            
            if (otherFeatures.Length > 0)
            {
                var result = Types.InAssembly(System.Reflection.Assembly.Load(RestaurantApiAssembly))
                    .That()
                    .ResideInNamespace($"{FeaturesNamespace}.{featureNamespace}")
                    .ShouldNot()
                    .HaveDependencyOnAll(otherFeatures.Select(f => $"{FeaturesNamespace}.{f}").ToArray())
                    .GetResult();

                Assert.That(result.IsSuccessful, 
                    $"Feature '{featureNamespace}' should not depend on other features. " +
                    $"Violations: {string.Join(", ", result.FailingTypeNames ?? [])}");
            }
        }
    }

    [Test]
    public void Common_ShouldNotDependOnFeatures()
    {
        var result = Types.InAssembly(System.Reflection.Assembly.Load(RestaurantApiAssembly))
            .That()
            .ResideInNamespace("RestaurantManagement.Api.Common")
            .ShouldNot()
            .HaveDependencyOn(FeaturesNamespace)
            .GetResult();

        Assert.That(result.IsSuccessful, 
            $"Common namespace should not depend on Features. " +
            $"Violations: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }

    [Test]
    public void Data_ShouldNotDependOnFeatures()
    {
        var result = Types.InAssembly(System.Reflection.Assembly.Load(RestaurantApiAssembly))
            .That()
            .ResideInNamespace("RestaurantManagement.Api.Data")
            .ShouldNot()
            .HaveDependencyOn(FeaturesNamespace)
            .GetResult();

        Assert.That(result.IsSuccessful, 
            $"Data namespace should not depend on Features. " +
            $"Violations: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }

    [Test]
    public void Entities_ShouldNotDependOnFeatures()
    {
        var result = Types.InAssembly(System.Reflection.Assembly.Load(RestaurantApiAssembly))
            .That()
            .ResideInNamespace("RestaurantManagement.Api.Entities")
            .ShouldNot()
            .HaveDependencyOn(FeaturesNamespace)
            .GetResult();

        Assert.That(result.IsSuccessful, 
            $"Entities namespace should not depend on Features. " +
            $"Violations: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }

    [Test]
    public void Entities_ShouldNotDependOnDataOrCommon()
    {
        var result = Types.InAssembly(System.Reflection.Assembly.Load(RestaurantApiAssembly))
            .That()
            .ResideInNamespace("RestaurantManagement.Api.Entities")
            .ShouldNot()
            .HaveDependencyOnAny("RestaurantManagement.Api.Data", "RestaurantManagement.Api.Common")
            .GetResult();

        Assert.That(result.IsSuccessful, 
            $"Entities should not depend on Data or Common namespaces. " +
            $"Violations: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }
}