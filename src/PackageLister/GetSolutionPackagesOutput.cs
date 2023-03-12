using System.Text.RegularExpressions;

namespace PackageLister;

public static class GetSolutionPackagesOutput
{
    public static SolutionListPackagesOutput Read(string[] output)
    {
        var solutionListPackagesOutput = new SolutionListPackagesOutput();
        foreach (var (projectAndFramework, packages) in GetPackages(output))
        {
            var addPackage = solutionListPackagesOutput.GetAddPackageFunction(projectAndFramework.ProjectName, projectAndFramework.FrameworkName);
            foreach (var package in packages)
                addPackage(package);
        }

        return solutionListPackagesOutput;
    }

    private static IEnumerable<PackagesByProjectAndFramework> GetPackages(string[] output)
    {
        CurrentProject? currentProject = null;

        foreach (var line in output)
        {
            switch (line)
            {
                case var l when Regex.Match(line, @"^Project '(.*)' has the following package references$") is { Success: true } match:
                    {
                        if (currentProject != null)
                        {
                            foreach (var packagesByFramework in currentProject.AllPackagesByFramework)
                            {
                                yield return packagesByFramework;
                            }
                        }
                        currentProject = new(match.Groups[1].Value);
                        break;
                    }
                default:
                    {
                        currentProject?.Add(line);
                        break;
                    }
            }
        }

        if (currentProject != null)
        {
            foreach (var packagesByFramework in currentProject.AllPackagesByFramework)
            {
                yield return packagesByFramework;
            }
        }
    }

    private class CurrentFramework
    {
        private readonly string _name;
        private readonly List<Package> _packages = new();
        private bool _transient = false;

        public string Name => _name;
        public List<Package> Packages => _packages;

        public CurrentFramework(string name)
        {
            _name = name;
        }

        public void Add(string line)
        {
            switch (line)
            {
                case var l when Regex.Match(line, @"^\s*> (\S+)\s+\S*\s+(\S+)\s*$") is { Success: true } match:
                    {
                        _packages.Add(new Package(match.Groups[1].Value, match.Groups[2].Value, !_transient));
                        break;
                    }
                case var l when Regex.Match(line, @"^\s*Transitive Package") is { Success: true } match:
                    {
                        _transient = true;
                        break;
                    }
            }
        }
    }

    private class CurrentProject
    {
        private readonly string _name;
        private readonly List<CurrentFramework> _frameworks = new();
        private CurrentFramework? _currentFramework;

        public CurrentProject(string name)
        {
            _name = name;
        }

        public List<PackagesByProjectAndFramework> AllPackagesByFramework
            => _frameworks.Select(f => new PackagesByProjectAndFramework(new ProjectAndFramework(_name, f.Name), f.Packages)).ToList();

        public void Add(string line)
        {
            switch (line)
            {
                case var l when Regex.Match(line, @"^\s*\[([^\[\]]+)\]:") is { Success: true } match:
                    {
                        _currentFramework = new CurrentFramework(match.Groups[1].Value);
                        _frameworks.Add(_currentFramework);
                        break;
                    }
                default:
                    {
                        _currentFramework?.Add(line);
                        break;
                    }
            }
        }
    }
}
