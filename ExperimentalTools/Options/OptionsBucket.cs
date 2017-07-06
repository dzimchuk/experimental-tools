using System;
using System.Collections.Generic;

namespace ExperimentalTools.Options
{
    internal class OptionsBucket
    {
        private static Lazy<OptionsBucket> instance = new Lazy<OptionsBucket>(true);
        public static OptionsBucket Instance => instance.Value;

        public Dictionary<string, bool> Features { get; } = new Dictionary<string, bool>()
        {
            { FeatureIdentifiers.AddConstructorParameterRefactoring, true },
            { FeatureIdentifiers.AddInitializedFieldRefactoring, true },
            { FeatureIdentifiers.AddNewConstructorWithParameterRefactoring, true },

            { FeatureIdentifiers.ChangeAccessModifierRefactoring, true },

            { FeatureIdentifiers.TypeAndDocumentNameAnalyzer, true },
            { FeatureIdentifiers.RenameTypeToMatchFileNameCodeFix, false },
            { FeatureIdentifiers.RenameFileToMatchTypeNameCodeFix, false },

            { FeatureIdentifiers.NamespaceNormalizationAnalyzer, true },
            { FeatureIdentifiers.NamespaceNormalizationCodeFix, true },

            { FeatureIdentifiers.FixConstructorNameCodeFix, true },

            { FeatureIdentifiers.LocateInSolutionExplorerCommand, true },

            { FeatureIdentifiers.ScaffoldXunitTheoryMemberData, true },
            { FeatureIdentifiers.ScaffoldXunitTheoryInlineData, true },

            { FeatureIdentifiers.GenerateGuid, true },

            { FeatureIdentifiers.AddBraces, true },
            { FeatureIdentifiers.RemoveBraces, true },

            {FeatureIdentifiers.FieldCanBeMadeReadOnlyAnalyzer, true },
            {FeatureIdentifiers.FieldCanBeMadeReadOnlyCodeFix, true }
        };
    }
}
