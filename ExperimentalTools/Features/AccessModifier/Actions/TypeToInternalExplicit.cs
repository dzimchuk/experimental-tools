using Microsoft.CodeAnalysis.CSharp;
using ExperimentalTools.Localization;

namespace ExperimentalTools.Features.AccessModifier.Actions
{
    internal class TypeToInternalExplicit : TypeActionProvider
    {
        public TypeToInternalExplicit() : 
            base(Resources.ToInternalExplicit, SyntaxKind.InternalKeyword)
        {
        }
    }
}
