using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Composition;
using ExperimentalTools.Localization;

namespace ExperimentalTools.Features.TypeDeclaration
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class TypeAndDocumentNameAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "ET_TypeAndDocumentNameAnalyzer";
        public const string NoFixDiagnosticId = "ET_TypeAndDocumentNameAnalyzer_NoFix";

        internal static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.TypeAndDocumentNameAnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        internal static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.TypeAndDocumentNameAnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Resources.CategoryNaming, DiagnosticSeverity.Info, true);
        internal static DiagnosticDescriptor NoFixRule = new DiagnosticDescriptor(NoFixDiagnosticId, Title, MessageFormat, Resources.CategoryNaming, DiagnosticSeverity.Info, true);

        private readonly IGeneratedCodeRecognitionService generatedCodeRecognitionService;

        [ImportingConstructor]
        public TypeAndDocumentNameAnalyzer(IGeneratedCodeRecognitionService generatedCodeRecognitionService)
        {
            this.generatedCodeRecognitionService = generatedCodeRecognitionService;
        }

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.ClassDeclaration, SyntaxKind.StructDeclaration, SyntaxKind.InterfaceDeclaration, SyntaxKind.EnumDeclaration);
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
        }

        private void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            if (generatedCodeRecognitionService.IsGeneratedCode(context))
            {
                return;
            }

            var typeDeclaration = (BaseTypeDeclarationSyntax)context.Node;
            if (!typeDeclaration.IsTopLevelType())
            {
                return;
            }
        }
    }
}