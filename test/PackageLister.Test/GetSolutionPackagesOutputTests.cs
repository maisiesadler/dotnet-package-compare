namespace PackageLister.Test;

public class GetSolutionPackagesOutputTests
{
    [Fact]
    public void EmptyFile()
    {
        // Arrange
        var listPackagesOutput = @"
".Split(Environment.NewLine);

        // Act
        var solutionPackagesOutput = GetSolutionPackagesOutput.Read(listPackagesOutput);

        // Assert
        var allPackagesByProjectAndFramework = solutionPackagesOutput.GetPackagesByProjectAndFramework().ToList();
        Assert.Empty(allPackagesByProjectAndFramework);
    }

    [Fact]
    public void OneProjectAndOneFrameworkNoPackages()
    {
        // Arrange
        var listPackagesOutput = @"
Project 'PackageLister' has the following package references
   [net7.0]: No packages were found for this framework.
".Split(Environment.NewLine);

        // Act
        var solutionPackagesOutput = GetSolutionPackagesOutput.Read(listPackagesOutput);

        // Assert
        var allPackagesByProjectAndFramework = solutionPackagesOutput.GetPackagesByProjectAndFramework().ToList();
        var (projectAndFramework, packages) = Assert.Single(allPackagesByProjectAndFramework);
        Assert.Equal("PackageLister", projectAndFramework.ProjectName);
        Assert.Equal("net7.0", projectAndFramework.FrameworkName);
        Assert.Empty(packages);
    }

    [Fact]
    public void OneProjectAndOneFramework()
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
        var solutionPackagesOutput = GetSolutionPackagesOutput.Read(listPackagesOutput);

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

    [Fact]
    public void TwoProjectsAndOneFramework()
    {
        // Arrange
        var listPackagesOutput = @"
Project 'PackageLister' has the following package references
   [net7.0]: 
   Top-level Package                               Requested   Resolved
   > Microsoft.Extensions.DependencyInjection      7.0.0       7.0.0   

Project 'PackageLister.Test' has the following package references
   [net7.0]: 
   Top-level Package                Requested   Resolved
   > coverlet.collector             3.1.2       3.1.2   
".Split(Environment.NewLine);

        // Act
        var solutionPackagesOutput = GetSolutionPackagesOutput.Read(listPackagesOutput);

        // Assert
        var allPackagesByProjectAndFramework = solutionPackagesOutput.GetPackagesByProjectAndFramework().ToList();
        Assert.Equal(2, allPackagesByProjectAndFramework.Count);

        var first = allPackagesByProjectAndFramework[0];
        Assert.Equal("PackageLister", first.ProjectAndFramework.ProjectName);
        Assert.Equal("net7.0", first.ProjectAndFramework.FrameworkName);
        var firstPackage = Assert.Single(first.Packages);
        Assert.Equal("Microsoft.Extensions.DependencyInjection", firstPackage.Name);
        Assert.Equal("7.0.0", firstPackage.Version);
        Assert.True(firstPackage.DirectReference);

        var second = allPackagesByProjectAndFramework[1];
        Assert.Equal("PackageLister.Test", second.ProjectAndFramework.ProjectName);
        Assert.Equal("net7.0", second.ProjectAndFramework.FrameworkName);
        var secondPackage = Assert.Single(second.Packages);
        Assert.Equal("coverlet.collector", secondPackage.Name);
        Assert.Equal("3.1.2", secondPackage.Version);
        Assert.True(secondPackage.DirectReference);
    }

    [Fact]
    public void OneProjectAndTwoFrameworks()
    {
        // Arrange
        var listPackagesOutput = @"
Project 'PackageLister' has the following package references
   [net6.0]: 
   Top-level Package                               Requested   Resolved

   Transitive Package                                           Resolved
   > Microsoft.Extensions.DependencyInjection.Abstractions      7.0.0   

   [net7.0]: 
   Top-level Package                               Requested   Resolved

   Transitive Package                                           Resolved
   > Microsoft.Extensions.DependencyInjection.Abstractions      7.0.0  
".Split(Environment.NewLine);

        // Act
        var solutionPackagesOutput = GetSolutionPackagesOutput.Read(listPackagesOutput);

        // Assert
        var allPackagesByProjectAndFramework = solutionPackagesOutput.GetPackagesByProjectAndFramework().ToList();
        Assert.Equal(2, allPackagesByProjectAndFramework.Count);

        var first = allPackagesByProjectAndFramework[0];
        Assert.Equal("PackageLister", first.ProjectAndFramework.ProjectName);
        Assert.Equal("net6.0", first.ProjectAndFramework.FrameworkName);
        var firstPackage = Assert.Single(first.Packages);
        Assert.Equal("Microsoft.Extensions.DependencyInjection.Abstractions", firstPackage.Name);
        Assert.Equal("7.0.0", firstPackage.Version);
        Assert.False(firstPackage.DirectReference);

        var second = allPackagesByProjectAndFramework[1];
        Assert.Equal("PackageLister", second.ProjectAndFramework.ProjectName);
        Assert.Equal("net7.0", second.ProjectAndFramework.FrameworkName);
        var secondPackage = Assert.Single(second.Packages);
        Assert.Equal("Microsoft.Extensions.DependencyInjection.Abstractions", secondPackage.Name);
        Assert.Equal("7.0.0", secondPackage.Version);
        Assert.False(secondPackage.DirectReference);
    }

    [Fact]
    public void ResolvedFrameworkChosen()
    {
        // Arrange
        var listPackagesOutput = @"
Project 'PackageLister' has the following package references
   [net7.0]: 
   Top-level Package                               Requested   Resolved
   > Microsoft.Extensions.DependencyInjection      5.0.0       7.0.0   
 
".Split(Environment.NewLine);

        // Act
        var solutionPackagesOutput = GetSolutionPackagesOutput.Read(listPackagesOutput);

        // Assert
        var allPackagesByProjectAndFramework = solutionPackagesOutput.GetPackagesByProjectAndFramework().ToList();
        var project = Assert.Single(allPackagesByProjectAndFramework);

        Assert.Equal("PackageLister", project.ProjectAndFramework.ProjectName);
        Assert.Equal("net7.0", project.ProjectAndFramework.FrameworkName);
        var firstPackage = Assert.Single(project.Packages);
        Assert.Equal("Microsoft.Extensions.DependencyInjection", firstPackage.Name);
        Assert.Equal("7.0.0", firstPackage.Version);
        Assert.True(firstPackage.DirectReference);
    }
}
