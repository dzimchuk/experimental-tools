using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Text.RegularExpressions;
using System.Threading;

namespace ExperimentalTools.Tests.Infrastructure
{
    internal class CodeRefactoringContextBuilder
    {
        private const string textSpanStart = "@:";
        private const string textSpanEnd = ":@";
        private static readonly Regex textSpanExpression =
            new Regex($"{textSpanStart}(?<textSpanContent>.*){textSpanEnd}", RegexOptions.Compiled | RegexOptions.Singleline);
        
        public static CodeRefactoringContext Build(string sourceText, ICodeActionAcceptor acceptor)
        {
            var document = DocumentProvider.GetDocument(NormalizeSource(sourceText));
            return new CodeRefactoringContext(document, GetTextSpan(sourceText), acceptor.Accept, CancellationToken.None);
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
