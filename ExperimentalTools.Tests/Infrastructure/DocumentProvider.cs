using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Linq;

namespace ExperimentalTools.Tests.Infrastructure
{
    internal static class DocumentProvider
    {
        private static readonly MetadataReference CorlibReference = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
        private static readonly MetadataReference SystemCoreReference = MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location);
        private static readonly MetadataReference CSharpSymbolsReference = MetadataReference.CreateFromFile(typeof(CSharpCompilation).Assembly.Location);
        private static readonly MetadataReference CodeAnalysisReference = MetadataReference.CreateFromFile(typeof(Compilation).Assembly.Location);

        private const string DefaultFilePathPrefix = "Test";
        private const string CSharpDefaultFileExt = "cs";
        private const string TestProjectName = "TestProject";

        public static Document[] GetDocuments(string[] sources) => 
            GetDocuments(sources, null);

        public static Document[] GetDocuments(string[] sources, string[] fileNames)
        {
            var project = CreateProject(sources, fileNames);
            var documents = project.Documents.ToArray();

            if (sources.Length != documents.Length)
            {
                throw new Exception("Amount of sources did not match amount of Documents created");
            }

            return documents;
        }

        public static Document GetDocument(string source) => 
            CreateProject(new[] { source }, null).Documents.First();

        public static Document GetDocument(string source, string fileName) =>
            CreateProject(new[] { source }, new[] { fileName }).Documents.First();

        private static Project CreateProject(string[] sources, string[] fileNames)
        {
            if (fileNames != null && sources.Length != fileNames.Length)
            {
                throw new ArgumentException("Number of specified file names does not match the number of sources");
            }

            var projectId = ProjectId.CreateNewId(debugName: TestProjectName);

            var solution = new AdhocWorkspace()
                .CurrentSolution
                .AddProject(projectId, TestProjectName, TestProjectName, LanguageNames.CSharp)
                .AddMetadataReference(projectId, CorlibReference)
                .AddMetadataReference(projectId, SystemCoreReference)
                .AddMetadataReference(projectId, CSharpSymbolsReference)
                .AddMetadataReference(projectId, CodeAnalysisReference);

            var count = 0;
            foreach (var source in sources)
            {
                var newFileName = fileNames != null
                    ? fileNames[count]
                    : DefaultFilePathPrefix + count + "." + CSharpDefaultFileExt;

                var documentId = DocumentId.CreateNewId(projectId, debugName: newFileName);
                solution = solution.AddDocument(documentId, newFileName, SourceText.From(source));
                count++;
            }

            return solution.GetProject(projectId);
        }
    }
}
