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

foreach (var project in projects)
{
    Console.WriteLine($"Project '{project.Name}' has '{project.Frameworks.Count}' frameworks");

    foreach (var framework in project.Frameworks)
    {
        var diffCount = framework.PackageChanges.Added.Count + framework.PackageChanges.Removed.Count + framework.PackageChanges.Changed.Count;
        Console.WriteLine($"- Framework '{framework.Name}' has '{diffCount}' differences");

        if (framework.PackageChanges.Added.Any())
        {
            Console.WriteLine($"  Added:");
            foreach (var package in framework.PackageChanges.Added)
            {
                var direct = package.DirectReference ? "(direct)" : "(transient)";
                Console.WriteLine($"  - {package.Name} [{package.Version} {direct}]");
            }
        }

        if (framework.PackageChanges.Removed.Any())
        {
            Console.WriteLine($"  Removed:");
            foreach (var package in framework.PackageChanges.Removed)
            {
                var direct = package.DirectReference ? "(direct)" : "(transient)";
                Console.WriteLine($"  - {package.Name} [{package.Version} {direct}]");
            }
        }

        if (framework.PackageChanges.Changed.Any())
        {
            Console.WriteLine($"  Changed:");
            foreach (var packageChange in framework.PackageChanges.Changed)
            {
                var directBefore = packageChange.DirectReferenceBefore ? "(direct)" : "(transient)";
                var directAfter = packageChange.DirectReferenceAfter ? "(direct)" : "(transient)";
                Console.WriteLine($"  - {packageChange.Name} [{packageChange.VersionBefore} {directBefore}] -> [{packageChange.VersionAfter} {directAfter}]");
            }
        }
    }
}
