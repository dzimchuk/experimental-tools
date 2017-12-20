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

namespace ExperimentalTools.Roslyn.Features.Constructor
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(FixConstructorNameCodeFix)), Shared]
    internal class FixConstructorNameCodeFix : CodeFixProvider
    {
        private const string diagnosticId = "CS1520";
        private readonly IOptions options;

        [ImportingConstructor]
        public FixConstructorNameCodeFix(IOptions options)
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
                        
            var methodDeclaration = root.FindNode(diagnostic.Location.SourceSpan) as BaseMethodDeclarationSyntax;
            if (methodDeclaration == null)
            {
                return;
            }
            
            if (DoesMethodHaveReturnType(methodDeclaration))
            {
                return;
            }

            var typeDeclaration = methodDeclaration.Parent as TypeDeclarationSyntax;
            if (typeDeclaration == null)
            {
                return;
            }

            var model = await context.Document.GetSemanticModelAsync(context.CancellationToken);
            if (CheckIfConstructorAlreadyExists(model, methodDeclaration, typeDeclaration, context.CancellationToken))
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

        private static bool DoesMethodHaveReturnType(BaseMethodDeclarationSyntax methodDeclaration)
        {
            var method = methodDeclaration as MethodDeclarationSyntax;
            return method != null && method.ReturnType != null && !method.ReturnType.IsMissing;
        }

        private static bool CheckIfConstructorAlreadyExists(SemanticModel model, BaseMethodDeclarationSyntax methodDeclaration,
            TypeDeclarationSyntax typeDeclaration, CancellationToken cancellationToken)
        {
            var constructors = typeDeclaration.DescendantNodes().OfType<ConstructorDeclarationSyntax>().Where(ctor => ctor != methodDeclaration).ToList();
            if (!constructors.Any())
            {
                return false;
            }

            foreach (var constructor in constructors)
            {
                if (constructor.ParameterList.Parameters.Count == methodDeclaration.ParameterList.Parameters.Count)
                {
                    for (int i = 0; i < constructor.ParameterList.Parameters.Count; i++)
                    {
                        var constructorParameter = model.GetDeclaredSymbol(constructor.ParameterList.Parameters[i], cancellationToken) as IParameterSymbol;
                        var methodParameter = model.GetDeclaredSymbol(methodDeclaration.ParameterList.Parameters[i], cancellationToken) as IParameterSymbol;
                        if (constructorParameter.Type != methodParameter.Type)
                        {
                            return false;
                        }
                    }

                    return true;
                }
            }
            
            return false;
        }

        private static Task<Document> FixConstructorNameAsync(Document document,
            SyntaxNode root,
            BaseMethodDeclarationSyntax methodDeclaration, 
            string name, 
            CancellationToken c)
        {
            var constructor = methodDeclaration.WithIdentifier(Identifier(name));
            return Task.FromResult(document.WithSyntaxRoot(root.ReplaceNode(methodDeclaration, constructor)));
        }
    }
}
