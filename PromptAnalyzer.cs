using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace PenmansAnalyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class PromptAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "PARWP001";

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId,
        "Use custom Prompt methods",
        "Replace calls to Telerik's RadWindow.Prompt with RadWindowHelper.Prompt, PromptSync, or PromptAsync",
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

    private void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
    {
        InvocationExpressionSyntax invocation = (InvocationExpressionSyntax)context.Node;

        // Get the symbol for the invoked method
        SymbolInfo symbolInfo = context.SemanticModel.GetSymbolInfo(invocation);

        if (symbolInfo.Symbol is not IMethodSymbol methodSymbol)
        {
            return;
        }

        // Check if the method being called is `RadWindow.Prompt`
        if (methodSymbol.ContainingType.ToString() == "Telerik.Windows.Controls.RadWindow" &&
            methodSymbol.Name == "Prompt")
        {
            // Allow calls from within `RadWindowHelper` implementations
            if (context.ContainingSymbol?.ContainingType.Name == "RadWindowHelper")
            {
                return;
            }

            // Report a diagnostic for any other direct calls to RadWindow.Prompt
            Diagnostic diagnostic = Diagnostic.Create(Rule, invocation.Expression.GetLocation());
            context.ReportDiagnostic(diagnostic);
        }
    }
}