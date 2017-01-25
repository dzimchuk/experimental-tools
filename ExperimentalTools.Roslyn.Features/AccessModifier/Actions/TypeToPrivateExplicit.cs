using Microsoft.CodeAnalysis.CSharp;

namespace ExperimentalTools.Roslyn.Features.AccessModifier.Actions
{
    internal class TypeToPrivateExplicit : TypeActionProvider
    {
        public TypeToPrivateExplicit() : 
            base(Resources.ToPrivateExplicit, SyntaxKind.PrivateKeyword)
        {
        }
    }
}
