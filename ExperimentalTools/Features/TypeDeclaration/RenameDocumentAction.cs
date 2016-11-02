using Microsoft.CodeAnalysis.CodeActions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.CodeAnalysis;

namespace ExperimentalTools.Features.TypeDeclaration
{
    internal class RenameDocumentAction : CodeAction
    {
        private readonly string title;
        private readonly Document document;
        private readonly SyntaxNode root;
        private readonly string newDocumentName;
        private readonly string equivalenceKey;

        public RenameDocumentAction(string title, Document document, SyntaxNode root, string newDocumentName, string equivalenceKey)
        {
            this.title = title;
            this.document = document;
            this.root = root;
            this.newDocumentName = newDocumentName;
            this.equivalenceKey = equivalenceKey;
        }

        public override string Title => title;

        public override string EquivalenceKey => equivalenceKey;

        protected override Task<IEnumerable<CodeActionOperation>> ComputeOperationsAsync(CancellationToken cancellationToken)
        {
            var project = document.Project.RemoveDocument(document.Id);
            var newDocument = project.AddDocument(newDocumentName, root, document.Folders);

            return Task.FromResult(new CodeActionOperation[]
                   {
                       new ApplyChangesOperation(newDocument.Project.Solution),
                       new OpenDocumentOperation(newDocument.Id, activateIfAlreadyOpen: true)
                   }.AsEnumerable());
        }
    }
}
