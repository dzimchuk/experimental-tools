using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

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

        public static IEnumerable<TNode> GetAncestorsOrThis<TNode>(this SyntaxNode node) => node.AncestorsAndSelf().OfType<TNode>();

        public static TNode GetAncestorOrThis<TNode>(this SyntaxNode node) => node.GetAncestorsOrThis<TNode>().First();

        public static IEnumerable<UsingDirectiveSyntax> GetEnclosingUsingDirectives(this SyntaxNode node)
        {
            return node.GetAncestorOrThis<CompilationUnitSyntax>().Usings
                       .Concat(node.GetAncestorsOrThis<NamespaceDeclarationSyntax>()
                                   .Reverse()
                                   .SelectMany(n => n.Usings));
        }

        public static IEnumerable<ExternAliasDirectiveSyntax> GetEnclosingExternAliasDirectives(this SyntaxNode node)
        {
            return node.GetAncestorOrThis<CompilationUnitSyntax>().Externs
                       .Concat(node.GetAncestorsOrThis<NamespaceDeclarationSyntax>()
                                   .Reverse()
                                   .SelectMany(n => n.Externs));
        }
        
        public static SyntaxNode AddNamespaceUsing(this SyntaxNode root, CompilationUnitSyntax compilationUnit, string @namespace)
        {
            var newUsing = UsingDirective(ConstructNameSyntax(@namespace));
            var usingList = compilationUnit.Usings.Concat(new[] { newUsing }).OrderBy(@using => @using.Name.ToString()).ToArray();

            var newCompilationUnit = compilationUnit.WithUsings(List(usingList));
            return root.ReplaceNode(compilationUnit, newCompilationUnit);
        }

        private static NameSyntax ConstructNameSyntax(string name)
        {
            var parts = name.Split('.');
            return ConstructNameSyntax(parts, parts.Length - 1);
        }

        private static NameSyntax ConstructNameSyntax(string[] parts, int index)
        {
            var currentPart = parts[index];
            var namePiece = IdentifierName(currentPart);

            return index == 0
                ? (NameSyntax)namePiece
                : QualifiedName(ConstructNameSyntax(parts, index - 1), namePiece);
        }

        public static StatementSyntax WithStatement(this StatementSyntax parentStatement, StatementSyntax statement)
        {
            var ifStatement = parentStatement as IfStatementSyntax;
            if (ifStatement != null)
            {
                return ifStatement.WithStatement(statement);
            }

            var whileStatement = parentStatement as WhileStatementSyntax;
            if (whileStatement != null)
            {
                return whileStatement.WithStatement(statement);
            }

            var forStatement = parentStatement as ForStatementSyntax;
            if (forStatement != null)
            {
                return forStatement.WithStatement(statement);
            }

            var foreachStatement = parentStatement as ForEachStatementSyntax;
            if (foreachStatement != null)
            {
                return foreachStatement.WithStatement(statement);
            }

            return parentStatement;
        }
    }
}
