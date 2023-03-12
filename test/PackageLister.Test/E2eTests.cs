using Microsoft.Extensions.DependencyInjection;

namespace PackageLister.Test;

public class E2eTests
{
    private readonly CompareSolutionsInteractor _interactor;

    public E2eTests()
    {
        var services = new ServiceCollection();
        services.AddPackageComparison("./test_data/empty_packages.txt", "./test_data/one_package.txt");

        var sp = services.BuildServiceProvider();

        _interactor = sp.GetRequiredService<CompareSolutionsInteractor>();
    }

    [Fact]
    public async Task OnePackageAdded()
    {
        // Act
        var changes = await _interactor.Execute();

        // Assert
        var project = Assert.Single(changes);
        Assert.Equal("PackageLister", project.Name);

        var frameworks = Assert.Single(project.Frameworks);
        Assert.Equal("net7.0", frameworks.Name);

        Assert.Empty(frameworks.PackageChanges.Changed);
        Assert.Empty(frameworks.PackageChanges.Removed);

        var addedPackage = Assert.Single(frameworks.PackageChanges.Added);
        Assert.Equal("Microsoft.Extensions.DependencyInjection", addedPackage.Name);
        Assert.Equal("7.0.0", addedPackage.Version);
        Assert.True(addedPackage.DirectReference);
    }
}
