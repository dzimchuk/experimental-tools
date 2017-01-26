using Microsoft.CodeAnalysis.CSharp;

namespace ExperimentalTools.Roslyn.Features.AccessModifier.Actions
{
    internal class TypeToPublic : TypeActionProvider
    {
        public TypeToPublic() : 
            base(Resources.ToPublic, SyntaxKind.PublicKeyword)
        {
        }
    }
}
