using System.Collections.Immutable;

namespace PackageLister.Test;

public class PackageComparisonTests
{
    [Fact]
    public void UnchangedPackage_NotReturned()
    {
        // Arrange
        var projectsBefore = new ProjectPackagesOutput(
            ("Project1", "net7.0", new Package("Package.One", "1.2.3", true))
        );

        var projectsAfter = new ProjectPackagesOutput(
            ("Project1", "net7.0", new Package("Package.One", "1.2.3", true))
        );

        // Act
        var changes = new PackageComparison().Compare(projectsBefore, projectsAfter);

        // Assert
        var project = Assert.Single(changes);
        var frameworks = Assert.Single(project.Frameworks);
        Assert.Empty(frameworks.PackageChanges.Added);
        Assert.Empty(frameworks.PackageChanges.Removed);
    }

    [Fact]
    public void AddedPackage_ReturnedAsAdded()
    {
        // Arrange
        var projectsBefore = new ProjectPackagesOutput(
            ("Project1", "net7.0", new Package("Package.One", "1.2.3", true))
        );

        var projectsAfter = new ProjectPackagesOutput(
            ("Project1", "net7.0", new Package("Package.One", "1.2.3", true)),
            ("Project1", "net7.0", new Package("Package.Two", "1.2.3", true))
        );

        // Act
        var changes = new PackageComparison().Compare(projectsBefore, projectsAfter);

        // Assert
        var project = Assert.Single(changes);
        var frameworks = Assert.Single(project.Frameworks);
        Assert.Empty(frameworks.PackageChanges.Changed);
        Assert.Empty(frameworks.PackageChanges.Removed);
        var packagesAdded = Assert.Single(frameworks.PackageChanges.Added);

        Assert.Equal("Package.Two", packagesAdded.Name);
        Assert.Equal("1.2.3", packagesAdded.Version);
        Assert.True(packagesAdded.DirectReference);
    }

    [Fact]
    public void RemovedPackage_ReturnedAsRemoved()
    {
        // Arrange
        var projectsBefore = new ProjectPackagesOutput(
            ("Project1", "net7.0", new Package("Package.One", "1.2.3", true)),
            ("Project1", "net7.0", new Package("Package.Two", "1.2.3", true))
        );

        var projectsAfter = new ProjectPackagesOutput(
            ("Project1", "net7.0", new Package("Package.One", "1.2.3", true))
        );

        // Act
        var changes = new PackageComparison().Compare(projectsBefore, projectsAfter);

        // Assert
        var project = Assert.Single(changes);
        var frameworks = Assert.Single(project.Frameworks);
        Assert.Empty(frameworks.PackageChanges.Added);
        Assert.Empty(frameworks.PackageChanges.Changed);
        var packagesAdded = Assert.Single(frameworks.PackageChanges.Removed);

        Assert.Equal("Package.Two", packagesAdded.Name);
        Assert.Equal("1.2.3", packagesAdded.Version);
        Assert.True(packagesAdded.DirectReference);
    }

    [Fact]
    public void VersionChange_ShownAsChange()
    {
        // Arrange
        var projectsBefore = new ProjectPackagesOutput(
            ("Project1", "net7.0", new Package("Package.One", "1.2.3", true))
        );

        var projectsAfter = new ProjectPackagesOutput(
            ("Project1", "net7.0", new Package("Package.One", "2.0.0", true))
        );

        // Act
        var changes = new PackageComparison().Compare(projectsBefore, projectsAfter);

        // Assert
        var project = Assert.Single(changes);
        var frameworks = Assert.Single(project.Frameworks);
        Assert.Empty(frameworks.PackageChanges.Added);
        Assert.Empty(frameworks.PackageChanges.Removed);
        var packagesChanged = Assert.Single(frameworks.PackageChanges.Changed);

        Assert.Equal("Package.One", packagesChanged.Name);
        Assert.Equal("1.2.3", packagesChanged.VersionBefore);
        Assert.Equal("2.0.0", packagesChanged.VersionAfter);
        Assert.True(packagesChanged.DirectReferenceBefore);
        Assert.True(packagesChanged.DirectReferenceAfter);
    }

    [Fact]
    public void DirectReferenceChange_ShownAsChange()
    {
        // Arrange
        var projectsBefore = new ProjectPackagesOutput(
            ("Project1", "net7.0", new Package("Package.One", "1.2.3", true))
        );

        var projectsAfter = new ProjectPackagesOutput(
            ("Project1", "net7.0", new Package("Package.One", "1.2.3", false))
        );

        // Act
        var changes = new PackageComparison().Compare(projectsBefore, projectsAfter);

        // Assert
        var project = Assert.Single(changes);
        var frameworks = Assert.Single(project.Frameworks);
        Assert.Empty(frameworks.PackageChanges.Added);
        Assert.Empty(frameworks.PackageChanges.Removed);
        var packagesChanged = Assert.Single(frameworks.PackageChanges.Changed);

        Assert.Equal("Package.One", packagesChanged.Name);
        Assert.Equal("1.2.3", packagesChanged.VersionBefore);
        Assert.Equal("1.2.3", packagesChanged.VersionAfter);
        Assert.True(packagesChanged.DirectReferenceBefore);
        Assert.False(packagesChanged.DirectReferenceAfter);
    }
}
