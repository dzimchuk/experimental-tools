using System.IO;
using System.Text;

namespace ExperimentalTools.Tests
{
    internal static class Extensions
    {
        public static string HomogenizeLineEndings(this string input)
        {
            var builder = new StringBuilder();

            using (var reader = new StringReader(input))
            {
                for (var line = reader.ReadLine(); line != null; line = reader.ReadLine())
                {
                    builder.AppendLine(line);
                }
            }

            return builder.ToString();
        }

        public static void EnableAllFeatures(this OptionsBucket bucket)
        {
            var features = bucket.Features;

            features[FeatureIdentifiers.AddConstructorParameterRefactoring] = true;
            features[FeatureIdentifiers.AddInitializedFieldRefactoring] = true;
            features[FeatureIdentifiers.AddNewConstructorWithParameterRefactoring] = true;

            features[FeatureIdentifiers.ChangeAccessModifierRefactoring] = true;

            features[FeatureIdentifiers.TypeAndDocumentNameAnalyzer] = true;
            features[FeatureIdentifiers.RenameTypeToMatchFileNameCodeFix] = true;
            features[FeatureIdentifiers.RenameFileToMatchTypeNameCodeFix] = true;
        }
    }
}
