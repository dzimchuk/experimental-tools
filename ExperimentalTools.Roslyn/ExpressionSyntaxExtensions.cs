using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ExperimentalTools.Roslyn
{
    /// <summary>
    /// taken from https://github.com/dotnet/roslyn/blob/master/src/Workspaces/CSharp/Portable/Extensions/ExpressionSyntaxExtensions.cs
    /// </summary>
    public static class ExpressionSyntaxExtensions
    {
        public static bool IsInOutContext(this ExpressionSyntax expression)
        {
            var argument = expression.Parent as ArgumentSyntax;
            return
                argument != null &&
                argument.Expression == expression &&
                argument.RefOrOutKeyword.Kind() == SyntaxKind.OutKeyword;
        }

        public static bool IsInRefContext(this ExpressionSyntax expression)
            => (expression?.Parent as ArgumentSyntax)?.RefOrOutKeyword.Kind() == SyntaxKind.RefKeyword;

        public static bool IsOnlyWrittenTo(this ExpressionSyntax expression)
        {
            if (expression.IsRightSideOfDotOrArrow())
            {
                expression = expression.Parent as ExpressionSyntax;
            }

            if (expression != null)
            {
                if (expression.IsInOutContext())
                {
                    return true;
                }

                if (expression.Parent != null)
                {
                    if (expression.IsLeftSideOfAssignExpression())
                    {
                        return true;
                    }

                    if (expression.IsAttributeNamedArgumentIdentifier())
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static bool IsAttributeNamedArgumentIdentifier(this ExpressionSyntax expression)
        {
            var nameEquals = expression?.Parent as NameEqualsSyntax;
            return nameEquals.IsParentKind(SyntaxKind.AttributeArgument);
        }

        public static bool IsWrittenTo(this ExpressionSyntax expression)
        {
            if (expression.IsOnlyWrittenTo())
            {
                return true;
            }

            if (expression.IsRightSideOfDotOrArrow())
            {
                expression = expression.Parent as ExpressionSyntax;
            }

            if (expression.IsInRefContext())
            {
                return true;
            }

            // We're written if we're used in a ++, or -- expression.
            if (expression.IsOperandOfIncrementOrDecrementExpression())
            {
                return true;
            }

            if (expression.IsLeftSideOfAnyAssignExpression())
            {
                return true;
            }

            return false;
        }

        public static bool IsOperandOfIncrementOrDecrementExpression(this ExpressionSyntax expression)
        {
            if (expression != null)
            {
                switch (expression.Parent.Kind())
                {
                    case SyntaxKind.PostIncrementExpression:
                    case SyntaxKind.PreIncrementExpression:
                    case SyntaxKind.PostDecrementExpression:
                    case SyntaxKind.PreDecrementExpression:
                        return true;
                }
            }

            return false;
        }

        public static bool IsRightSideOfDotOrArrow(this ExpressionSyntax name)
        {
            return IsAnyMemberAccessExpressionName(name) || IsRightSideOfQualifiedName(name);
        }

        public static bool IsAnyMemberAccessExpressionName(this ExpressionSyntax expression)
        {
            if (expression == null)
            {
                return false;
            }

            return expression == (expression.Parent as MemberAccessExpressionSyntax)?.Name ||
                expression.IsMemberBindingExpressionName();
        }

        private static bool IsMemberBindingExpressionName(this ExpressionSyntax expression)
        {
            return expression.IsParentKind(SyntaxKind.MemberBindingExpression) &&
                ((MemberBindingExpressionSyntax)expression.Parent).Name == expression;
        }

        public static bool IsRightSideOfQualifiedName(this ExpressionSyntax expression)
        {
            return expression.IsParentKind(SyntaxKind.QualifiedName) && ((QualifiedNameSyntax)expression.Parent).Right == expression;
        }
    }
}
