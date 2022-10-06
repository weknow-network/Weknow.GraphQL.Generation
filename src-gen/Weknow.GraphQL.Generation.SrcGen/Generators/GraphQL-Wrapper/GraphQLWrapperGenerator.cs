using System.CodeDom.Compiler;
using System.Collections.Immutable;
using System.Text;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Weknow.GraphQL;

[Generator]
public class GraphQLWrapperGenerator : IIncrementalGenerator
{
    private const string TARGET_ATTRIBUTE = nameof(GenGqlResultContainerAttribute);
    private static readonly string TARGET_SHORT_ATTRIBUTE = nameof(GenGqlResultContainerAttribute).Replace("Attribute", "");
    private const string DESC = "Description = \"";
    private const string SUFFIX = "Suffix = \"";

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
            GenerateWrapper(spc, compilation, item);
        }
    }

    #endregion // Generate

    #region GenerateWrapper

    /// <summary>
    /// Generates a wrapper.
    /// </summary>
    /// <param name="spc">The SPC.</param>
    /// <param name="compilation">The compilation.</param>
    /// <param name="item">The item.</param>
    private static void GenerateWrapper(
        SourceProductionContext spc,
        Compilation compilation,
        GenerationInput item)
    {
        var symbol = item.Symbol;
        TypeDeclarationSyntax syntax = item.Syntax;


        var args = syntax.AttributeLists.Single().Attributes.Single(m => AttributePredicate(m.Name.ToString())).ArgumentList?.Arguments;

        var cls = syntax.Identifier.Text;
        string? nsCandidate = symbol.ContainingNamespace.ToString();
        string ns = nsCandidate != null ? $"namespace {nsCandidate};{Environment.NewLine}" : "";
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
        var operationName = args?.First()
            .GetText()
            .ToString()
            .Replace("\"", "")
            .ToString();

        StringBuilder sb = new();
        sb.AppendLine(@$"
#pragma warning disable CS8618 // ignore nullable.
{ns}
/// <summary>
/// {description}.
/// </summary>
[System.CodeDom.Compiler.GeneratedCode(""Weknow.GraphQL.Generation"", ""1.0.0"")]
public sealed class {cls}{suffix}
{{
    /// <summary>
    /// Result slot.
    /// </summary>
    public {cls}[] {operationName} {{ get; init; }}
}}
");
        spc.AddSource($"{cls}QlWrapper.cs", sb.ToString());
    }

    #endregion // GenerateWrapper

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