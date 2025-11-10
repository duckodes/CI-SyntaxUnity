using System.Collections.Immutable;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class NoUnderscoreAnalyzer : DiagnosticAnalyzer
{
    public const string ConstDiagnosticId = "IDE99999";
    private static readonly LocalizableString s_title = "名稱不能包含底線";
    private static readonly LocalizableString s_messageFormat = "符號 '{0}' 包含底線 '_'";
    private static readonly LocalizableString s_description = "命名規則禁止使用底線";
    private const string _constCategory = "Naming";

    private static DiagnosticDescriptor s_rule = new DiagnosticDescriptor(
        ConstDiagnosticId, s_title, s_messageFormat, _constCategory,
        DiagnosticSeverity.Error, isEnabledByDefault: true, description: s_description,
        helpLinkUri: "https://notes.duckode.com/?user=IDE&category=IDE99999&categoryID=0");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        => ImmutableArray.Create(s_rule);

    public override void Initialize(AnalysisContext context)
    {
        // 註冊符號分析
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.NamedType, SymbolKind.Method, SymbolKind.Field, SymbolKind.Property, SymbolKind.Parameter);

        // 語法分析：區域變數宣告
        context.RegisterSyntaxNodeAction(AnalyzeLocalVariable, Microsoft.CodeAnalysis.CSharp.SyntaxKind.VariableDeclarator);
    }

    private static void AnalyzeSymbol(SymbolAnalysisContext context)
    {
        var symbol = context.Symbol;
        var name = symbol.Name;

        // 檢查是否有底線，但排除開頭和結尾
        if (name.Length > 2 && name.Substring(1, name.Length - 2).Contains("_"))
        {
            var diagnostic = Diagnostic.Create(s_rule, symbol.Locations[0], symbol.Name);
            context.ReportDiagnostic(diagnostic);
        }
    }
    private static void AnalyzeLocalVariable(SyntaxNodeAnalysisContext context)
    {
        var variableDeclarator = (Microsoft.CodeAnalysis.CSharp.Syntax.VariableDeclaratorSyntax)context.Node;
        var name = variableDeclarator.Identifier.Text;

        // 檢查中間是否有底線
        if (name.Length > 2 && name.Substring(1, name.Length - 2).Contains("_"))
        {
            var diagnostic = Diagnostic.Create(s_rule, variableDeclarator.Identifier.GetLocation(), name);
            context.ReportDiagnostic(diagnostic);
        }
    }
}
