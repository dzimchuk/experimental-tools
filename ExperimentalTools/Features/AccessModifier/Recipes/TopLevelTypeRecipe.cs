using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ExperimentalTools.Features.AccessModifier.Actions;
using System.Composition;

namespace ExperimentalTools.Features.AccessModifier.Recipes
{
    [Export(typeof(ITypeRecipe))]
    internal class TopLevelTypeRecipe : TypeRecipe
    {
        private static readonly IEnumerable<ITypeActionProvider> actionProviders = new List<ITypeActionProvider>
        {
            new TypeToInternalExplicit(),
            new TypeToInternalImplicit(),
            new TypeToPublic()
        };

        public TopLevelTypeRecipe() : base(actionProviders)
        {
        }

        public override bool CanHandle(BaseTypeDeclarationSyntax typeDeclaration) =>
            typeDeclaration.IsTopLevel();
    }
}
