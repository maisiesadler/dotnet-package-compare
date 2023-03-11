using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace PackageLister;

public class ProjectPackagesOutput
{
    private readonly Dictionary<ProjectAndFramework, Dictionary<string, Package>> _packagesByProjectAndFramwork = new();

    public ProjectPackagesOutput(params (string projectName, string frameworkName, Package package)[] initial)
    {
        foreach (var project in initial)
        {
            Add(project.projectName, project.frameworkName, project.package);
        }
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

    public IEnumerable<(ProjectAndFramework projectAndFramework, List<Package> package)> GetPackagesByProjectAndFramework()
    {
        foreach (var (projectAndFramework, packageKv) in _packagesByProjectAndFramwork)
        {
            yield return (projectAndFramework, packageKv.Values.ToList());
        }
    }
}
public record ProjectAndFramework(string ProjectName, string FrameworkName);

public record Package(string Name, string Version, bool DirectReference);

public record Project(string Name, IList<ProjectFramework> Frameworks);
public record ProjectFramework(string Name, PackageChanges PackageChanges);
public record PackageChanges(IList<Package> Added, IList<Package> Removed, IList<PackageChange> Changed);
public record PackageChange(string Name, string VersionBefore, bool DirectReferenceBefore, string VersionAfter, bool DirectReferenceAfter);

public class PackageComparison
{
    public IEnumerable<Project> Compare(ProjectPackagesOutput before, ProjectPackagesOutput after)
    {
        var allProjectsAndFrameworks = new HashSet<string>();
        var projects = new Dictionary<ProjectAndFramework, PackageChanges>();

        foreach (var (projectAndFramework, frameworkPackages) in after.GetPackagesByProjectAndFramework())
        {
            if (!projects.ContainsKey(projectAndFramework))
            {
                projects[projectAndFramework] = new PackageChanges(new List<Package>(), new List<Package>(), new List<PackageChange>());
            }

            foreach (var package in frameworkPackages!)
            {
                if (!before.TryGetPackage(projectAndFramework, package.Name, out var packageBefore))
                {
                    projects[projectAndFramework].Added.Add(package);
                }
                else if (packageBefore.Version != package.Version || packageBefore.DirectReference != package.DirectReference)
                {
                    projects[projectAndFramework].Changed.Add(new PackageChange(package.Name, packageBefore.Version, packageBefore.DirectReference, package.Version, package.DirectReference));
                }
            }
        }

        foreach (var (projectAndFramework, frameworkPackages) in before.GetPackagesByProjectAndFramework())
        {
            foreach (var package in frameworkPackages!)
            {
                if (!after.TryGetPackage(projectAndFramework, package.Name, out var packageBefore))
                {
                    projects[projectAndFramework].Removed.Add(package);
                }
            }
        }

        var uniqueProjects = new Dictionary<string, List<(string framework, PackageChanges changes)>>();

        foreach (var (projectAndFramework, changes) in projects)
        {
            if (!uniqueProjects.ContainsKey(projectAndFramework.ProjectName))
                uniqueProjects[projectAndFramework.ProjectName] = new List<(string, PackageChanges)>();

            uniqueProjects[projectAndFramework.ProjectName].Add((projectAndFramework.FrameworkName, changes));
        }

        return uniqueProjects
            .Select(projectAndFramwork => new Project(projectAndFramwork.Key, projectAndFramwork.Value.Select(f => new ProjectFramework(f.framework, f.changes)).ToList()));
    }
}
