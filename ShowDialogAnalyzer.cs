using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace PenmansAnalyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RadWindowShowDialogAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "PARWP005";

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId,
        "Avoid calling ShowDialog on RadWindow",
        "Avoid calling the ShowDialog method on a RadWindow object",
        "Usage",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
    }

    private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
    {
        InvocationExpressionSyntax invocation = (InvocationExpressionSyntax)context.Node;

        // Get the symbol for the invoked method
        SymbolInfo symbolInfo = context.SemanticModel.GetSymbolInfo(invocation);

        if (symbolInfo.Symbol is not IMethodSymbol methodSymbol)
        {
            return;
        }

        // Get the containing type of the call site

        if (context.SemanticModel.GetEnclosingSymbol(invocation.SpanStart) is INamedTypeSymbol containingType &&
            containingType.Name == "Wpf" &&
            containingType.ContainingNamespace.ToString() == "WebClient.Interaction")
        {
            // Skip diagnostics for calls in the Wpf class within WebClient.Interaction namespace
            return;
        }

        // Check if the method is `ShowDialog` and the containing type is `RadWindow`
        if (methodSymbol.Name == "ShowDialog" &&
            methodSymbol.ContainingType.ToString() == "Telerik.Windows.Controls.RadWindow")
        {
            // Report a diagnostic
            Diagnostic diagnostic = Diagnostic.Create(Rule, invocation.GetLocation());
            context.ReportDiagnostic(diagnostic);
        }
    }
}