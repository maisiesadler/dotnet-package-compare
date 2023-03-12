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
}
