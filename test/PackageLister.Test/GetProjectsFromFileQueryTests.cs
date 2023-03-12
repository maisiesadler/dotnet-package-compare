namespace PackageLister.Test;

public class GetProjectsFromFileQueryTests
{
    [Fact]
    public async Task OneProject_OneFramework_NoPackages()
    {
        // Arrange
        var query = new GetProjectsFromFileQuery("./test_data/empty_packages.txt");

        // Act
        var solutionPackagesOutput = await query.Execute();

        // Assert
        var allPackagesByProjectAndFramework = solutionPackagesOutput.GetPackagesByProjectAndFramework().ToList();
        var (projectAndFramework, packages) = Assert.Single(allPackagesByProjectAndFramework);
        Assert.Equal("PackageLister", projectAndFramework.ProjectName);
        Assert.Equal("net7.0", projectAndFramework.FrameworkName);
        Assert.Empty(packages);
    }
}
