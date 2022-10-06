namespace Weknow.GraphQL.Generation.SrcGen.Playground
{
    // file3.cs
    [GraphQLResult("Pong")]
    public class Class3 { }


/// <summary>
/// GraphQL entity wrapper of Class3.
/// </summary>
class Class3QlWrapper1
{
        /// <summary>
        /// Result slot.
        /// </summary>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public Class3 Pong { get; init; }
    }
}
