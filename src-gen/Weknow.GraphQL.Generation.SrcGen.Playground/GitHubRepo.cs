namespace Weknow.GraphQL.Generation.SrcGen.Playground
{
    // file3.cs
    [GenGqlQueryExec("repository", false)]
    public record struct GitHubRepo
    {
        public DateTime CreatedAt { get; init; }
        public int ForkCount { get; init; }
    }
}
