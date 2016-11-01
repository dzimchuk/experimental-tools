using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using ExperimentalTools.Localization;
using System.Text.RegularExpressions;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ExperimentalTools.Features.TypeDeclaration
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class TypeAndDocumentNameAnalyzer : DiagnosticAnalyzer
    {
        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.TypeAndDocumentNameAnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.TypeAndDocumentNameAnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        
        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticCodes.TypeAndDocumentNameAnalyzer, Title, MessageFormat, Resources.CategoryNaming, DiagnosticSeverity.Warning, true);

        private readonly IGeneratedCodeRecognitionService generatedCodeRecognitionService = ServiceLocator.GetService<IGeneratedCodeRecognitionService>();
        private readonly Regex documentNameExpression = new Regex(@"^(?<name>.+)\.cs$", RegexOptions.IgnoreCase);
        
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSymbolAction(AnalyzeNode, SymbolKind.NamedType);
        }

        private void AnalyzeNode(SymbolAnalysisContext context)
        {
            if (generatedCodeRecognitionService.IsGeneratedCode(context))
            {
                return;
            }

            var symbol = (INamedTypeSymbol)context.Symbol;
            if (symbol.DeclaringSyntaxReferences.Count() > 1)
            {
                return;
            }

            if (symbol.ContainingType != null)
            {
                return;
            }

            if (!IsTheOnlyType(symbol, context.CancellationToken))
            {
                return;
            }

            var documentName = symbol.DeclaringSyntaxReferences[0].SyntaxTree.FilePath;
            var suitableDocumentName = NameHelper.GetSuitableDocumentName(documentName);
            if (suitableDocumentName != null)
            {
                if (!suitableDocumentName.Equals(symbol.Name, StringComparison.OrdinalIgnoreCase))
                {
                    context.ReportDiagnostic(Diagnostic.Create(Rule, symbol.Locations[0], symbol.Name));
                }
            }
        }

        private static bool IsTheOnlyType(INamedTypeSymbol symbol, CancellationToken cancellationToken)
        {
            var root = symbol.DeclaringSyntaxReferences[0].SyntaxTree.GetRoot(cancellationToken);
            var topLevelTypes = root.DescendantNodes().OfType<BaseTypeDeclarationSyntax>().Where(t => t.IsTopLevelType());
            return topLevelTypes.Count() == 1;
        }
    }
}