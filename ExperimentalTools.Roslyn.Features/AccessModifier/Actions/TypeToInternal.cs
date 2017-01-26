using Microsoft.CodeAnalysis.CSharp;

namespace ExperimentalTools.Roslyn.Features.AccessModifier.Actions
{
    internal class TypeToInternal : TypeActionProvider
    {
        public TypeToInternal() : 
            base(Resources.ToInternal, SyntaxKind.InternalKeyword)
        {
        }
    }
}
