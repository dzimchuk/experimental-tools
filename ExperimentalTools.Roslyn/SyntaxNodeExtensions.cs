using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ExperimentalTools.Roslyn
{
    /// <summary>
    /// taken from https://github.com/dotnet/roslyn/blob/master/src/Workspaces/CSharp/Portable/Extensions/SyntaxNodeExtensions.cs
    /// </summary>
    public static class SyntaxNodeExtensions
    {
        public static bool IsParentKind(this SyntaxNode node, SyntaxKind kind)
        {
            return node != null && node.Parent.IsKind(kind);
        }

        public static bool IsAnyAssignExpression(this SyntaxNode node)
            => SyntaxFacts.IsAssignmentExpression(node.Kind());

        public static bool IsLeftSideOfAssignExpression(this SyntaxNode node)
        {
            return node.IsParentKind(SyntaxKind.SimpleAssignmentExpression) &&
                ((AssignmentExpressionSyntax)node.Parent).Left == node;
        }

        public static bool IsLeftSideOfAnyAssignExpression(this SyntaxNode node)
        {
            return node != null &&
                node.Parent.IsAnyAssignExpression() &&
                ((AssignmentExpressionSyntax)node.Parent).Left == node;
        }

        public static bool IsRightSideOfAnyAssignExpression(this SyntaxNode node)
        {
            return node.Parent.IsAnyAssignExpression() &&
                ((AssignmentExpressionSyntax)node.Parent).Right == node;
        }
    }
}
