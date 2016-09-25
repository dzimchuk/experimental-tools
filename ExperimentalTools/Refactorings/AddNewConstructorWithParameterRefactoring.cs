using System.Composition;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static ExperimentalTools.SyntaxFactory;
using ExperimentalTools.Localization;
using System.Threading;
using System.Collections.Generic;

namespace ExperimentalTools.Refactorings
{
    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = nameof(AddNewConstructorWithParameterRefactoring)), Shared]
    internal class AddNewConstructorWithParameterRefactoring : CodeRefactoringProvider
    {
        public sealed override async Task ComputeRefactoringsAsync(CodeRefactoringContext context)
        {
            if (context.Document.Project.Solution.Workspace.Kind == WorkspaceKind.MiscellaneousFiles)
            {
                return;
            }

            if (!context.Span.IsEmpty)
            {
                return;
            }

            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            var node = root.FindNode(context.Span);

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
                return;
            }

            var typeDeclaration = fieldDeclaration.GetParentTypeDeclaration();
            if (typeDeclaration == null)
            {
                return;
            }

            if (await CheckIfAlreadyInitializedAsync(context.Document, variableDeclarator, typeDeclaration, context.CancellationToken))
            {
                return;
            }

            if (await CheckIfConstructorWithSingleParameterExistsAsync(context.Document, variableDeclarator, typeDeclaration, context.CancellationToken))
            {
                return;
            }

            context.RegisterRefactoring(
                CodeAction.Create(Resources.AddConstructorAndInitializeField, token => AddConstructorAsync(context.Document, root, fieldDeclaration)));
        }

        private static async Task<bool> CheckIfAlreadyInitializedAsync(Document document, VariableDeclaratorSyntax variableDeclarator,
            TypeDeclarationSyntax typeDeclaration, CancellationToken cancellationToken)
        {
            var constructors = typeDeclaration.DescendantNodes().OfType<ConstructorDeclarationSyntax>().ToList();
            if (!constructors.Any())
            {
                return false;
            }

            var model = await document.GetSemanticModelAsync(cancellationToken);
            var fieldSymbol = model.GetDeclaredSymbol(variableDeclarator, cancellationToken);

            foreach (var constructor in constructors)
            {
                if (!constructor.ParameterList.Parameters.Any())
                {
                    continue;
                }

                var assignments = constructor.DescendantNodes().OfType<AssignmentExpressionSyntax>().ToList();
                foreach (var assignment in assignments)
                {
                    var leftSymbol = model.GetSymbolInfo(assignment.Left, cancellationToken).Symbol;
                    if (leftSymbol != null && leftSymbol == fieldSymbol)
                    {
                        var rightSymbol = model.GetSymbolInfo(assignment.Right, cancellationToken).Symbol;
                        foreach (var parameter in constructor.ParameterList.Parameters)
                        {
                            var parameterSymbol = model.GetDeclaredSymbol(parameter, cancellationToken);
                            if (rightSymbol == parameterSymbol)
                            {
                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }

        private static async Task<bool> CheckIfConstructorWithSingleParameterExistsAsync(Document document, VariableDeclaratorSyntax variableDeclarator,
            TypeDeclarationSyntax typeDeclaration, CancellationToken cancellationToken)
        {
            var constructors = typeDeclaration.DescendantNodes().OfType<ConstructorDeclarationSyntax>()
                .Where(constructor => constructor.ParameterList.Parameters.Count == 1).ToList();
            if (!constructors.Any())
            {
                return false;
            }

            var model = await document.GetSemanticModelAsync(cancellationToken);
            var fieldSymbol = model.GetDeclaredSymbol(variableDeclarator, cancellationToken) as IFieldSymbol;

            foreach (var constructor in constructors)
            {
                var parameterSymbol = model.GetDeclaredSymbol(constructor.ParameterList.Parameters[0], cancellationToken) as IParameterSymbol;
                if (fieldSymbol?.Type == parameterSymbol?.Type)
                {
                    return true;
                }
            }

            return false;
        }

        private static Task<Document> AddConstructorAsync(Document document, SyntaxNode root, FieldDeclarationSyntax fieldDeclaration)
        {
            var variableDeclaration = fieldDeclaration.DescendantNodes().OfType<VariableDeclarationSyntax>().FirstOrDefault();
            var variableDeclarator = variableDeclaration.DescendantNodes().OfType<VariableDeclaratorSyntax>().FirstOrDefault();
            var fieldName = variableDeclarator.Identifier.ValueText;

            var typeDeclaration = fieldDeclaration.GetParentTypeDeclaration();

            var newConstructor = CreateEmptyConstructor(typeDeclaration.Identifier.ValueText);
            newConstructor = newConstructor.WithParameterList(
                ParameterList(
                    SingletonSeparatedList(
                        Parameter(Identifier(fieldName))
                        .WithType(variableDeclaration.Type))));

            var assignment = CreateThisAssignmentStatement(fieldName, fieldName);
            newConstructor = newConstructor.WithBody(newConstructor.Body.AddStatements(assignment));
            newConstructor = InitializeStructFields(fieldDeclaration, typeDeclaration, newConstructor);

            var newRoot = root.InsertNodesAfter(FindInsertionPoint(fieldDeclaration), SingletonList(newConstructor));
            return Task.FromResult(document.WithSyntaxRoot(newRoot));
        }

        private static ConstructorDeclarationSyntax InitializeStructFields(FieldDeclarationSyntax fieldDeclaration, TypeDeclarationSyntax typeDeclaration, ConstructorDeclarationSyntax newConstructor)
        {
            if (typeDeclaration is StructDeclarationSyntax)
            {
                var assignments = new List<ExpressionStatementSyntax>();
                var otherFields = typeDeclaration.DescendantNodes().OfType<FieldDeclarationSyntax>()
                    .Where(field => field != fieldDeclaration).ToList();
                foreach (var field in otherFields)
                {
                    var variableDeclaration = field.DescendantNodes().OfType<VariableDeclarationSyntax>().FirstOrDefault();
                    var variableDeclarator = variableDeclaration?.DescendantNodes().OfType<VariableDeclaratorSyntax>().FirstOrDefault();
                    if (variableDeclarator != null)
                    {
                        assignments.Add(
                            CreateDefaultAssignmentStatement(variableDeclarator.Identifier.ValueText, variableDeclaration.Type));
                    }
                }

                if (assignments.Any())
                {
                    newConstructor = newConstructor.WithBody(newConstructor.Body.AddStatements(assignments.ToArray()));
                }
            }

            return newConstructor;
        }

        private static SyntaxNode FindInsertionPoint(FieldDeclarationSyntax fieldDeclaration)
        {
            var typeDeclaration = fieldDeclaration.GetParentTypeDeclaration();
            var existingConstructor = typeDeclaration.DescendantNodes().OfType<ConstructorDeclarationSyntax>().FirstOrDefault();

            return existingConstructor ?? FindInsertionPointBelowFieldGroup(fieldDeclaration);
        }

        private static SyntaxNode FindInsertionPointBelowFieldGroup(FieldDeclarationSyntax fieldDeclaration)
        {
            SyntaxNode insertionPoint = fieldDeclaration;
            var typeDeclaration = fieldDeclaration.GetParentTypeDeclaration();

            var tracking = true;
            foreach (var node in typeDeclaration.ChildNodes())
            {
                if (tracking)
                {
                    if (node is FieldDeclarationSyntax)
                    {
                        insertionPoint = node;
                        continue;
                    }

                    break;
                }
                else if (node == fieldDeclaration)
                {
                    tracking = true;
                }
            }

            return insertionPoint;
        }
    }
}