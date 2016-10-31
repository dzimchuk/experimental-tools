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

namespace ExperimentalTools.Features.TypeDeclaration
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class TypeAndDocumentNameAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "TypeAndDocumentNameAnalyzer";
        internal static readonly LocalizableString Title = "TypeAndDocumentNameAnalyzer Title";
        internal static readonly LocalizableString MessageFormat = "TypeAndDocumentNameAnalyzer '{0}'";
        internal const string Category = "TypeAndDocumentNameAnalyzer Category";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Info, true);

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