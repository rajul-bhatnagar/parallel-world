using System.Xml.Linq;

namespace ParallelWorld.ArchitectureTests;

public sealed class ProjectReferenceTests
{
    private static readonly IReadOnlyDictionary<string, string[]> ExpectedReferences =
        new Dictionary<string, string[]>(StringComparer.Ordinal)
        {
            ["ParallelWorld.Domain"] = [],
            ["ParallelWorld.Application"] = ["ParallelWorld.Domain"],
            ["ParallelWorld.Infrastructure"] = ["ParallelWorld.Application", "ParallelWorld.Domain"],
            ["ParallelWorld.Simulation"] = ["ParallelWorld.Application", "ParallelWorld.Domain"],
            ["ParallelWorld.AI"] = ["ParallelWorld.Application"],
            ["ParallelWorld.Api"] =
            [
                "ParallelWorld.AI",
                "ParallelWorld.Application",
                "ParallelWorld.Infrastructure",
                "ParallelWorld.Simulation",
            ],
        };

    [Fact]
    public void ProductionProjects_References_MatchApprovedDependencyDirection()
    {
        var backendDirectory = FindBackendDirectory();

        foreach (var (projectName, expectedReferences) in ExpectedReferences)
        {
            var projectPath = Path.Combine(backendDirectory.FullName, "src", projectName, $"{projectName}.csproj");
            var document = XDocument.Load(projectPath);
            var actualReferences = document
                .Descendants("ProjectReference")
                .Select(element => Path.GetFileNameWithoutExtension(element.Attribute("Include")!.Value))
                .Order(StringComparer.Ordinal)
                .ToArray();

            Assert.Equal(expectedReferences.Order(StringComparer.Ordinal), actualReferences);
        }
    }

    [Fact]
    public void ApplicationUseCases_DoNotDependOnInfrastructureOrPersistenceFrameworks()
    {
        var applicationDirectory = Path.Combine(
            FindBackendDirectory().FullName,
            "src",
            "ParallelWorld.Application");
        var source = string.Join(
            Environment.NewLine,
            Directory.EnumerateFiles(applicationDirectory, "*.cs", SearchOption.AllDirectories)
                .Select(File.ReadAllText));

        Assert.DoesNotContain("ParallelWorld.Infrastructure", source, StringComparison.Ordinal);
        Assert.DoesNotContain("Microsoft.EntityFrameworkCore", source, StringComparison.Ordinal);
        Assert.DoesNotContain("Npgsql", source, StringComparison.Ordinal);
    }

    [Fact]
    public void UseCaseServices_AreOwnedByApplicationLayer()
    {
        var backendDirectory = FindBackendDirectory();
        var infrastructureDirectory = Path.Combine(
            backendDirectory.FullName,
            "src",
            "ParallelWorld.Infrastructure");
        var applicationDirectory = Path.Combine(
            backendDirectory.FullName,
            "src",
            "ParallelWorld.Application");

        Assert.False(File.Exists(Path.Combine(
            infrastructureDirectory,
            "Authentication",
            "AuthenticationService.cs")));
        Assert.False(File.Exists(Path.Combine(infrastructureDirectory, "Worlds", "WorldService.cs")));
        Assert.True(File.Exists(Path.Combine(
            applicationDirectory,
            "Authentication",
            "AuthenticationService.cs")));
        Assert.True(File.Exists(Path.Combine(applicationDirectory, "Worlds", "WorldService.cs")));
    }

    private static DirectoryInfo FindBackendDirectory()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);

        while (current is not null && !File.Exists(Path.Combine(current.FullName, "ParallelWorld.sln")))
        {
            current = current.Parent;
        }

        return current ?? throw new DirectoryNotFoundException("Could not locate the backend solution directory.");
    }
}
