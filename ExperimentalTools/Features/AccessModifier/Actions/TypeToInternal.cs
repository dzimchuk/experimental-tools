using ExperimentalTools.Localization;
using Microsoft.CodeAnalysis.CSharp;

namespace ExperimentalTools.Features.AccessModifier.Actions
{
    internal class TypeToInternal : TypeActionProvider
    {
        public TypeToInternal() : 
            base(Resources.ToInternal, SyntaxKind.InternalKeyword)
        {
        }
    }
}
