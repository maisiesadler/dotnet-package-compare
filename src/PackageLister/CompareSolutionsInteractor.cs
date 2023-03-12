namespace PackageLister;

public interface IGetProjectsBeforeQuery
{
    public Task<SolutionListPackagesOutput> Execute();
}

public interface IGetProjectsAfterQuery
{
    public Task<SolutionListPackagesOutput> Execute();
}

public class CompareSolutionsInteractor
{
    private readonly IGetProjectsBeforeQuery _getProjectsBeforeQuery;
    private readonly IGetProjectsAfterQuery _getProjectsAfterQuery;

    public CompareSolutionsInteractor(IGetProjectsBeforeQuery getProjectsBeforeQuery, IGetProjectsAfterQuery getProjectsAfterQuery)
    {
        _getProjectsBeforeQuery = getProjectsBeforeQuery;
        _getProjectsAfterQuery = getProjectsAfterQuery;
    }

    public async Task<IList<Project>> Execute()
    {
        var before = await _getProjectsBeforeQuery.Execute();
        var after = await _getProjectsAfterQuery.Execute();

        return PackageComparison.Compare(before, after).ToArray();
    }
}
