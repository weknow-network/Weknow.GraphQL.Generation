using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

public class GenerationInput
{
    public GenerationInput(ClassDeclarationSyntax syntax, INamedTypeSymbol? symbol)
    {
        Syntax = syntax;
        Symbol = symbol ?? throw new NullReferenceException(nameof(symbol));
    }

    public ClassDeclarationSyntax Syntax { get; }
    public INamedTypeSymbol Symbol { get; }
}
