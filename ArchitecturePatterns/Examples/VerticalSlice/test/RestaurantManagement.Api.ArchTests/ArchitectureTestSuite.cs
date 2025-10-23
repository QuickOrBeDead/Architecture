namespace RestaurantManagement.Api.ArchTests;

using NUnit.Framework;

/// <summary>
/// Architecture tests for the Restaurant Management API using Vertical Slice Architecture.
/// These tests enforce architectural rules and patterns to maintain code quality and consistency.
/// </summary>
[TestFixture]
public class ArchitectureTestSuite
{
    [Test]
    public void RunAllArchitectureTests()
    {
        // This is a meta-test that provides a summary of all architectural rules
        var testResults = new List<(string TestName, bool Passed, string[] Details)>();

        // Run dependency tests
        var dependencyTests = new DependencyArchitectureTests();
        RunTest(testResults, "Features should not depend on each other", 
            () => dependencyTests.Features_ShouldNotDependOnEachOther());
        RunTest(testResults, "Common should not depend on Features", 
            () => dependencyTests.Common_ShouldNotDependOnFeatures());
        RunTest(testResults, "Data should not depend on Features", 
            () => dependencyTests.Data_ShouldNotDependOnFeatures());
        RunTest(testResults, "Entities should not depend on Features", 
            () => dependencyTests.Entities_ShouldNotDependOnFeatures());
        RunTest(testResults, "Entities should be isolated", 
            () => dependencyTests.Entities_ShouldNotDependOnDataOrCommon());

        // Run design pattern tests
        var designTests = new DesignPatternTests();
        RunTest(testResults, "Handlers should implement IRequestHandler", 
            () => designTests.Handlers_ShouldImplementIRequestHandler());
        RunTest(testResults, "Queries should implement IRequest", 
            () => designTests.Queries_ShouldImplementIRequest());
        RunTest(testResults, "Commands should implement IRequest", 
            () => designTests.Commands_ShouldImplementIRequest());
        RunTest(testResults, "Validators should inherit from AbstractValidator", 
            () => designTests.Validators_ShouldInheritFromAbstractValidator());
        RunTest(testResults, "Endpoints should be static classes", 
            () => designTests.Endpoints_ShouldBeStaticClasses());

        // Run structural tests
        var structuralTests = new StructuralTests();
        RunTest(testResults, "Each feature should have at least one endpoint", 
            () => structuralTests.EachFeature_ShouldHaveAtLeastOneEndpoint());
        RunTest(testResults, "Each feature should have at least one handler", 
            () => structuralTests.EachFeature_ShouldHaveAtLeastOneHandler());
        RunTest(testResults, "Each feature should have requests", 
            () => structuralTests.EachFeature_ShouldHaveRequestOrQueryOrCommand());
        RunTest(testResults, "Features should not be empty", 
            () => structuralTests.Features_ShouldNotHaveEmptyOperations());
        RunTest(testResults, "Handlers should have matching request types", 
            () => structuralTests.Handlers_ShouldHaveMatchingRequestType());

        // Print summary
        var passedTests = testResults.Where(r => r.Passed).ToList();
        var failedTests = testResults.Where(r => !r.Passed).ToList();

        TestContext.WriteLine($"\n=== ARCHITECTURE TEST SUMMARY ===");
        TestContext.WriteLine($"Total Tests: {testResults.Count}");
        TestContext.WriteLine($"Passed: {passedTests.Count}");
        TestContext.WriteLine($"Failed: {failedTests.Count}");

        if (failedTests.Any())
        {
            TestContext.WriteLine($"\n=== FAILED TESTS ===");
            foreach (var (testName, _, details) in failedTests)
            {
                TestContext.WriteLine($"❌ {testName}");
                foreach (var detail in details)
                {
                    TestContext.WriteLine($"   - {detail}");
                }
            }
        }

        TestContext.WriteLine($"\n=== PASSED TESTS ===");
        foreach (var (testName, _, _) in passedTests)
        {
            TestContext.WriteLine($"✅ {testName}");
        }

        // The test passes if at least 80% of architectural rules are satisfied
        var passRate = (double)passedTests.Count / testResults.Count;
        Assert.That(passRate, Is.GreaterThanOrEqualTo(0.8), 
            $"Architecture compliance rate is {passRate:P}. Expected at least 80% compliance.");
    }

    private static void RunTest(List<(string TestName, bool Passed, string[] Details)> results, 
                              string testName, System.Action testAction)
    {
        try
        {
            testAction();
            results.Add((testName, true, Array.Empty<string>()));
        }
        catch (AssertionException ex)
        {
            var details = ex.Message?.Split('\n')
                            .Where(line => !string.IsNullOrWhiteSpace(line))
                            .Take(3)
                            .ToArray() ?? Array.Empty<string>();
            results.Add((testName, false, details));
        }
        catch (Exception ex)
        {
            results.Add((testName, false, new[] { ex.Message }));
        }
    }
}