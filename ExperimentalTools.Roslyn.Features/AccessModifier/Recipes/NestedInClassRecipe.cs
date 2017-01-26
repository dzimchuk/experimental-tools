using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Composition;
using ExperimentalTools.Roslyn.Features.AccessModifier.Actions;

namespace ExperimentalTools.Roslyn.Features.AccessModifier.Recipes
{
    [Export(typeof(ITypeRecipe))]
    internal class NestedInClassRecipe : TypeRecipe
    {
        private static readonly IEnumerable<ITypeActionProvider> actionProviders = new List<ITypeActionProvider>
        {
            new TypeToPrivateExplicit(),
            new TypeToPrivateImplicit(),
            new TypeToPublic(),
            new TypeToInternal(),
            new TypeToProtected(),
            new TypeToProtectedInternal()
        };

        public NestedInClassRecipe() : base(actionProviders)
        {
        }

        public override bool CanHandle(BaseTypeDeclarationSyntax typeDeclaration) => 
            typeDeclaration.Parent is ClassDeclarationSyntax;
    }
}
