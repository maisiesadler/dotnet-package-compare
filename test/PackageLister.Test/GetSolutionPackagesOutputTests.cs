namespace PackageLister.Test;

public class GetSolutionPackagesOutputTests
{
    [Fact]
    public void UnchangedPackage_NotReturned()
    {
        // Arrange
        var listPackagesOutput = @"
Project 'PackageLister' has the following package references
   [net7.0]: 
   Top-level Package                               Requested   Resolved
   > Microsoft.Extensions.DependencyInjection      7.0.0       7.0.0   

   Transitive Package                                           Resolved
   > Microsoft.Extensions.DependencyInjection.Abstractions      7.0.0   
".Split(Environment.NewLine);

        // Act
        var solutionPackagesOutput = new GetSolutionPackagesOutput().Read(listPackagesOutput);

        // Assert
        var allPackagesByProjectAndFramework = solutionPackagesOutput.GetPackagesByProjectAndFramework().ToList();
        var (projectAndFramework, packages) = Assert.Single(allPackagesByProjectAndFramework);
        Assert.Equal("PackageLister", projectAndFramework.ProjectName);
        Assert.Equal("net7.0", projectAndFramework.FrameworkName);
        Assert.Equal(2, packages.Count);
        Assert.Equal("Microsoft.Extensions.DependencyInjection", packages[0].Name);
        Assert.Equal("7.0.0", packages[0].Version);
        Assert.True(packages[0].DirectReference);
        Assert.Equal("Microsoft.Extensions.DependencyInjection.Abstractions", packages[1].Name);
        Assert.Equal("7.0.0", packages[1].Version);
        Assert.False(packages[1].DirectReference);
    }
}
