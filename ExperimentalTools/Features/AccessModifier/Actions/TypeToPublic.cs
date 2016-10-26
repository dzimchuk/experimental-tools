using Microsoft.CodeAnalysis.CSharp;
using ExperimentalTools.Localization;

namespace ExperimentalTools.Features.AccessModifier.Actions
{
    internal class TypeToPublic : TypeActionProvider
    {
        public TypeToPublic() : 
            base(Resources.ToPublic, SyntaxKind.PublicKeyword)
        {
        }
    }
}
