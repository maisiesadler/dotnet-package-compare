if (args.Length < 2) throw new InvalidOperationException("Expected args <file-before> <file-after>");

var before = args[0];
var after = args[1];

if (!File.Exists(before)) throw new InvalidOperationException($"Before file '{before}' cannot be found.");
if (!File.Exists(after)) throw new InvalidOperationException($"After file '{after}' cannot be found.");

var services = new ServiceCollection();
services.AddPackageComparison(before, after);

var sp = services.BuildServiceProvider();

var interactor = sp.GetRequiredService<CompareSolutionsInteractor>();

var projects = await interactor.Execute();

var anyChanges = projects
                    .SelectMany(p => p.Frameworks.Select(framework => (ChangedCount(framework))))
                    .Any(changes => changes > 0);

if (!anyChanges) return;

Console.WriteLine($"## Changed Packages");

foreach (var project in projects)
{
    Console.WriteLine($"### {project.Name}");

    foreach (var framework in project.Frameworks)
    {
        var diffCount = ChangedCount(framework);
        if (diffCount == 0) continue;
        Console.WriteLine($"#### {framework.Name} ({diffCount} differences)");

        if (framework.PackageChanges.Added.Any())
        {
            Console.WriteLine($"##### Added ({framework.PackageChanges.Added.Count})");
            foreach (var package in framework.PackageChanges.Added)
            {
                var direct = package.DirectReference ? "(direct)" : "(transient)";
                Console.WriteLine($"- {package.Name} [{package.Version} {direct}]");
            }
            Console.WriteLine();
        }

        if (framework.PackageChanges.Removed.Any())
        {
            Console.WriteLine($"##### Removed ({framework.PackageChanges.Removed.Count})");
            foreach (var package in framework.PackageChanges.Removed)
            {
                var direct = package.DirectReference ? "(direct)" : "(transient)";
                Console.WriteLine($"- {package.Name} [{package.Version} {direct}]");
            }
        }

        if (framework.PackageChanges.Changed.Any())
        {
            Console.WriteLine($"##### Changed ({framework.PackageChanges.Changed.Count})");
            foreach (var packageChange in framework.PackageChanges.Changed)
            {
                var directBefore = packageChange.DirectReferenceBefore ? "(direct)" : "(transient)";
                var directAfter = packageChange.DirectReferenceAfter ? "(direct)" : "(transient)";
                Console.WriteLine($"- {packageChange.Name} [{packageChange.VersionBefore} {directBefore}] -> [{packageChange.VersionAfter} {directAfter}]");
            }
        }
    }
}

int ChangedCount(ProjectFramework projectFramework)
{
    return projectFramework.PackageChanges.Added.Count + projectFramework.PackageChanges.Removed.Count + projectFramework.PackageChanges.Changed.Count;
}
