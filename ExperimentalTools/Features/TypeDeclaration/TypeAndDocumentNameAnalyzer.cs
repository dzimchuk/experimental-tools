using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using ExperimentalTools.Localization;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.IO;

namespace ExperimentalTools.Features.TypeDeclaration
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class TypeAndDocumentNameAnalyzer : DiagnosticAnalyzer
    {
        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.TypeAndDocumentNameAnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.TypeAndDocumentNameAnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticCodes.TypeAndDocumentNameAnalyzer, Title, MessageFormat, Resources.CategoryNaming, GetSeverity(), true);

        private static DiagnosticSeverity GetSeverity() => 
            ServiceLocator.GetService<IOptions>().IsFeatureEnabled(FeatureIdentifiers.TypeAndDocumentNameAnalyzer) ? DiagnosticSeverity.Warning : DiagnosticSeverity.Hidden;

        private readonly IGeneratedCodeRecognitionService generatedCodeRecognitionService = ServiceLocator.GetService<IGeneratedCodeRecognitionService>();

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.NamedType);
        }

        private void AnalyzeSymbol(SymbolAnalysisContext context)
        {
            if (generatedCodeRecognitionService.IsGeneratedCode(context))
            {
                return;
            }

            var symbol = (INamedTypeSymbol)context.Symbol;
            if (symbol.DeclaringSyntaxReferences.Count() != 1)
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
            
            var documentPath = symbol.DeclaringSyntaxReferences[0].SyntaxTree.FilePath;
            if (documentPath == null)
            {
                return;
            }

            var documentName = NameHelper.RemoveExtension(Path.GetFileName(documentPath));
            if (documentName != null)
            {
                if (!documentName.Equals(symbol.Name, StringComparison.OrdinalIgnoreCase))
                {
                    context.ReportDiagnostic(Diagnostic.Create(Rule, symbol.Locations[0], symbol.Name));
                }
            }
        }

        private static bool IsTheOnlyType(INamedTypeSymbol symbol, CancellationToken cancellationToken)
        {
            var root = symbol.DeclaringSyntaxReferences[0].SyntaxTree.GetRoot(cancellationToken);
            var topLevelTypes = root.DescendantNodes().OfType<BaseTypeDeclarationSyntax>().Where(t => t.IsTopLevel());
            return topLevelTypes.Count() == 1;
        }
    }
}