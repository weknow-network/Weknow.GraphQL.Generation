namespace Weknow.GraphQL
{
    /// <summary>
    /// Code generation decoration of GraphQL result wrapper attribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class GenGqlResultContainerAttribute : Attribute
    {
        public GenGqlResultContainerAttribute(string operationName)
        {
            OperationName = operationName;
        }
        public string OperationName { get; }
        public string Description { get; set; } = string.Empty;
    }
}
