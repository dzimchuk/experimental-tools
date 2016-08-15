using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace ExperimentalTools.Tests
{
    internal class DocumentProvider
    {
        private static readonly MetadataReference[] MetadataReferences = new[]
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location)
        };

        public Document GetDocument(string sourceText)
        {
            var projectId = ProjectId.CreateNewId();
            var documentId = DocumentId.CreateNewId(projectId);

            var documentInfo = DocumentInfo.Create(documentId, "test.cs", null, SourceCodeKind.Regular,
                TextLoader.From(TextAndVersion.Create(SourceText.From(sourceText), VersionStamp.Create())));

            var projectInfo = ProjectInfo.Create(
                projectId,
                VersionStamp.Create(),
                "Test",
                "Test",
                LanguageNames.CSharp,
                null,
                null,
                new CSharpCompilationOptions(
                    OutputKind.DynamicallyLinkedLibrary,
                    false,
                    "",
                    "",
                    "Script",
                    null,
                    OptimizationLevel.Debug,
                    false,
                    true
                ),
                new CSharpParseOptions(),
                new[] { documentInfo },
                null,
                MetadataReferences
            );

            var solutionInfo = SolutionInfo.Create(SolutionId.CreateNewId(), VersionStamp.Create(), null,
                new[] { projectInfo });

            var workspace = new TestWorkspace();
            workspace.Open(solutionInfo);

            return workspace.CurrentSolution.GetProject(projectId).GetDocument(documentId);
        }
    }
}
