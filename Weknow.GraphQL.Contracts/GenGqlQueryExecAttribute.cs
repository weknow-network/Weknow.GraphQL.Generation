namespace Weknow.GraphQL
{
    /// <summary>
    /// Code generation decoration of GraphQL result wrapper attribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false)]
    public class GenGqlQueryExecAttribute : Attribute
    {
        public const string DefaultSuffix = "QlResults";

        public GenGqlQueryExecAttribute(string operationName)
        {
            OperationName = operationName;
        }
        /// <summary>
        /// The name of the result wrapper property.
        /// </summary>
        public string OperationName { get; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the namespace.
        /// </summary>
        public string? Namespace { get; set; } 
    }
}
