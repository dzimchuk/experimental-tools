using Microsoft.CodeAnalysis.CSharp;

namespace ExperimentalTools.Roslyn.Features.AccessModifier.Actions
{
    internal class TypeToProtected : TypeActionProvider
    {
        public TypeToProtected() : 
            base(Resources.ToProtected, SyntaxKind.ProtectedKeyword)
        {
        }
    }
}
