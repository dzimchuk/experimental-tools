using System.Composition;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ExperimentalTools.Roslyn.Features.Namespace
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(NamespaceNormalizationCodeFix)), Shared]
    internal class NamespaceNormalizationCodeFix : CodeFixProvider
    {
        private readonly IOptions options;

        [ImportingConstructor]
        public NamespaceNormalizationCodeFix(IOptions options)
        {
            this.options = options;
        }

        public sealed override ImmutableArray<string> FixableDiagnosticIds => 
            ImmutableArray.Create(DiagnosticCodes.NamespaceNormalizationAnalyzer);

        public sealed override FixAllProvider GetFixAllProvider() => 
            WellKnownFixAllProviders.BatchFixer;

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            if (!options.IsFeatureEnabled(FeatureIdentifiers.NamespaceNormalizationCodeFix))
            {
                return;
            }

            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            var node = root.FindNode(diagnosticSpan);
            if (node == null)
            {
                return;
            }

            var namespaceDeclaration = node.AncestorsAndSelf().OfType<NamespaceDeclarationSyntax>().FirstOrDefault();
            if (namespaceDeclaration == null)
            {
                return;
            }

            var desiredName = diagnostic.Properties["DesiredName"];
            var title = string.Format(Resources.ChangeTo, desiredName);
            context.RegisterCodeFix(
                CodeAction.Create(
                    title,
                    cancellationToken => RenameAsync(context.Document, root, namespaceDeclaration, desiredName, cancellationToken),
                    equivalenceKey: title), 
                diagnostic);
        }

        private static Task<Document> RenameAsync(Document document, SyntaxNode root, NamespaceDeclarationSyntax namespaceDesclaration, string newName, CancellationToken cancellationToken)
        {
            var newNamespace = namespaceDesclaration.WithName(newName.ConstructNameSyntax());
            var newRoot = root.ReplaceNode(namespaceDesclaration, newNamespace);
            return Task.FromResult(document.WithSyntaxRoot(newRoot));
        }
    }
}