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

            var structDeclaration = typeDeclaration as StructDeclarationSyntax;
            if (structDeclaration != null)
            {
                return structDeclaration.WithModifiers(modifiers);
            }

            var interfaceDeclaration = typeDeclaration as InterfaceDeclarationSyntax;
            if (interfaceDeclaration != null)
            {
                return interfaceDeclaration.WithModifiers(modifiers);
            }

            var enumDeclaration = typeDeclaration as EnumDeclarationSyntax;
            if (enumDeclaration != null)
            {
                return enumDeclaration.WithModifiers(modifiers);
            }

            return typeDeclaration;
        }

        public static bool IsTopLevel(this BaseTypeDeclarationSyntax typeDeclaration) =>
            !typeDeclaration.Ancestors().OfType<BaseTypeDeclarationSyntax>().Any();

        public static bool IsTopLevel(this NamespaceDeclarationSyntax namespaceDeclaration) =>
            !namespaceDeclaration.Ancestors().OfType<NamespaceDeclarationSyntax>().Any();
    }
}
