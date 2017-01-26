using ExperimentalTools.Roslyn.Features.AccessModifier.Actions;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Composition;

namespace ExperimentalTools.Roslyn.Features.AccessModifier.Recipes
{
    [Export(typeof(ITypeRecipe))]
    internal class NestedInStructRecipe : TypeRecipe
    {
        private static readonly IEnumerable<ITypeActionProvider> actionProviders = new List<ITypeActionProvider>
        {
            new TypeToPrivateExplicit(),
            new TypeToPrivateImplicit(),
            new TypeToPublic(),
            new TypeToInternal()
        };

        public NestedInStructRecipe() : base(actionProviders)
        {
        }

        public override bool CanHandle(BaseTypeDeclarationSyntax typeDeclaration) =>
            typeDeclaration.Parent is StructDeclarationSyntax;
    }
}
