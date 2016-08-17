using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace ExperimentalTools.Refactorings
{
    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = nameof(InitializeFieldFromConstructorParameter)), Shared]
    internal class InitializeFieldFromConstructorParameter : CodeRefactoringProvider
    {
        public sealed override async Task ComputeRefactoringsAsync(CodeRefactoringContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            var node = root.FindNode(context.Span);
            
            var parameter = node as ParameterSyntax;
            if (parameter == null)
            {
                return;
            }

            var constructor = parameter.Ancestors().OfType<ConstructorDeclarationSyntax>().FirstOrDefault();
            if (constructor == null)
            {
                return;
            }

            var action = CodeAction.Create("Initialize field", 
                cancellationToken => InitializeFieldAsync(context.Document, root, parameter, constructor, cancellationToken));
            context.RegisterRefactoring(action);
        }

        private Task<Document> InitializeFieldAsync(Document document, SyntaxNode root, 
            ParameterSyntax parameter, ConstructorDeclarationSyntax constructor, 
            CancellationToken cancellationToken)
        {
            var field = FieldDeclaration(
                            VariableDeclaration(
                                PredefinedType(
                                    Token(SyntaxKind.IntKeyword)))
                            .WithVariables(
                                SingletonSeparatedList<VariableDeclaratorSyntax>(
                                    VariableDeclarator(
                                        Identifier("index")))))
                        .WithModifiers(
                            TokenList(
                                new[]{
                                    Token(SyntaxKind.PrivateKeyword),
                                    Token(SyntaxKind.ReadOnlyKeyword)}));

            var assignment = ExpressionStatement(
                                AssignmentExpression(
                                    SyntaxKind.SimpleAssignmentExpression,
                                    MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        ThisExpression(),
                                        IdentifierName("index")),
                                    IdentifierName("index")));

            var trackedRoot = root.TrackNodes(constructor);
            var newRoot = trackedRoot.InsertNodesBefore(trackedRoot.GetCurrentNode(constructor), SingletonList(field));

            var constructorBody = newRoot.GetCurrentNode(constructor).Body;
            var newConstructorBody = constructorBody.AddStatements(assignment);

            newRoot = newRoot.ReplaceNode(constructorBody, newConstructorBody);

            return Task.FromResult(document.WithSyntaxRoot(newRoot));
        }
    }
}