using System.Composition;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Rename;

namespace ExperimentalTools.Roslyn.Features.TypeDeclaration
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(TypeAndDocumentNameCodeFixProvider)), Shared]
    internal class TypeAndDocumentNameCodeFixProvider : CodeFixProvider
    {
        private readonly IOptions options;

        [ImportingConstructor]
        public TypeAndDocumentNameCodeFixProvider(IOptions options)
        {
            this.options = options;
        }

        public sealed override ImmutableArray<string> FixableDiagnosticIds => 
            ImmutableArray.Create(DiagnosticCodes.TypeAndDocumentNameAnalyzer);

        public sealed override FixAllProvider GetFixAllProvider() => 
            WellKnownFixAllProviders.BatchFixer;

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            var typeDeclaration = root.FindNode(diagnosticSpan) as TypeDeclarationSyntax;
            if (typeDeclaration == null)
            {
                return;
            }

            CalculateRenameTypeAction(context, diagnostic, typeDeclaration);
            CalculateRenameDocumentAction(context, root, diagnostic, typeDeclaration);
        }

        private void CalculateRenameDocumentAction(CodeFixContext context, SyntaxNode root, Diagnostic diagnostic, TypeDeclarationSyntax typeDeclaration)
        {
            if (!options.IsFeatureEnabled(FeatureIdentifiers.RenameFileToMatchTypeNameCodeFix))
            {
                return;
            }

            var newDocumentName = NameHelper.AddExtension(typeDeclaration.Identifier.ValueText);
            var renameDocumentTitle = string.Format(Resources.RenameAToB, context.Document.Name, newDocumentName);
            context.RegisterCodeFix(
                new RenameDocumentAction(renameDocumentTitle, context.Document, root, newDocumentName, renameDocumentTitle),
                diagnostic);
        }

        private void CalculateRenameTypeAction(CodeFixContext context, Diagnostic diagnostic, TypeDeclarationSyntax typeDeclaration)
        {
            if (!options.IsFeatureEnabled(FeatureIdentifiers.RenameTypeToMatchFileNameCodeFix))
            {
                return;
            }

            var documentName = NameHelper.RemoveExtension(context.Document.Name);
            if (documentName == null)
            {
                return;
            }

            if (!documentName.IsValidIdentifier())
            {
                return;
            }

            var renameTypeTitle = string.Format(Resources.RenameAToB, typeDeclaration.Identifier.ValueText, documentName);
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: renameTypeTitle,
                    createChangedSolution: c => RenameTypeAsync(context.Document, typeDeclaration, documentName, c),
                    equivalenceKey: renameTypeTitle),
                diagnostic);
        }

        private async Task<Solution> RenameTypeAsync(Document document, TypeDeclarationSyntax typeDeclaration,
            string suitableDocumentName, CancellationToken cancellationToken)
        {
            var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
            var typeSymbol = semanticModel.GetDeclaredSymbol(typeDeclaration, cancellationToken);
            
            var originalSolution = document.Project.Solution;
            var optionSet = originalSolution.Workspace.Options;
            var newSolution = await Renamer.RenameSymbolAsync(document.Project.Solution, typeSymbol, suitableDocumentName, optionSet, cancellationToken).ConfigureAwait(false);
            
            return newSolution;
        }
    }
}