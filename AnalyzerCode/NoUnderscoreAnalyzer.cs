using System;
using System.Collections.Immutable;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class NoUnderscoreAnalyzer : DiagnosticAnalyzer
{
    public const string ConstDiagnosticId = "IDE99999";
    private static readonly LocalizableString s_title = "名稱不能包含底線";
    private static readonly LocalizableString s_messageFormat = "符號 '{0}' 包含底線 '_'";
    private static readonly LocalizableString s_description = "命名規則禁止使用底線.";
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

        // 區域變數宣告分析
        context.RegisterSyntaxNodeAction(AnalyzeLocalVariable, Microsoft.CodeAnalysis.CSharp.SyntaxKind.VariableDeclarator);
        context.RegisterSyntaxNodeAction(AnalyzeForEachVariable, Microsoft.CodeAnalysis.CSharp.SyntaxKind.ForEachStatement);
    }

    private static void AnalyzeSymbol(SymbolAnalysisContext context)
    {
        var symbol = context.Symbol;

        // 去除get;set;檢查
        if (symbol is IMethodSymbol methodSymbol &&
           (methodSymbol.MethodKind == MethodKind.PropertyGet || methodSymbol.MethodKind == MethodKind.PropertySet))
        {
            return;
        }
        var name = symbol.Name;

        // 允許特定方法前墜
        if (symbol is IMethodSymbol)
        {
            string[] allowedMethodPrefixes = { "BtnClick_", "InputField_", "FadeOutWindow_" };

            foreach (var prefix in allowedMethodPrefixes)
            {
                if (name.StartsWith(prefix))
                {
                    var afterPrefix = name.Substring(prefix.Length);

                    if (afterPrefix.Length > 0 && !char.IsUpper(afterPrefix[0]))
                    {
                        var diagnostic = Diagnostic.Create(s_rule, symbol.Locations[0], symbol.Name);
                        context.ReportDiagnostic(diagnostic);
                    }

                    if (afterPrefix.Contains("_"))
                    {
                        var diagnostic = Diagnostic.Create(s_rule, symbol.Locations[0], symbol.Name);
                        context.ReportDiagnostic(diagnostic);
                    }

                    return;
                }
            }

            var lowerName = name.ToLower();
            foreach (var prefix in allowedMethodPrefixes)
            {
                var prefixWithoutUnderscore = prefix.Replace("_", "").ToLower();
                if (lowerName.Contains(prefixWithoutUnderscore) && !name.StartsWith(prefix))
                {
                    var diagnostic = Diagnostic.Create(s_rule, symbol.Locations[0], symbol.Name);
                    context.ReportDiagnostic(diagnostic);
                    return;
                }
            }
        }

        // s_ 開頭只檢查後面部分是否還有底線
        if (name.Length > 2 && name.StartsWith("s_"))
        {
            var afterPrefix = name.Substring(2);
            if (afterPrefix.Contains("_"))
            {
                var diagnostic = Diagnostic.Create(s_rule, symbol.Locations[0], symbol.Name);
                context.ReportDiagnostic(diagnostic);
            }
            return;
        }

        // 檢查是否有底線並排除開頭和結尾
        if (name.Length > 2 && name.Substring(1, name.Length - 2).Contains("_"))
        {
            var diagnostic = Diagnostic.Create(s_rule, symbol.Locations[0], symbol.Name);
            context.ReportDiagnostic(diagnostic);
        }
    }
    private static void AnalyzeLocalVariable(SyntaxNodeAnalysisContext context)
    {
        var variableDeclarator = (Microsoft.CodeAnalysis.CSharp.Syntax.VariableDeclaratorSyntax)context.Node;

        var parent = variableDeclarator.Parent;
        if (parent == null) return;
        var grandParent = parent.Parent;
        if (!(grandParent is Microsoft.CodeAnalysis.CSharp.Syntax.LocalDeclarationStatementSyntax))
        {
            return;
        }

        var name = variableDeclarator.Identifier.Text;

        if (name.StartsWith("s_"))
        {
            return;
        }

        // 檢查中間是否有底線
        if (name.Length > 2 && name.Substring(1, name.Length - 2).Contains("_"))
        {
            var diagnostic = Diagnostic.Create(s_rule, variableDeclarator.Identifier.GetLocation(), name);
            context.ReportDiagnostic(diagnostic);
        }
    }
    private static void AnalyzeForEachVariable(SyntaxNodeAnalysisContext context)
    {
        var forEachStatement = (Microsoft.CodeAnalysis.CSharp.Syntax.ForEachStatementSyntax)context.Node;
        var identifier = forEachStatement.Identifier.Text;

        // 檢查中間是否有底線並排除開頭和結尾
        if (identifier.Length > 2 && identifier.Substring(1, identifier.Length - 2).Contains("_"))
        {
            var diagnostic = Diagnostic.Create(s_rule, forEachStatement.Identifier.GetLocation(), identifier);
            context.ReportDiagnostic(diagnostic);
        }
    }

}
