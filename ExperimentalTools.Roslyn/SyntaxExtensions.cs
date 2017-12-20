using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace ExperimentalTools.Roslyn
{
    public static class SyntaxExtensions
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

        public static NameSyntax ConstructNameSyntax(this string name)
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

            var doStatement = parentStatement as DoStatementSyntax;
            if (doStatement != null)
            {
                return doStatement.WithStatement(statement);
            }

            var lockStatment = parentStatement as LockStatementSyntax;
            if (lockStatment != null)
            {
                return lockStatment.WithStatement(statement);
            }

            var usingStatement = parentStatement as UsingStatementSyntax;
            if (usingStatement != null)
            {
                return usingStatement.WithStatement(statement);
            }

            var fixedStatement = parentStatement as FixedStatementSyntax;
            if (fixedStatement != null)
            {
                return fixedStatement.WithStatement(statement);
            }

            return parentStatement;
        }

        public static bool IsIdentifier(this SyntaxToken token)
        {
            return token.IsKind(SyntaxKind.IdentifierToken);
        }

        public static bool IsValidIdentifier(this string identifier)
        {
            var token = ParseToken(identifier);
            return IsIdentifier(token) && !token.ContainsDiagnostics && token.ToString().Length == identifier.Length;
        }

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

        public static BaseMethodDeclarationSyntax WithIdentifier(this BaseMethodDeclarationSyntax methodDeclaration, SyntaxToken identifier)
        {
            var method = methodDeclaration as MethodDeclarationSyntax;
            if (method != null)
            {
                return method.WithIdentifier(identifier);
            }

            var constructor = methodDeclaration as ConstructorDeclarationSyntax;
            if (constructor != null)
            {
                return constructor.WithIdentifier(identifier);
            }

            return methodDeclaration;
        }
    }
}
