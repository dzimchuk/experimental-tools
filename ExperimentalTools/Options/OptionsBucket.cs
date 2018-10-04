using System;
using System.Collections.Generic;

namespace ExperimentalTools.Options
{
    internal class OptionsBucket
    {
        private static Lazy<OptionsBucket> instance = new Lazy<OptionsBucket>(true);
        public static OptionsBucket Instance => instance.Value;

        public Dictionary<string, FeatureState> Features { get; private set; }

        public void Initialize(Version vsVersion)
        {
            Features = new Dictionary<string, FeatureState>()
            {
                { FeatureIdentifiers.AddConstructorParameterRefactoring, new FeatureState(vsVersion) },
                { FeatureIdentifiers.AddInitializedFieldRefactoring, new FeatureState(vsVersion) },
                { FeatureIdentifiers.AddNewConstructorWithParameterRefactoring, new FeatureState(vsVersion) },

                { FeatureIdentifiers.ChangeAccessModifierRefactoring, new FeatureState(vsVersion) },

                { FeatureIdentifiers.TypeAndDocumentNameAnalyzer, new FeatureState(vsVersion) },
                { FeatureIdentifiers.RenameTypeToMatchFileNameCodeFix, new FeatureState(vsVersion, null, new Version(14, 9)) },
                { FeatureIdentifiers.RenameFileToMatchTypeNameCodeFix, new FeatureState(vsVersion, null, new Version(14, 9)) },

                { FeatureIdentifiers.NamespaceNormalizationAnalyzer, new FeatureState(vsVersion) },
                { FeatureIdentifiers.NamespaceNormalizationCodeFix, new FeatureState(vsVersion) },

                { FeatureIdentifiers.FixConstructorNameCodeFix, new FeatureState(vsVersion) },

                { FeatureIdentifiers.LocateInSolutionExplorerCommand, new FeatureState(vsVersion) },

                { FeatureIdentifiers.ScaffoldXunitTheoryMemberData, new FeatureState(vsVersion) },
                { FeatureIdentifiers.ScaffoldXunitTheoryInlineData, new FeatureState(vsVersion) },

                { FeatureIdentifiers.GenerateGuid, new FeatureState(vsVersion) },

                { FeatureIdentifiers.AddBraces, new FeatureState(vsVersion) },
                { FeatureIdentifiers.RemoveBraces, new FeatureState(vsVersion) },

                {FeatureIdentifiers.FieldCanBeMadeReadOnly, new FeatureState(vsVersion, null, new Version(15, 6)) }
            };
        }
    }
}
