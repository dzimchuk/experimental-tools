using System.Composition;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using Microsoft.CodeAnalysis.CodeActions;
using System.Threading;
using System.Collections.Generic;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static ExperimentalTools.Roslyn.SyntaxFactory;

namespace ExperimentalTools.Roslyn.Features.Constructor
{
    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = nameof(AddConstructorParameterRefactoring)), Shared]
    internal class AddConstructorParameterRefactoring : CodeRefactoringProvider
    {
        private readonly SimpleNameGenerator nameGenerator = new SimpleNameGenerator();
        private readonly IOptions options;

        [ImportingConstructor]
        public AddConstructorParameterRefactoring(IOptions options)
        {
            this.options = options;
        }

        public sealed override async Task ComputeRefactoringsAsync(CodeRefactoringContext context)
        {
            if (!options.IsFeatureEnabled(FeatureIdentifiers.AddConstructorParameterRefactoring))
            {
                return;
            }

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

            var constructors = typeDeclaration.DescendantNodes().OfType<ConstructorDeclarationSyntax>().ToList();
            if (!constructors.Any())
            {
                return;
            }

            var model = await context.Document.GetSemanticModelAsync(context.CancellationToken);
            if (model.IsConstant(variableDeclarator, context.CancellationToken))
            {
                return;
            }

            constructors = FilterConstructors(model, variableDeclarator, typeDeclaration,
                constructors, context.CancellationToken);
            if (!constructors.Any())
            {
                return;
            }

            var actionTitle = constructors.Count > 1
                ? Resources.InitializeFieldInExistingConstructors
                : Resources.InitializeFieldInExistingConstructor;
            var action = CodeAction.Create(actionTitle, 
                token => InitializeFieldInConstructorsAsync(context.Document, root, fieldDeclaration, 
                    constructors, token));
            context.RegisterRefactoring(action);
        }

        private static List<ConstructorDeclarationSyntax> FilterConstructors(SemanticModel model,
            VariableDeclaratorSyntax variableDeclarator,
            TypeDeclarationSyntax typeDeclaration, 
            IEnumerable<ConstructorDeclarationSyntax> constructors,
            CancellationToken cancellationToken)
        {
            var result = new List<ConstructorDeclarationSyntax>();
            var fieldSymbol = model.GetDeclaredSymbol(variableDeclarator, cancellationToken);
            
            foreach (var constructor in constructors)
            {
                var constructorSymbol = model.GetDeclaredSymbol(constructor, cancellationToken);
                if (constructorSymbol != null && constructorSymbol.IsStatic)
                {
                    continue;
                }

                var skip = false;
                var expressions = constructor.DescendantNodes().OfType<ExpressionSyntax>().ToList();
                foreach (var expression in expressions)
                {
                    var symbol = model.GetSymbolInfo(expression, cancellationToken).Symbol;
                    if (symbol != null && symbol == fieldSymbol && expression.IsWrittenTo())
                    {
                        skip = true;
                        break;
                    }
                }

                if (!skip)
                {
                    result.Add(constructor);
                }
            }

            return result;
        }

        private async Task<Document> InitializeFieldInConstructorsAsync(Document document, SyntaxNode root,
            FieldDeclarationSyntax fieldDeclaration, IEnumerable<ConstructorDeclarationSyntax> constructors,
            CancellationToken cancellationToken)
        {
            var variableDeclaration = fieldDeclaration.DescendantNodes().OfType<VariableDeclarationSyntax>().FirstOrDefault();
            var variableDeclarator = variableDeclaration.DescendantNodes().OfType<VariableDeclaratorSyntax>().FirstOrDefault();
            var fieldName = variableDeclarator.Identifier.ValueText;

            var model = await document.GetSemanticModelAsync(cancellationToken);
            var fieldSymbol = model.GetDeclaredSymbol(variableDeclarator, cancellationToken) as IFieldSymbol;

            var trackedRoot = root.TrackNodes(constructors);
            foreach (var constructor in constructors)
            {
                var currentConstructor = trackedRoot.GetCurrentNode(constructor);
                var newConstructor = currentConstructor;

                string parameterName;
                var existingParameter = FindExistingParameter(model, fieldSymbol, constructor, cancellationToken);
                if (existingParameter != null)
                {
                    parameterName = existingParameter.Identifier.ValueText;
                }
                else
                {
                    parameterName = nameGenerator.GetNewParameterName(currentConstructor.ParameterList, fieldName);
                    newConstructor = currentConstructor.WithParameterList(currentConstructor.ParameterList.AddParameters(
                    Parameter(Identifier(parameterName))
                        .WithType(variableDeclaration.Type)));
                }

                var assignment = newConstructor.ParameterList.Parameters.Any(p => p.Identifier.ValueText == fieldName)
                    ? CreateThisAssignmentStatement(fieldName, parameterName)
                    : CreateAssignmentStatement(fieldName, parameterName);
                newConstructor = newConstructor.WithBody(newConstructor.Body.AddStatements(assignment));

                trackedRoot = trackedRoot.ReplaceNode(currentConstructor, newConstructor);
            }

            return document.WithSyntaxRoot(trackedRoot);
        }

        private static ParameterSyntax FindExistingParameter(SemanticModel model, IFieldSymbol fieldSymbol,
            ConstructorDeclarationSyntax constructor, CancellationToken cancellationToken)
        {
            foreach (var parameter in constructor.ParameterList.Parameters)
            {
                var parameterSymbol = model.GetDeclaredSymbol(parameter, cancellationToken) as IParameterSymbol;
                if (fieldSymbol != null && parameterSymbol != null && fieldSymbol.Type == parameterSymbol.Type)
                {
                    var assignments = constructor.DescendantNodes().OfType<AssignmentExpressionSyntax>().ToList();
                    foreach (var assignment in assignments)
                    {
                        var rightSymbol = model.GetSymbolInfo(assignment.Right, cancellationToken).Symbol;
                        if (rightSymbol != null && rightSymbol == parameterSymbol)
                        {
                            return null;
                        }
                    }

                    return parameter;
                }
            }

            return null;
        }
    }
}