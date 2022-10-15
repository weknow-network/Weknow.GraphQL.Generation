namespace Weknow.GraphQL
{
    /// <summary>
    /// Code generation decoration of GraphQL result wrapper attribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false)]
    public class GenGqlResultContainerAttribute : Attribute
    {
        public const string DefaultSuffix = "QlResults";

        public GenGqlResultContainerAttribute(string operationName, bool plural = true)
        {
            OperationName = operationName;
            Plural = plural;
        }

        /// <summary>
        /// The name of the result wrapper property.
        /// </summary>
        public string OperationName { get; }

        /// <summary>
        /// Indicating whether to generate a plural result (array).
        /// </summary>
        public bool Plural { get; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the suffix of the result's type.
        /// </summary>
        public string ResultSuffix { get; set; } = DefaultSuffix;
    }
}
