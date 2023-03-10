using System.Collections.Immutable;

namespace PackageLister;

public record ProjectPackages(string Name, IImmutableList<FrameworkPackages> Frameworks);
public record FrameworkPackages(string Name, IImmutableList<Package> Packages);
public record Package(string Name, string Version, bool DirectReference);

public record Project(string Name, IList<ProjectFramework> Frameworks);
public record ProjectFramework(string Name, PackageChanges PackageChanges);
public record PackageChanges(IList<Package> Added, IList<Package> Removed);

file record InProgressProjectAndFramework(string ProjectName, string FrameworkName);

public class PackageComparison
{
    public IEnumerable<Project> Compare(List<ProjectPackages> before, List<ProjectPackages> after)
    {
        var allProjectsAndFrameworks = new HashSet<string>();
        var projectsByFrameworkByPackageBefore = GetPackagesByNameByProjectByFramework(before);
        var projectsByFrameworkByPackageAfter = GetPackagesByNameByProjectByFramework(after);

        var projects = new Dictionary<InProgressProjectAndFramework, PackageChanges>();

        foreach (var (project, projectFrameworks) in projectsByFrameworkByPackageAfter)
        {
            foreach (var (framework, frameworkPackages) in projectFrameworks)
            {
                var projectAndFramework = new InProgressProjectAndFramework(project, framework);
                if (!projects.ContainsKey(projectAndFramework))
                {
                    projects[projectAndFramework] = new PackageChanges(new List<Package>(), new List<Package>());
                }

                foreach (var (packageName, package) in frameworkPackages)
                {
                    if (!projectsByFrameworkByPackageBefore.TryGetValue(project, out var projectByFrameworkByPackageBefore)
                        || !projectByFrameworkByPackageBefore.TryGetValue(framework, out var frameworkByPackageBefore)
                        || !frameworkByPackageBefore.ContainsKey(packageName))
                        projects[projectAndFramework].Added.Add(package);
                }
            }
        }

        foreach (var (project, projectFrameworks) in projectsByFrameworkByPackageBefore)
        {
            foreach (var (framework, frameworkPackages) in projectFrameworks)
            {
                var projectAndFramework = new InProgressProjectAndFramework(project, framework);
                if (!projects.ContainsKey(projectAndFramework))
                {
                    projects[projectAndFramework] = new PackageChanges(new List<Package>(), new List<Package>());
                }

                foreach (var (packageName, package) in frameworkPackages)
                {
                    if (!projectsByFrameworkByPackageAfter.TryGetValue(project, out var projectByFrameworkByPackageAfter)
                        || !projectByFrameworkByPackageAfter.TryGetValue(framework, out var frameworkByPackageAfter)
                        || !frameworkByPackageAfter.ContainsKey(packageName))
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

    private Dictionary<string, Dictionary<string, Dictionary<string, Package>>> GetPackagesByNameByProjectByFramework(List<ProjectPackages> projectPackages)
    {
        var projects = new Dictionary<string, Dictionary<string, Dictionary<string, Package>>>();
        foreach (var project in projectPackages)
        {
            projects.Add(project.Name, new Dictionary<string, Dictionary<string, Package>>());

            foreach (var framework in project.Frameworks)
            {
                projects[project.Name].Add(framework.Name, new Dictionary<string, Package>());

                foreach (var package in framework.Packages)
                {
                    projects[project.Name][framework.Name].Add(package.Name, package);
                }
            }
        }

        return projects;
    }
}
