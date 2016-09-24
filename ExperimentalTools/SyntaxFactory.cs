using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace ExperimentalTools
{
    internal static class SyntaxFactory
    {
        public static ExpressionStatementSyntax CreateThisAssignmentStatement(string leftIdentifier, string rightIdentifier) => 
            ExpressionStatement(
                    AssignmentExpression(
                        SyntaxKind.SimpleAssignmentExpression,
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            ThisExpression(),
                            IdentifierName(leftIdentifier)),
                        IdentifierName(rightIdentifier)));

        public static ExpressionStatementSyntax CreateAssignmentStatement(string leftIdentifier, string rightIdentifier) => 
            ExpressionStatement(
                    AssignmentExpression(
                        SyntaxKind.SimpleAssignmentExpression,
                        IdentifierName(leftIdentifier),
                        IdentifierName(rightIdentifier)));

        public static ExpressionStatementSyntax CreateDefaultAssignmentStatement(string leftIdentifier, TypeSyntax type) =>
            ExpressionStatement(
                    AssignmentExpression(
                        SyntaxKind.SimpleAssignmentExpression,
                        IdentifierName(leftIdentifier),
                        DefaultExpression(type)));

        public static FieldDeclarationSyntax CreateFieldDeclaration(TypeSyntax fieldType, string fieldName) => 
            FieldDeclaration(
                VariableDeclaration(fieldType)
                    .WithVariables(
                        SingletonSeparatedList(
                            VariableDeclarator(
                                Identifier(fieldName)))))
                .WithModifiers(
                    TokenList(
                        new[]{
                              Token(SyntaxKind.PrivateKeyword),
                              Token(SyntaxKind.ReadOnlyKeyword)}));

        public static ConstructorDeclarationSyntax CreateEmptyConstructor(string name) =>
            ConstructorDeclaration(Identifier(name)).WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword))).WithBody(Block());
    }
}
