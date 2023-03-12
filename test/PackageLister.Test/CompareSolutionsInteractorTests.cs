using Moq;

namespace PackageLister.Test;

public class CompareSolutionsInteractorTests
{
    private readonly Mock<IGetProjectsBeforeQuery> _getProjectsBeforeQuery;
    private Mock<IGetProjectsAfterQuery> _getProjectsAfterQuery;
    private readonly CompareSolutionsInteractor _interactor;

    public CompareSolutionsInteractorTests()
    {
        _getProjectsBeforeQuery = new Mock<IGetProjectsBeforeQuery>();
        _getProjectsAfterQuery = new Mock<IGetProjectsAfterQuery>();

        _interactor = new CompareSolutionsInteractor(_getProjectsBeforeQuery.Object, _getProjectsAfterQuery.Object);
    }

    [Fact]
    public async Task UnchangedPackage_NotReturned()
    {
        // Arrange
        _getProjectsBeforeQuery.Setup(q => q.Execute())
            .ReturnsAsync(new SolutionListPackagesOutput(
            ("Project1", "net7.0", new Package("Package.One", "1.2.3", true))
        ));

        _getProjectsAfterQuery.Setup(q => q.Execute())
            .ReturnsAsync(new SolutionListPackagesOutput(
            ("Project1", "net7.0", new Package("Package.One", "1.2.3", true))
        ));

        // Act
        var changes = await _interactor.Execute();

        // Assert
        var project = Assert.Single(changes);
        var frameworks = Assert.Single(project.Frameworks);
        Assert.Empty(frameworks.PackageChanges.Added);
        Assert.Empty(frameworks.PackageChanges.Removed);
    }

    [Fact]
    public async Task AddedPackage_ReturnedAsAdded()
    {
        // Arrange
        _getProjectsBeforeQuery.Setup(q => q.Execute())
            .ReturnsAsync(new SolutionListPackagesOutput(
            ("Project1", "net7.0", new Package("Package.One", "1.2.3", true))
        ));

        _getProjectsAfterQuery.Setup(q => q.Execute())
            .ReturnsAsync(new SolutionListPackagesOutput(
            ("Project1", "net7.0", new Package("Package.One", "1.2.3", true)),
            ("Project1", "net7.0", new Package("Package.Two", "1.2.3", true))
        ));

        // Act
        var changes = await _interactor.Execute();

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
    public async Task RemovedPackage_ReturnedAsRemoved()
    {
        // Arrange
        _getProjectsBeforeQuery.Setup(q => q.Execute())
            .ReturnsAsync(new SolutionListPackagesOutput(
            ("Project1", "net7.0", new Package("Package.One", "1.2.3", true)),
            ("Project1", "net7.0", new Package("Package.Two", "1.2.3", true))
        ));

        _getProjectsAfterQuery.Setup(q => q.Execute())
            .ReturnsAsync(new SolutionListPackagesOutput(
            ("Project1", "net7.0", new Package("Package.One", "1.2.3", true))
        ));

        // Act
        var changes = await _interactor.Execute();

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
    public async Task VersionChange_ShownAsChange()
    {
        // Arrange
        _getProjectsBeforeQuery.Setup(q => q.Execute())
            .ReturnsAsync(new SolutionListPackagesOutput(
            ("Project1", "net7.0", new Package("Package.One", "1.2.3", true))
        ));

        _getProjectsAfterQuery.Setup(q => q.Execute())
            .ReturnsAsync(new SolutionListPackagesOutput(
            ("Project1", "net7.0", new Package("Package.One", "2.0.0", true))
        ));

        // Act
        var changes = await _interactor.Execute();

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
    public async Task DirectReferenceChange_ShownAsChange()
    {
        // Arrange
        _getProjectsBeforeQuery.Setup(q => q.Execute())
            .ReturnsAsync(new SolutionListPackagesOutput(
            ("Project1", "net7.0", new Package("Package.One", "1.2.3", true))
        ));

        _getProjectsAfterQuery.Setup(q => q.Execute())
            .ReturnsAsync(new SolutionListPackagesOutput(
            ("Project1", "net7.0", new Package("Package.One", "1.2.3", false))
        ));

        // Act
        var changes = await _interactor.Execute();

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
