using System.Collections.Immutable;

namespace PackageLister;

public record ProjectPackages(string Name, IImmutableList<FrameworkPackages> Frameworks);
public record FrameworkPackages(string Name, IImmutableList<Package> Packages);
public record Package(string Name, string Version, bool DirectReference);

public record Project(string Name, IList<ProjectFramework> Frameworks);
public record ProjectFramework(string Name, PackageChanges PackageChanges);
public record PackageChanges(IList<Package> Added, IList<Package> Removed, IList<PackageChange> Changed);
public record PackageChange(string Name, string VersionBefore, bool DirectReferenceBefore, string VersionAfter, bool DirectReferenceAfter);

public class PackageComparison
{
    public IEnumerable<Project> Compare(List<ProjectPackages> before, List<ProjectPackages> after)
    {
        var allProjectsAndFrameworks = new HashSet<string>();
        var projectsByFrameworkByPackageBefore = GetPackagesByNameByProjectByFramework(before);
        var projectsByFrameworkByPackageAfter = GetPackagesByNameByProjectByFramework(after);

        var projects = new Dictionary<InProgressProjectAndFramework, PackageChanges>();

        foreach (var (projectAndFramework, frameworkPackages) in projectsByFrameworkByPackageAfter)
        {
            if (!projects.ContainsKey(projectAndFramework))
            {
                projects[projectAndFramework] = new PackageChanges(new List<Package>(), new List<Package>(), new List<PackageChange>());
            }

            foreach (var (packageName, package) in frameworkPackages)
            {
                if (!projectsByFrameworkByPackageBefore.TryGetValue(projectAndFramework, out var projectAndFrameworkBefore)
                    || !projectAndFrameworkBefore.TryGetValue(packageName, out var packageBefore))
                {
                    projects[projectAndFramework].Added.Add(package);
                }
                else if (packageBefore.Version != package.Version)
                {
                    projects[projectAndFramework].Changed.Add(new PackageChange(packageName, packageBefore.Version, packageBefore.DirectReference, package.Version, package.DirectReference));
                }
            }
        }

        foreach (var (projectAndFramework, frameworkPackages) in projectsByFrameworkByPackageBefore)
        {
            if (!projects.ContainsKey(projectAndFramework))
            {
                projects[projectAndFramework] = new PackageChanges(new List<Package>(), new List<Package>(), new List<PackageChange>());
            }

            foreach (var (packageName, package) in frameworkPackages)
            {
                if (!projectsByFrameworkByPackageAfter.TryGetValue(projectAndFramework, out var projectAndFrameworkAfter)
                    || !projectAndFrameworkAfter.ContainsKey(packageName))
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

    private record InProgressProjectAndFramework(string ProjectName, string FrameworkName);
    private Dictionary<InProgressProjectAndFramework, Dictionary<string, Package>> GetPackagesByNameByProjectByFramework(List<ProjectPackages> projectPackages)
    {
        var projects = new Dictionary<InProgressProjectAndFramework, Dictionary<string, Package>>();
        foreach (var project in projectPackages)
        {
            foreach (var framework in project.Frameworks)
            {
                var projectAndFramework = new InProgressProjectAndFramework(project.Name, framework.Name);
                projects.Add(projectAndFramework, new Dictionary<string, Package>());

                foreach (var package in framework.Packages)
                {
                    projects[projectAndFramework].Add(package.Name, package);
                }
            }
        }

        return projects;
    }
}
