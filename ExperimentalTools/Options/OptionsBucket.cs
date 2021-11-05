using System;
using System.Collections.Generic;

namespace ExperimentalTools.Options
{
    internal class OptionsBucket
    {
        private static Lazy<OptionsBucket> instance = new Lazy<OptionsBucket>(true);
        public static OptionsBucket Instance => instance.Value;

        public const int DefaultMajorVersionNumber = 17;
        public static readonly Version DefaultVersion = new Version(DefaultMajorVersionNumber, 0);

        public Dictionary<string, FeatureState> Features { get; private set; }

        public OptionsBucket()
        {
            Initialize(DefaultVersion); // ugly but VS kicks off analyzers even before the package gets a chance to fully initialize
        }

        public void Initialize(Version vsVersion)
        {
            Features = GetFeatureStates(vsVersion);
        }

        private Dictionary<string, FeatureState> GetFeatureStates(Version vsVersion)
        {
            return new Dictionary<string, FeatureState>()
            {
                { FeatureIdentifiers.AddConstructorParameterRefactoring, new FeatureState(vsVersion) },
                { FeatureIdentifiers.AddInitializedFieldRefactoring, new FeatureState(vsVersion, null, new Version(15, 9)) },
                { FeatureIdentifiers.AddNewConstructorWithParameterRefactoring, new FeatureState(vsVersion, null, new Version(16, 11)) },

                { FeatureIdentifiers.ChangeAccessModifierRefactoring, new FeatureState(vsVersion) },

                { FeatureIdentifiers.TypeAndDocumentNameAnalyzer, new FeatureState(vsVersion) },
                { FeatureIdentifiers.RenameTypeToMatchFileNameCodeFix, new FeatureState(vsVersion, null, new Version(14, 9)) },
                { FeatureIdentifiers.RenameFileToMatchTypeNameCodeFix, new FeatureState(vsVersion, null, new Version(14, 9)) },

                { FeatureIdentifiers.NamespaceNormalizationAnalyzer, new FeatureState(vsVersion) },
                { FeatureIdentifiers.NamespaceNormalizationCodeFix, new FeatureState(vsVersion, null, new Version(15, 9)) },

                { FeatureIdentifiers.FixConstructorNameCodeFix, new FeatureState(vsVersion) },

                { FeatureIdentifiers.LocateInSolutionExplorerCommand, new FeatureState(vsVersion) },

                { FeatureIdentifiers.ScaffoldXunitTheoryMemberData, new FeatureState(vsVersion) },
                { FeatureIdentifiers.ScaffoldXunitTheoryInlineData, new FeatureState(vsVersion) },

                { FeatureIdentifiers.GenerateGuid, new FeatureState(vsVersion) },

                { FeatureIdentifiers.AddBraces, new FeatureState(vsVersion) },
                { FeatureIdentifiers.RemoveBraces, new FeatureState(vsVersion) },

                { FeatureIdentifiers.FieldCanBeMadeReadOnly, new FeatureState(vsVersion, null, new Version(15, 6)) }
            };
        }
    }
}
