namespace ParallelWorld.IntegrationTests;

public sealed class ProjectShellTests
{
    [Fact]
    public void ApiAssembly_ProjectShell_IsLoadable()
    {
        Assert.Equal("ParallelWorld.Api", typeof(Api.AssemblyMarker).Assembly.GetName().Name);
    }
}
