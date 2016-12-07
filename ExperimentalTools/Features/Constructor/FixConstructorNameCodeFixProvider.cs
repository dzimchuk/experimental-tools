using ExperimentalTools.Localization;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace ExperimentalTools.Features.Constructor
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(FixConstructorNameCodeFixProvider)), Shared]
    internal class FixConstructorNameCodeFixProvider : CodeFixProvider
    {
        private const string diagnosticId = "CS1520";
        private readonly IOptions options;

        [ImportingConstructor]
        public FixConstructorNameCodeFixProvider(IOptions options)
        {
            this.options = options;
        }

        public sealed override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(diagnosticId);

        public sealed override FixAllProvider GetFixAllProvider() =>
            WellKnownFixAllProviders.BatchFixer;

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            if (!options.IsFeatureEnabled(FeatureIdentifiers.FixConstructorNameCodeFix))
            {
                return;
            }

            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            var diagnostic = context.Diagnostics.FirstOrDefault(d => d.Id == diagnosticId);
            if (diagnostic == null)
            {
                return;
            }
            
            var methodDeclaration = root.FindNode(diagnostic.Location.SourceSpan) as MethodDeclarationSyntax;
            if (methodDeclaration == null || 
                (methodDeclaration.ReturnType != null && !methodDeclaration.ReturnType.IsMissing))
            {
                return;
            }

            var typeDeclaration = methodDeclaration.Parent as TypeDeclarationSyntax;
            if (typeDeclaration == null)
            {
                return;
            }

            if (typeDeclaration.Identifier.ToString().Equals(methodDeclaration.Identifier.ToString()))
            {
                return;
            }

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: Resources.FixConstructorName,
                    createChangedDocument: c => FixConstructorNameAsync(context.Document, root, methodDeclaration, typeDeclaration.Identifier.ToString(), c),
                    equivalenceKey: Resources.FixConstructorName),
                diagnostic);
        }

        private Task<Document> FixConstructorNameAsync(Document document,
            SyntaxNode root,
            MethodDeclarationSyntax methodDeclaration, 
            string name, 
            CancellationToken c)
        {
            var constructor = methodDeclaration.WithIdentifier(Identifier(name));
            return Task.FromResult(document.WithSyntaxRoot(root.ReplaceNode(methodDeclaration, constructor)));
        }
    }
}
