using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Rename;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(NoUnderscoreCodeFixProvider)), Shared]
public class NoUnderscoreCodeFixProvider : CodeFixProvider
{
    public sealed override ImmutableArray<string> FixableDiagnosticIds
        => ImmutableArray.Create(NoUnderscoreAnalyzer.ConstDiagnosticId);

    public sealed override FixAllProvider GetFixAllProvider()
        => WellKnownFixAllProviders.BatchFixer;

    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var diagnostic = context.Diagnostics[0];
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        var token = root.FindToken(diagnostic.Location.SourceSpan.Start);

        var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);
        var symbol = semanticModel.GetDeclaredSymbol(token.Parent, context.CancellationToken);

        if (symbol != null)
        {
            // 計算移除中間底線後的新名稱
            var name = symbol.Name;
            string newName = name;
            if (name.Length > 2)
            {
                var middle = name.Substring(1, name.Length - 2).Replace("_", "");
                newName = name[0] + middle + name[name.Length - 1];
            }
            if (name.StartsWith("s_") && name.Length > 3)
            {
                var afterPrefix = name.Substring(2).Replace("_", "");
                newName = "s_" + afterPrefix;
            }

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: $"修正名稱違規: {newName}",
                    createChangedSolution: c => RenameSymbolAsync(context.Document, symbol, c),
                    equivalenceKey: "修正名稱違規"),
                diagnostic);
        }
    }

    private async Task<Solution> RenameSymbolAsync(Document document, ISymbol symbol, CancellationToken cancellationToken)
    {
        var name = symbol.Name;
        var solution = document.Project.Solution;

        if (name.StartsWith("s_") && name.Length > 3)
        {
            var afterPrefix = name.Substring(2).Replace("_", "");
            var newName = "s_" + afterPrefix;

            return await Renamer.RenameSymbolAsync(
                solution,
                symbol,
                new SymbolRenameOptions(),
                newName,
                cancellationToken
            ).ConfigureAwait(false);
        }

        if (name.Length > 2)
        {
            var middle = name.Substring(1, name.Length - 2).Replace("_", "");
            var newName = name[0] + middle + name[name.Length - 1];

            return await Renamer.RenameSymbolAsync(
                solution,
                symbol,
                new SymbolRenameOptions(),
                newName,
                cancellationToken
            ).ConfigureAwait(false);
        }

        return solution;
    }

}
