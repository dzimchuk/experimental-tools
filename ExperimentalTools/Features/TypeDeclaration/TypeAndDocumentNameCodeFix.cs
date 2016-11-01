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
using ExperimentalTools.Localization;

namespace ExperimentalTools.Features.TypeDeclaration
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(TypeAndDocumentNameCodeFix)), Shared]
    public class TypeAndDocumentNameCodeFix : CodeFixProvider
    {
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

            var suitableDocumentName = NameHelper.GetSuitableDocumentName(context.Document.Name);
            if (suitableDocumentName == null)
            {
                return;
            }

            var renameTypeTitle = string.Format(Resources.RenameAToB, typeDeclaration.Identifier.ValueText, suitableDocumentName);
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: renameTypeTitle,
                    createChangedSolution: c => RenameTypeAsync(context.Document, typeDeclaration, suitableDocumentName, c),
                    equivalenceKey: renameTypeTitle),
                diagnostic);

            var renameDocumentTitle = string.Format(Resources.RenameAToB, context.Document.Name, typeDeclaration.Identifier.ValueText);
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: renameDocumentTitle,
                    createChangedSolution: c => RenameDocumentAsync(context.Document, root, typeDeclaration.Identifier.ValueText, c),
                    equivalenceKey: renameDocumentTitle),
                diagnostic);
        }

        private Task<Solution> RenameDocumentAsync(Document document, SyntaxNode root,
            string newDocumentName, CancellationToken cancellationToken)
        {
            var project = document.Project.RemoveDocument(document.Id);
            var newDocument = project.AddDocument($"{newDocumentName}.cs", root, document.Folders);

            return Task.FromResult(newDocument.Project.Solution);
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