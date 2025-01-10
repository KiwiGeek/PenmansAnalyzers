using System.Collections.Immutable;
using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Text;

namespace PenmansAnalyzers;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(PromptCodeFixProvider)), Shared]
public class PromptCodeFixProvider : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds => [PromptAnalyzer.DiagnosticId];

    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        SyntaxNode? root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        Diagnostic diagnostic = context.Diagnostics.First();
        TextSpan diagnosticSpan = diagnostic.Location.SourceSpan;
       
        // Find the node that the diagnostic is reported on
        SyntaxNode? node = root?.FindNode(diagnosticSpan);

        // Ensure it's a MemberAccessExpressionSyntax (e.g., RadWindow.Prompt)
        if (node is MemberAccessExpressionSyntax memberAccess)
        {
            context.RegisterCodeFix(
                Microsoft.CodeAnalysis.CodeActions.CodeAction.Create(
                    "Use RadWindowHelper.Prompt",
                    c => ReplaceWithCustomPromptAsync(context.Document, memberAccess, c),
                    nameof(PromptCodeFixProvider)),
                diagnostic);
        }
    }
    private static async Task<Document> ReplaceWithCustomPromptAsync(Document document, MemberAccessExpressionSyntax memberAccess, CancellationToken cancellationToken)
    {
        DocumentEditor? editor = await DocumentEditor.CreateAsync(document, cancellationToken);

        // Replace `RadWindow.Prompt` with `RadWindowHelper.Prompt`
        MemberAccessExpressionSyntax newExpression = SyntaxFactory.MemberAccessExpression(
            SyntaxKind.SimpleMemberAccessExpression,
            SyntaxFactory.IdentifierName("RadWindowHelper"),
            SyntaxFactory.IdentifierName("Prompt"));

        editor.ReplaceNode(memberAccess, newExpression);
        return editor.GetChangedDocument();
    }
}