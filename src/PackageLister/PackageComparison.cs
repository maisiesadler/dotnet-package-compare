using System.Collections.Immutable;

namespace PackageLister;

public record ProjectPackages(string Name, IImmutableList<FrameworkPackages> Frameworks);
public record FrameworkPackages(string Name, IImmutableList<Package> Packages);
public record Package(string Name, string Version, bool DirectReference);

public record Project(string Name, IList<ProjectFramework> Frameworks);
public record ProjectFramework(string Name, PackageChanges PackageChanges);
public record PackageChanges(IList<Package> Added, IList<Package> Removed);

file record InProgressProject(string Name, Dictionary<string, ProjectFramework> Frameworks);

public class PackageComparison
{
    public IEnumerable<Project> Compare(List<ProjectPackages> before, List<ProjectPackages> after)
    {
        var projectsByFrameworkByPackageBefore = GetPackagesByNameByProjectByFramework(before);
        var projectsByFrameworkByPackageAfter = GetPackagesByNameByProjectByFramework(after);

        var projects = new Dictionary<string, InProgressProject>();

        foreach (var (project, projectFrameworks) in projectsByFrameworkByPackageAfter)
        {
            if (!projects.ContainsKey(project))
            {
                projects[project] = new InProgressProject(project, new Dictionary<string, ProjectFramework>());
            }

            foreach (var (framework, frameworkPackages) in projectFrameworks)
            {
                if (!projects[project].Frameworks.ContainsKey(framework))
                {
                    projects[project].Frameworks[framework] = new ProjectFramework(framework, new PackageChanges(new List<Package>(), new List<Package>()));
                }
                var changes = projects[project].Frameworks[framework].PackageChanges;

                foreach (var (packageName, package) in frameworkPackages)
                {
                    if (!projectsByFrameworkByPackageBefore.TryGetValue(project, out var projectByFrameworkByPackageBefore)
                        || !projectByFrameworkByPackageBefore.TryGetValue(framework, out var frameworkByPackageBefore)
                        || !frameworkByPackageBefore.ContainsKey(packageName))
                        changes.Added.Add(package);
                }
            }
        }

        foreach (var (project, projectFrameworks) in projectsByFrameworkByPackageBefore)
        {
            if (!projects.ContainsKey(project))
            {
                projects[project] = new InProgressProject(project, new Dictionary<string, ProjectFramework>());
            }

            foreach (var (framework, frameworkPackages) in projectFrameworks)
            {
                if (!projects[project].Frameworks.ContainsKey(framework))
                {
                    projects[project].Frameworks[framework] = new ProjectFramework(framework, new PackageChanges(new List<Package>(), new List<Package>()));
                }
                var changes = projects[project].Frameworks[framework].PackageChanges;

                foreach (var (packageName, package) in frameworkPackages)
                {
                    if (!projectsByFrameworkByPackageAfter.TryGetValue(project, out var projectByFrameworkByPackageAfter)
                        || !projectByFrameworkByPackageAfter.TryGetValue(framework, out var frameworkByPackageAfter)
                        || !frameworkByPackageAfter.ContainsKey(packageName))
                        changes.Removed.Add(package);
                }
            }
        }

        return projects
            .Select(p => new Project(p.Key, p.Value.Frameworks.Select(v => v.Value).ToList()));
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
