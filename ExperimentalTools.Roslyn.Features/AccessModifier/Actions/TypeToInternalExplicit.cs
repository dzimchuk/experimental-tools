using Microsoft.CodeAnalysis.CSharp;

namespace ExperimentalTools.Roslyn.Features.AccessModifier.Actions
{
    internal class TypeToInternalExplicit : TypeActionProvider
    {
        public TypeToInternalExplicit() : 
            base(Resources.ToInternalExplicit, SyntaxKind.InternalKeyword)
        {
        }
    }
}
