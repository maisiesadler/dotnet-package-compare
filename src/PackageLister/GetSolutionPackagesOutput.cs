using System.Text.RegularExpressions;

namespace PackageLister;

public class GetSolutionPackagesOutput
{
    public SolutionListPackagesOutput Read(string[] output)
    {
        var solutionListPackagesOutput = new SolutionListPackagesOutput();
        foreach (var (projectAndFramework, packages) in GetPackages(output))
        {
            foreach (var package in packages)
                solutionListPackagesOutput.Add(projectAndFramework.ProjectName, projectAndFramework.FrameworkName, package);
        }

        return solutionListPackagesOutput;
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

        public ProjectAndFramework ProjectAndFramework => new ProjectAndFramework(_name, _currentFramework!.Name);
        public List<Package> Packages => _currentFramework!.Packages;

        public void Add(string line)
        {
            switch (line)
            {
                case var l when Regex.Match(line, @"^\s*\[([^\[\]]+)\]:") is { Success: true } match:
                    {
                        _currentFramework = new CurrentFramework(match.Groups[1].Value);
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

    private IEnumerable<(ProjectAndFramework projectAndFramework, List<Package> packages)> GetPackages(string[] output)
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
                            yield return (currentProject.ProjectAndFramework, currentProject.Packages);
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
            yield return (currentProject.ProjectAndFramework, currentProject.Packages);
        }
    }
}
