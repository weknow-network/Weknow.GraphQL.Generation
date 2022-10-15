using System.CodeDom.Compiler;
using System.Collections.Immutable;
using System.Text;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Weknow.GraphQL;

[Generator]
public class GraphQLSendQuaryGenerator : IIncrementalGenerator
{
    private const string TARGET_ATTRIBUTE = nameof(GenGqlQueryExecAttribute);
    private static readonly string TARGET_SHORT_ATTRIBUTE = nameof(GenGqlQueryExecAttribute).Replace("Attribute", "");
    private const string DESC = "Description = \"";
    private const string SUFFIX = "ResultSuffix = \"";
    private const string NAMESPACE = "Namespace = \"";

    #region Initialize

    /// <summary>
    /// Called to initialize the generator and register generation steps via callbacks
    /// on the <paramref name="context" />
    /// </summary>
    /// <param name="context">The <see cref="T:Microsoft.CodeAnalysis.IncrementalGeneratorInitializationContext" /> to register callbacks on</param>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {

        #region var classDeclarations = ...

#pragma warning disable CS8619
        IncrementalValuesProvider<GenerationInput> classDeclarations =
                context.SyntaxProvider
                    .CreateSyntaxProvider(
                        predicate: static (s, _) => ShouldTriggerGeneration(s),
                        transform: static (ctx, _) => ToGenerationInput(ctx))
                    .Where(static m => m is not null);
#pragma warning restore CS8619

        #endregion // var classDeclarations = ...

        #region ShouldTriggerGeneration

        /// <summary>
        /// Indicate whether the node should trigger a source generation />
        /// </summary>
        static bool ShouldTriggerGeneration(SyntaxNode node)
        {

            //    if (node is RecordDeclarationSyntax r)
            //    { 
            //        bool hasRecordAttributes = r.AttributeLists.Any(m => m.Attributes.Any(m1 =>
            //                AttributePredicate(m1.Name.ToString())));

            //        return hasRecordAttributes;
            //    }
            if (!(node is TypeDeclarationSyntax t)) return false;

            bool hasAttributes = t.AttributeLists.Any(m => m.Attributes.Any(m1 =>
                    AttributePredicate(m1.Name.ToString())));

            return hasAttributes;
        };

        #endregion // ShouldTriggerGeneration

        IncrementalValueProvider<(Compilation, ImmutableArray<GenerationInput>)> compilationAndClasses
            = context.CompilationProvider.Combine(classDeclarations.Collect());

        // register a code generator for the triggers
        context.RegisterSourceOutput(compilationAndClasses, Generate);
    }

    #endregion // Initialize

    #region Generate

    /// <summary>
    /// Source generates loop.
    /// </summary>
    /// <param name="spc">The SPC.</param>
    /// <param name="source">The source.</param>
    private static void Generate(
        SourceProductionContext spc,
        (Compilation compilation,
        ImmutableArray<GenerationInput> items) source)
    {
        var (compilation, items) = source;
        foreach (GenerationInput item in items)
        {
            GenerateQueryExtension(spc, compilation, item);
        }
    }

    #endregion // Generate

    #region GenerateQueryExtension

    /// <summary>
    /// Generates the code.
    /// </summary>
    /// <param name="spc">The SPC.</param>
    /// <param name="compilation">The compilation.</param>
    /// <param name="item">The item.</param>
    private static void GenerateQueryExtension(
        SourceProductionContext spc,
        Compilation compilation,
        GenerationInput item)
    {
        GraphQLResultContainerGenerator.GenerateWrapper(spc, compilation, item, AttributePredicate);

        var symbol = item.Symbol;
        TypeDeclarationSyntax syntax = item.Syntax;


        var args = syntax.AttributeLists.Where(m => m.Attributes.Any(m1 =>
                                                        AttributePredicate(m1.Name.ToString())))
                                        .Single()
                                        .Attributes.Single(m => AttributePredicate(m.Name.ToString())).ArgumentList?.Arguments;

        var cls = syntax.Identifier.Text;
        var nsOverride = args?.Select(m => m.ToString())
                        .FirstOrDefault(m => m.StartsWith(NAMESPACE))
                        ?.Replace(NAMESPACE, "")
                        .Replace("\"", "")
                        .Trim();
        string? nsCandidate = symbol.ContainingNamespace.ToString();
        string ns = "";
        if (nsOverride != null)
            ns = $"namespace {nsOverride};{Environment.NewLine}";
        else if (nsCandidate != null)
            ns = $"namespace {nsCandidate};{Environment.NewLine}";
        string use = nsOverride == null ? "" : $"using {nsCandidate};{Environment.NewLine}";


        var description = args?.Select(m => m.ToString())
                        .FirstOrDefault(m => m.StartsWith(DESC))
                        ?.Replace(DESC, "")
                        .Replace("\"", "")
                        .Trim() ?? $"GraphQL entity wrapper of {cls}";
        var suffix = args?.Select(m => m.ToString())
                        .FirstOrDefault(m => m.StartsWith(SUFFIX))
                        ?.Replace(SUFFIX, "")
                        .Replace("\"", "")
                        .Trim() ?? GenGqlResultContainerAttribute.DefaultSuffix;

        string operationName = args?.First()
            .GetText()
            .ToString()
            .Replace("\"", "")
            .ToString() ?? string.Empty;
        string? pluralStr = args?.Skip(1)?.FirstOrDefault()
            ?.GetText()
            ?.ToString()
            ?.Replace("\"", "")
            ?.ToString();
        bool plural = pluralStr == null || pluralStr != "false";
        string array = plural ? "[]" : string.Empty;
        string name = operationName;
        if (!string.IsNullOrEmpty(name)  && char.IsLower(name[0]))
        {
            name = $"{char.ToUpper(name[0])}{name.Substring(1)}";
        }

        StringBuilder sb = new();
        sb.AppendLine(@$"
using GraphQL;
using GraphQL.Client.Abstractions;
using System.Threading.Tasks;
{use}{ns}
/// <summary>
/// {description}.
/// </summary>
[System.CodeDom.Compiler.GeneratedCode(""Weknow.GraphQL.Generation"", ""1.0.0"")]
public static class {name}Extensions
{{
    /// <summary>
    /// Execute {operationName}
    /// </summary>
    public static async ValueTask<({cls}{array}? Data, GraphQLError[]? Errors)> QueryGql{name} (
                        this IGraphQLClient client, 
                        GraphQLRequest query, 
                        CancellationToken cancellationToken = default)
    {{
        GraphQLResponse<{cls}{suffix}> response = await client.SendQueryAsync<{cls}{suffix}>(query, cancellationToken);
        {cls}{array}? data = response.Errors == null ? response?.Data?.{operationName} : null;
        return (data, response?.Errors);
    }}
}}
");
        spc.AddSource($"{cls}{suffix}.{operationName}Extensions.cs", sb.ToString());
    }

    #endregion // GenerateQueryExtension

    #region ToGenerationInput

    /// <summary>
    /// Converts to generation-input.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <returns></returns>
    private static GenerationInput ToGenerationInput(GeneratorSyntaxContext context)
    {
        var declarationSyntax = (TypeDeclarationSyntax)context.Node;

        var symbol = context.SemanticModel.GetDeclaredSymbol(declarationSyntax);
        if (symbol == null) throw new NullReferenceException($"Code generated symbol of {nameof(declarationSyntax)} is missing");
        return new GenerationInput(declarationSyntax, symbol as INamedTypeSymbol);
    }

    #endregion // ToGenerationInput

    /// <summary>
    /// The predicate whether match to the target attribute
    /// </summary>
    private static bool AttributePredicate(string candidate)
    {
        int len = candidate.LastIndexOf(".");
        if (len != -1)
            candidate = candidate.Substring(len + 1);

        return candidate == TARGET_ATTRIBUTE ||
               candidate == TARGET_SHORT_ATTRIBUTE;
    }
}