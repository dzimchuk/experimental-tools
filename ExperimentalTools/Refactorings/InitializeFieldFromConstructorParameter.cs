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
using Microsoft.CodeAnalysis.Formatting;

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
            
            if (await CheckIfAlreadyInitializedAsync(context.Document, parameter, constructor, context.CancellationToken))
            {
                return;
            }

            var action = CodeAction.Create("Initialize field", 
                cancellationToken => InitializeFieldAsync(context.Document, root, parameter, constructor, cancellationToken));
            context.RegisterRefactoring(action);
        }

        private static async Task<bool> CheckIfAlreadyInitializedAsync(Document document, ParameterSyntax parameter,
            ConstructorDeclarationSyntax constructor, CancellationToken cancellationToken)
        {
            var parameterName = parameter.Identifier.ValueText;
            var assignments = (constructor.DescendantNodes().OfType<AssignmentExpressionSyntax>()
                .Where(exp => exp.Right.DescendantTokens().Any(token => token.ValueText == parameterName))).ToList();

            if (!assignments.Any())
            {
                return false;
            }

            var model = await document.GetSemanticModelAsync(cancellationToken);
            var currentType = model.GetDeclaredSymbol(constructor.Parent) as INamedTypeSymbol;

            return assignments.Any(assignment =>
            {
                var targetType = model.GetSymbolInfo(assignment.Left).Symbol?.ContainingType;
                return currentType == targetType;
            });
        }

        private Task<Document> InitializeFieldAsync(Document document, SyntaxNode root, 
            ParameterSyntax parameter, ConstructorDeclarationSyntax constructor, 
            CancellationToken cancellationToken)
        {
            var field = CreateFieldDeclaration(parameter);
            var assignment = CreateAssignmentStatement(parameter);

            var trackedRoot = root.TrackNodes(constructor);
            var newRoot = trackedRoot.InsertNodesBefore(trackedRoot.GetCurrentNode(constructor), SingletonList(field));

            var constructorBody = newRoot.GetCurrentNode(constructor).Body;
            var newConstructorBody = constructorBody.AddStatements(assignment)
                .WithAdditionalAnnotations(Formatter.Annotation);

            newRoot = newRoot.ReplaceNode(constructorBody, newConstructorBody);

            return Task.FromResult(document.WithSyntaxRoot(newRoot));
        }

        private static ExpressionStatementSyntax CreateAssignmentStatement(ParameterSyntax parameter)
        {
            return ExpressionStatement(
                                AssignmentExpression(
                                    SyntaxKind.SimpleAssignmentExpression,
                                    MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        ThisExpression(),
                                        IdentifierName(parameter.Identifier.ValueText)),
                                    IdentifierName(parameter.Identifier.ValueText)));
        }

        private static FieldDeclarationSyntax CreateFieldDeclaration(ParameterSyntax parameter)
        {
            return FieldDeclaration(
                            VariableDeclaration(parameter.Type)
                            .WithVariables(
                                SingletonSeparatedList<VariableDeclaratorSyntax>(
                                    VariableDeclarator(
                                        Identifier(parameter.Identifier.ValueText)))))
                        .WithModifiers(
                            TokenList(
                                new[]{
                                    Token(SyntaxKind.PrivateKeyword),
                                    Token(SyntaxKind.ReadOnlyKeyword)}));
        }
    }
}