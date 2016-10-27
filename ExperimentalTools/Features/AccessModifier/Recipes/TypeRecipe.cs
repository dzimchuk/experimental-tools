using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ExperimentalTools.Features.AccessModifier.Recipes
{
    internal abstract class TypeRecipe : ITypeRecipe
    {
        private readonly IEnumerable<ITypeActionProvider> actionProviders;

        public TypeRecipe(IEnumerable<ITypeActionProvider> actionProviders)
        {
            this.actionProviders = actionProviders;
        }

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

        public abstract bool CanHandle(BaseTypeDeclarationSyntax typeDeclaration);
    }
}
