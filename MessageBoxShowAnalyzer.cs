using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace PenmansAnalyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MessageBoxShowAnalyzer : DiagnosticAnalyzer
{
    private const string DIAGNOSTIC_ID = "PAMB001";

    private static readonly DiagnosticDescriptor Rule = new (
        DIAGNOSTIC_ID,
        "Avoid using MessageBox.Show",
        "Avoid using '{0}.Show'. Use RadWindowHelper.Alert for more consistent user notifications.",
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

        // Check if the method is `Show` and belongs to `MessageBox` or `MsgBox`
        if ((methodSymbol.Name == "Show" && methodSymbol.ContainingType.ToString() == "System.Windows.MessageBox")
            || (methodSymbol.Name == "MsgBox"))
        {
            // Create a diagnostic and report it
            Diagnostic diagnostic = Diagnostic.Create(
                Rule,
                invocation.GetLocation(),
                methodSymbol.ContainingType.Name); // Pass the containing type (e.g., MessageBox or MsgBox) as an argument
            context.ReportDiagnostic(diagnostic);
        }
    }
}