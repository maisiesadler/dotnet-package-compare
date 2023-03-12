using Microsoft.Extensions.DependencyInjection;

namespace PackageLister;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPackageComparison(
        this IServiceCollection services, string beforeFileLocation, string afterFileLocation)
    {
        services.AddTransient<IGetProjectsBeforeQuery>(_ => new GetProjectsFromFileQuery(beforeFileLocation));
        services.AddTransient<IGetProjectsAfterQuery>(_ => new GetProjectsFromFileQuery(afterFileLocation));
        services.AddTransient<CompareSolutionsInteractor>();
        return services;
    }
}
