using ExperimentalTools.Localization;
using Microsoft.CodeAnalysis.CSharp;

namespace ExperimentalTools.Features.AccessModifier.Actions
{
    internal class TypeToPrivateExplicit : TypeActionProvider
    {
        public TypeToPrivateExplicit() : 
            base(Resources.ToPrivateExplicit, SyntaxKind.PrivateKeyword)
        {
        }
    }
}
