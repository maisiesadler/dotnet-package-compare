using System.Diagnostics.CodeAnalysis;

namespace PackageLister;

internal record ProjectAndFramework(string ProjectName, string FrameworkName);
internal record PackagesByProjectAndFramework(ProjectAndFramework ProjectAndFramework, List<Package> Packages);

internal class SolutionListPackagesOutput
{
    private readonly Dictionary<ProjectAndFramework, Dictionary<string, Package>> _packagesByProjectAndFramwork = new();

    public SolutionListPackagesOutput(params (string projectName, string frameworkName, Package package)[] initial)
    {
        foreach (var project in initial)
        {
            var addPackage = GetAddPackageFunction(project.projectName, project.frameworkName);
            addPackage(project.package);
        }
    }

    public Action<Package> GetAddPackageFunction(string projectName, string frameworkName)
    {
        var projectAndFramework = new ProjectAndFramework(projectName, frameworkName);
        if (!_packagesByProjectAndFramwork.ContainsKey(projectAndFramework))
            _packagesByProjectAndFramwork.Add(projectAndFramework, new Dictionary<string, Package>());

        return (package) => { _packagesByProjectAndFramwork[projectAndFramework].Add(package.Name, package); };
    }

    public bool TryGetPackage(ProjectAndFramework projectAndFramework, string packageName, [NotNullWhen(true)] out Package? package)
    {
        if (_packagesByProjectAndFramwork.TryGetValue(projectAndFramework, out var packages)
            && packages.TryGetValue(packageName, out package))
        {
            return true;
        }

        package = null;
        return false;
    }

    public IEnumerable<PackagesByProjectAndFramework> GetPackagesByProjectAndFramework()
    {
        foreach (var (projectAndFramework, packageKv) in _packagesByProjectAndFramwork)
        {
            yield return new(projectAndFramework, packageKv.Values.ToList());
        }
    }
}
