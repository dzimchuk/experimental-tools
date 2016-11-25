using System;
using System.Collections.Generic;

namespace ExperimentalTools
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
            { FeatureIdentifiers.RenameTypeToMatchFileNameCodeFix, true },
            { FeatureIdentifiers.RenameFileToMatchTypeNameCodeFix, true },

            { FeatureIdentifiers.NamespaceNormalizationAnalyzer, true }
        };
    }
}
