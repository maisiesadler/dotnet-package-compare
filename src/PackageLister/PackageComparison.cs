namespace PackageLister;

public record Project(string Name, IList<ProjectFramework> Frameworks);
public record ProjectFramework(string Name, PackageChanges PackageChanges);
public record PackageChanges(IList<Package> Added, IList<Package> Removed, IList<PackageChange> Changed);
public record PackageChange(string Name, string VersionBefore, bool DirectReferenceBefore, string VersionAfter, bool DirectReferenceAfter);

internal class PackageComparison
{
    public static IEnumerable<Project> Compare(SolutionListPackagesOutput before, SolutionListPackagesOutput after)
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
