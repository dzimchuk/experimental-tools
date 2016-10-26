using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace ExperimentalTools
{
    internal static class SyntaxExtensions
    {
        public static TypeDeclarationSyntax GetParentTypeDeclaration(this SyntaxNode node) => 
            node.Ancestors().OfType<TypeDeclarationSyntax>().FirstOrDefault();

        private static SyntaxKind[] accessModifierKinds = new[]
            {
                SyntaxKind.PublicKeyword,
                SyntaxKind.InternalKeyword,
                SyntaxKind.PrivateKeyword,
                SyntaxKind.ProtectedKeyword
            };

        public static List<SyntaxToken> FindAccessModifiers(this BaseTypeDeclarationSyntax typeDeclaration) =>
            typeDeclaration.Modifiers.Where(modifier => accessModifierKinds.Contains(modifier.Kind())).ToList();

        public static List<SyntaxToken> FindOtherModifiers(this BaseTypeDeclarationSyntax typeDeclaration) =>
            typeDeclaration.Modifiers.Where(modifier => !accessModifierKinds.Contains(modifier.Kind())).ToList();

        public static BaseTypeDeclarationSyntax WithModifiers(this BaseTypeDeclarationSyntax typeDeclaration, SyntaxTokenList modifiers)
        {
            var classDeclaration = typeDeclaration as ClassDeclarationSyntax;
            if (classDeclaration != null)
            {
                return classDeclaration.WithModifiers(modifiers);
            }

            return typeDeclaration;
        }
    }
}
