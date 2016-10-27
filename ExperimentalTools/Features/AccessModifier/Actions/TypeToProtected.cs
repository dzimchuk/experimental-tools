using ExperimentalTools.Localization;
using Microsoft.CodeAnalysis.CSharp;

namespace ExperimentalTools.Features.AccessModifier.Actions
{
    internal class TypeToProtected : TypeActionProvider
    {
        public TypeToProtected() : 
            base(Resources.ToProtected, SyntaxKind.ProtectedKeyword)
        {
        }
    }
}
