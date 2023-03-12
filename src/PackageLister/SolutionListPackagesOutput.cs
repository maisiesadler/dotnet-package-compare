using System.Diagnostics.CodeAnalysis;

namespace PackageLister;

public record ProjectAndFramework(string ProjectName, string FrameworkName);

public class SolutionListPackagesOutput
{
    private readonly Dictionary<ProjectAndFramework, Dictionary<string, Package>> _packagesByProjectAndFramwork = new();

    public SolutionListPackagesOutput(params (string projectName, string frameworkName, Package package)[] initial)
    {
        foreach (var project in initial)
        {
            Add(project.projectName, project.frameworkName, project.package);
        }
    }

    public void Add(string projectName, string frameworkName)
    {
        var projectAndFramework = new ProjectAndFramework(projectName, frameworkName);
        if (!_packagesByProjectAndFramwork.ContainsKey(projectAndFramework))
            _packagesByProjectAndFramwork.Add(projectAndFramework, new Dictionary<string, Package>());
    }

    public void Add(string projectName, string frameworkName, Package package)
    {
        var projectAndFramework = new ProjectAndFramework(projectName, frameworkName);
        if (!_packagesByProjectAndFramwork.ContainsKey(projectAndFramework))
            _packagesByProjectAndFramwork.Add(projectAndFramework, new Dictionary<string, Package>());

        _packagesByProjectAndFramwork[projectAndFramework].Add(package.Name, package);
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

    public IEnumerable<(ProjectAndFramework projectAndFramework, List<Package> packages)> GetPackagesByProjectAndFramework()
    {
        foreach (var (projectAndFramework, packageKv) in _packagesByProjectAndFramwork)
        {
            yield return (projectAndFramework, packageKv.Values.ToList());
        }
    }
}
