using System;
using System.Composition;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Rename;
using Microsoft.CodeAnalysis.Text;
using ExperimentalTools.Localization;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace ExperimentalTools.Features.Braces
{
    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = nameof(AddBracesRefactoring)), Shared]
    internal class AddBracesRefactoring : CodeRefactoringProvider
    {
        private readonly IOptions options;

        [ImportingConstructor]
        public AddBracesRefactoring(IOptions options)
        {
            this.options = options;
        }

        public sealed override async Task ComputeRefactoringsAsync(CodeRefactoringContext context)
        {
            if (!options.IsFeatureEnabled(FeatureIdentifiers.AddInitializedFieldRefactoring))
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

            var statement = node as StatementSyntax;
            if (statement == null || statement.Parent == null)
            {
                return;
            }

            if (statement is BlockSyntax || statement.Parent is BlockSyntax)
            {
                return;
            }

            var parentStatement = statement.Parent as StatementSyntax;
            if (parentStatement == null)
            {
                return;
            }

            context.RegisterRefactoring(
                CodeAction.Create(
                    Resources.AddBraces, 
                    cancellationToken => AddBracesAsync(context.Document, root, statement, parentStatement, cancellationToken)));
        }

        private Task<Document> AddBracesAsync(Document document, SyntaxNode root, StatementSyntax statement, StatementSyntax parentStatement, CancellationToken cancellationToken)
        {
            var newParentStatement = parentStatement.WithStatement(Block(statement));
            var newRoot = root.ReplaceNode(parentStatement, newParentStatement);
            return Task.FromResult(document.WithSyntaxRoot(newRoot));
        }
    }
}