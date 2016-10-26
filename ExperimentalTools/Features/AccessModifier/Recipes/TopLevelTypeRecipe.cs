using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ExperimentalTools.Features.AccessModifier.Actions;
using System.Composition;

namespace ExperimentalTools.Features.AccessModifier.Recipes
{
    [Export(typeof(ITypeRecipe))]
    internal class TopLevelTypeRecipe : ITypeRecipe
    {
        private readonly IEnumerable<ITypeActionProvider> actionProviders = new List<ITypeActionProvider>
        {
            new TypeToInternalExplicit(),
            new TypeToInternalImplicit(),
            new TypeToPublic()
        };

        public IEnumerable<CodeAction> Apply(Document document, SyntaxNode root, BaseTypeDeclarationSyntax typeDeclaration)
        {
            var actions = new List<CodeAction>();
            foreach (var provider in actionProviders)
            {
                var action = provider.CalculateAction(document, root, typeDeclaration);
                if (action != null)
                {
                    actions.Add(action);
                }
            }

            return actions;
        }
    }
}
