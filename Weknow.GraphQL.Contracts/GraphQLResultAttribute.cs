namespace Weknow.GraphQL
{
    /// <summary>
    /// Code generation decoration of GraphQL result wrapper attribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class GenGqlResultContainerAttribute : Attribute
    {
        public const string DefaultSuffix = "QlResults";

        public GenGqlResultContainerAttribute(string operationName)
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
        /// Gets or sets the suffix of the type.
        /// </summary>
        public string Suffix { get; set; } = DefaultSuffix;
    }
}
