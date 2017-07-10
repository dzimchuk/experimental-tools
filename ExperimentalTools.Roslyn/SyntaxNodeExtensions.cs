using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;

namespace ExperimentalTools.Roslyn
{
    public static class SyntaxNodeExtensions
    {
        public static (FieldDeclarationSyntax FieldDeclaration, VariableDeclaratorSyntax VariableDeclarator)? ParseFieldDeclaration(this SyntaxNode node)
        {
            FieldDeclarationSyntax fieldDeclaration;
            VariableDeclaratorSyntax variableDeclarator;

            variableDeclarator = node as VariableDeclaratorSyntax;
            if (variableDeclarator != null)
            {
                fieldDeclaration = variableDeclarator.Ancestors().OfType<FieldDeclarationSyntax>().FirstOrDefault();
            }
            else
            {
                fieldDeclaration = node as FieldDeclarationSyntax;
                if (fieldDeclaration != null)
                {
                    variableDeclarator = fieldDeclaration.DescendantNodes().OfType<VariableDeclaratorSyntax>().FirstOrDefault();
                }
            }

            if (fieldDeclaration == null || variableDeclarator == null)
            {
                return null;
            }

            return (fieldDeclaration, variableDeclarator);
        }
    }
}
