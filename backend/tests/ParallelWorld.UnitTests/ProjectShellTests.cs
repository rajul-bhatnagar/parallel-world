namespace ParallelWorld.UnitTests;

public sealed class ProjectShellTests
{
    [Theory]
    [InlineData(typeof(Domain.AssemblyMarker), "ParallelWorld.Domain")]
    [InlineData(typeof(Application.AssemblyMarker), "ParallelWorld.Application")]
    [InlineData(typeof(Simulation.AssemblyMarker), "ParallelWorld.Simulation")]
    public void AssemblyMarker_ProjectShell_UsesExpectedAssemblyName(Type markerType, string expectedName)
    {
        Assert.Equal(expectedName, markerType.Assembly.GetName().Name);
    }
}
