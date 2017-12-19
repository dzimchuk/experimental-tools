using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace ExperimentalTools.Tests.Infrastructure.Refactoring
{
    internal class CodeRefactoringContextBuilder
    {
        private const string textSpanStart = "@:";
        private const string textSpanEnd = ":@";
        private static readonly Regex textSpanExpression =
            new Regex($"{textSpanStart}(?<textSpanContent>.*){textSpanEnd}", RegexOptions.Compiled | RegexOptions.Singleline);
        
        public static CodeRefactoringContext Build(string sourceText, ICodeActionAcceptor acceptor, IEnumerable<MetadataReference> additionalReferences)
        {
            return Build(new[] { sourceText }, acceptor, additionalReferences);
        }

        public static CodeRefactoringContext Build(string[] sources, ICodeActionAcceptor acceptor, IEnumerable<MetadataReference> additionalReferences)
        {
            var normalizedSources = new[] { NormalizeSource(sources[0]) }.Concat(sources.Skip(1)).ToArray();
            var documents = DocumentProvider.GetDocuments(normalizedSources);

            var document = documents[0];
            if (additionalReferences != null && additionalReferences.Any())
            {
                var solution = document.Project.Solution.AddMetadataReferences(document.Project.Id, additionalReferences);
                document = solution.GetDocument(document.Id);
            }

            return new CodeRefactoringContext(document, GetTextSpan(sources[0]), acceptor.Accept, CancellationToken.None);
        }

        private static TextSpan GetTextSpan(string sourceText)
        {
            var match = MatchInput(sourceText);
            var textSpanContent = match.Groups["textSpanContent"].Value;
            var start = sourceText.IndexOf(match.Value);
            return new TextSpan(start, textSpanContent.Length);
        }

        private static string NormalizeSource(string sourceText)
        {
            var match = MatchInput(sourceText);
            return sourceText.Replace(match.Value, match.Groups["textSpanContent"].Value);
        }

        private static Match MatchInput(string sourceText)
        {
            var match = textSpanExpression.Match(sourceText);
            if (!match.Success)
            {
                throw new ArgumentException("Input string does not match the pattern");
            }

            return match;
        }
    }
}
