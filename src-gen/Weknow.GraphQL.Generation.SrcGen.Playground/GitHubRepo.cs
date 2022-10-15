namespace Weknow.GraphQL.Generation.SrcGen.Playground
{
    // file3.cs
    [GenGqlQueryExec("repository")]
    public record struct GitHubRepo
    {
        DateTime CreatedAt { get; init; }
        int ForkCount { get; init; }
    }
}
