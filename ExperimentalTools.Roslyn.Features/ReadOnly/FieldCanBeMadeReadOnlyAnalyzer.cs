using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.FindSymbols;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace ExperimentalTools.Roslyn.Features.ReadOnly
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class FieldCanBeMadeReadOnlyAnalyzer : DiagnosticAnalyzer
    {
        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.FieldCanBeMadeReadOnlyAnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.FieldCanBeMadeReadOnlyAnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticCodes.FieldCanBeMadeReadOnlyAnalyzer, Title, MessageFormat, Resources.CategoryAccess, GetSeverity(), true);

        private static DiagnosticSeverity GetSeverity() =>
            ServiceLocator.GetService<IOptions>().IsFeatureEnabled(FeatureIdentifiers.FieldCanBeMadeReadOnlyAnalyzer) ? DiagnosticSeverity.Info : DiagnosticSeverity.Hidden;

        private readonly GeneratedCodeRecognitionService generatedCodeRecognitionService = new GeneratedCodeRecognitionService();

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.Field);
            //context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.FieldDeclaration);
        }

        private void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            if (generatedCodeRecognitionService.IsGeneratedCode(context))
            {
                return;
            }

            var fieldDeclaration = (FieldDeclarationSyntax)context.Node;
            var variableDeclarator = fieldDeclaration.DescendantNodes().OfType<VariableDeclaratorSyntax>().FirstOrDefault();
            if (variableDeclarator == null)
            {
                return;
            }

            var fieldSymbol = context.ContainingSymbol as IFieldSymbol;
            if (fieldSymbol == null)
            {
                return;
            }

            if (fieldSymbol.IsConst || fieldSymbol.IsReadOnly || fieldSymbol.IsVolatile)
            {
                return;
            }

            var result = context.SemanticModel.AnalyzeControlFlow(variableDeclarator);
            

            context.ReportDiagnostic(Diagnostic.Create(Rule, fieldSymbol.Locations[0], fieldSymbol.Name));
        }

        private void AnalyzeSymbol(SymbolAnalysisContext context)
        {
            if (generatedCodeRecognitionService.IsGeneratedCode(context))
            {
                return;
            }

            var fieldSymbol = (IFieldSymbol)context.Symbol;
            if (fieldSymbol.IsConst || fieldSymbol.IsReadOnly || fieldSymbol.IsVolatile)
            {
                return;
            }

            

            context.ReportDiagnostic(Diagnostic.Create(Rule, fieldSymbol.Locations[0], fieldSymbol.Name));
        }
    }
}
