using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace PenmansAnalyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
internal class VisualBasicAnalyzer : DiagnosticAnalyzer
{
    private const string DIAGNOSTIC_ID = "PAVBI001";
    private static readonly DiagnosticDescriptor Rule = new (
        DIAGNOSTIC_ID,
        "Usage of Microsoft.VisualBasic namespace detected",
        "Avoid using the Microsoft.VisualBasic namespace: {0}",
        "Usage",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeUsingDirective, SyntaxKind.UsingDirective);
        context.RegisterSyntaxNodeAction(AnalyzeIdentifier, SyntaxKind.IdentifierName);
        context.RegisterSyntaxNodeAction(AnalyzeMemberAccess, SyntaxKind.SimpleMemberAccessExpression);
    }

    private static void AnalyzeUsingDirective(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is UsingDirectiveSyntax { Name: not null } usingDirective &&
            usingDirective.Name.ToString().Contains("VisualBasic"))
        {
            Diagnostic diagnostic = Diagnostic.Create(Rule, usingDirective.GetLocation(),
                $"Avoid using the {usingDirective.Name} namespace.");
            context.ReportDiagnostic(diagnostic);
        }
    }

    private static void AnalyzeIdentifier(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is IdentifierNameSyntax identifier)
        {
            SymbolInfo symbolInfo = context.SemanticModel.GetSymbolInfo(identifier);
            ISymbol? symbol = symbolInfo.Symbol;

            // Ensure the symbol is not null and has a containing namespace
            if (symbol is INamespaceOrTypeSymbol { ContainingNamespace: not null } namespaceOrTypeSymbol &&
                namespaceOrTypeSymbol.ContainingNamespace.ToDisplayString().Contains("VisualBasic"))
            {
                Diagnostic diagnostic = Diagnostic.Create(
                    Rule,
                    identifier.GetLocation(),
                    $"Avoid referencing types or members from the {namespaceOrTypeSymbol.ContainingNamespace} namespace."
                );
                context.ReportDiagnostic(diagnostic);
            }
        }
    }

    private static void AnalyzeMemberAccess(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is MemberAccessExpressionSyntax memberAccess)
        {
            SymbolInfo symbolInfo = context.SemanticModel.GetSymbolInfo(memberAccess);
            ISymbol? symbol = symbolInfo.Symbol;

            // Ensure the symbol is not null and has a containing namespace
            if (symbol is { ContainingNamespace: not null } &&
                symbol.ContainingNamespace.ToDisplayString().Contains("VisualBasic"))
            {
                Diagnostic diagnostic = Diagnostic.Create(
                    Rule,
                    memberAccess.GetLocation(),
                    $"Avoid referencing types or members from the {symbol.ContainingNamespace} namespace."
                );
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}
