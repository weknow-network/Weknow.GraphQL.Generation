namespace Weknow.GraphQL
{
    /// <summary>
    /// Code generation decoration of GraphQL result wrapper attribute
    /// </summary>
    [AttributeUsage(AttributeTargets.All)]
    public class GraphQLResultAttribute : Attribute
    {
        public GraphQLResultAttribute(string operationName)
        {
            OperationName = operationName;
        }
        public string OperationName { get; }
        public string Description { get; set; } = string.Empty;
    }
}
