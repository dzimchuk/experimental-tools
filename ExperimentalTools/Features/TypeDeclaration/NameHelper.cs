using System.Text.RegularExpressions;

namespace ExperimentalTools.Features.TypeDeclaration
{
    internal static class NameHelper
    {
        private static readonly Regex documentNameExpression = new Regex(@"^(?<name>.+)\.cs$", RegexOptions.IgnoreCase);

        public static string GetSuitableDocumentName(string documentName)
        {
            var m = documentNameExpression.Match(documentName);
            return m.Success ? m.Groups["name"].Value : null;
        }
    }
}
