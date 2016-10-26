using System.Composition;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ExperimentalTools.Features.AccessModifier
{
    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = nameof(ChangeAccessModifierRefactoring)), Shared]
    internal class ChangeAccessModifierRefactoring : CodeRefactoringProvider
    {
        private readonly ITypeRecipe topLevelTypeRecipe;

        [ImportingConstructor]
        public ChangeAccessModifierRefactoring(ITypeRecipe topLevelTypeRecipe)
        {
            this.topLevelTypeRecipe = topLevelTypeRecipe;
        }

        public sealed override async Task ComputeRefactoringsAsync(CodeRefactoringContext context)
        {
            if (context.Document.Project.Solution.Workspace.Kind == WorkspaceKind.MiscellaneousFiles)
            {
                return;
            }

            if (!context.Span.IsEmpty)
            {
                return;
            }

            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            var node = root.FindNode(context.Span);

            var actions = CalculateActions(context.Document, root, node);

            if (actions != null && actions.Any())
            {
                foreach (var action in actions)
                {
                    context.RegisterRefactoring(action);
                }
            }
        }

        private IEnumerable<CodeAction> CalculateActions(Document document, SyntaxNode root, SyntaxNode node)
        {
            var typeDeclaration = node as BaseTypeDeclarationSyntax;
            if (typeDeclaration != null && IsTopLevel(typeDeclaration))
            {
                return topLevelTypeRecipe.Apply(document, root, typeDeclaration);
            }

            return null;
        }

        private bool IsTopLevel(BaseTypeDeclarationSyntax typeDeclaration) => 
            !typeDeclaration.Ancestors().OfType<BaseTypeDeclarationSyntax>().Any();
    }
}