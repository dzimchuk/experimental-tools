using ExperimentalTools.Options;
using Microsoft.VisualStudio.Shell;
using System.ComponentModel;

namespace ExperimentalTools.Vsix.Features.Options
{
    internal class GeneralOptions : DialogPage
    {
        private const string category = "Feature";

        [Category(category)]
        [DisplayName("Initialize field in constructor")]
        [Description("Initialize field in existing constructor(s).")]
        [TypeConverter(typeof(EnabledDisabledConverter))]
        public bool AddConstructorParameterRefactoring { get; set; }

        [Category(category)]
        [DisplayName("Add initialized field")]
        [Description("Declare a new field and initialize it from the constructor parameter.")]
        [TypeConverter(typeof(EnabledDisabledConverter))]
        public bool AddInitializedFieldRefactoring { get; set; }

        [Category(category)]
        [DisplayName("Add constructor and initialize field")]
        [Description("Add new constructor and initialize field.")]
        [TypeConverter(typeof(EnabledDisabledConverter))]
        public bool AddNewConstructorWithParameterRefactoring { get; set; }

        [Category(category)]
        [DisplayName("Fix constructor name")]
        [Description("Provide a code fix for CS1520 that changes the incorrect constructor name to the one that matches the type name.")]
        [TypeConverter(typeof(EnabledDisabledConverter))]
        public bool FixConstructorNameCodeFix { get; set; }

        [Category(category)]
        [DisplayName("Change access modifier")]
        [Description("Change access modifier of a type declaration.")]
        [TypeConverter(typeof(EnabledDisabledConverter))]
        public bool ChangeAccessModifierRefactoring { get; set; }

        [Category(category)]
        [DisplayName("Warn if type name does not match file name")]
        [Description("Analyze if a top level type name does not match the name of the file where it is declared and display a warning. Requires VS restart.")]
        [TypeConverter(typeof(EnabledDisabledConverter))]
        public bool TypeAndDocumentNameAnalyzer { get; set; }

        [Category(category)]
        [DisplayName("Rename type to match file name")]
        [Description("Rename a top level type to match its file name. VS2015 only.")]
        [TypeConverter(typeof(EnabledDisabledConverter))]
        [Browsable(false)]
        public bool RenameTypeToMatchFileNameCodeFix { get; set; }

        [Category(category)]
        [DisplayName("Rename file to match type name")]
        [Description("Rename a file to match the name of the top level type declared in it. VS2015 only.")]
        [TypeConverter(typeof(EnabledDisabledConverter))]
        [Browsable(false)]
        public bool RenameFileToMatchTypeNameCodeFix { get; set; }

        [Category(category)]
        [DisplayName("Warn if namespace does not match file path")]
        [Description("Analyze if a top level namespace does not match the path of the file where it is declared or does not start with the project's default namespace. Requires VS restart.")]
        [TypeConverter(typeof(EnabledDisabledConverter))]
        public bool NamespaceNormalizationAnalyzer { get; set; }

        [Category(category)]
        [DisplayName("Change namespace to match file path")]
        [Description("Change namespace to match file path and default namespace.")]
        [TypeConverter(typeof(EnabledDisabledConverter))]
        public bool NamespaceNormalizationCodeFix { get; set; }

        [Category(category)]
        [DisplayName("Locate in Solution Explorer")]
        [Description("Provide a context menu command and the key shortcut (Shit+Alt+L) to locate the currently open document in Solution Explorer. It's essentially the same command as 'Sync with Active Document'.")]
        [TypeConverter(typeof(EnabledDisabledConverter))]
        public bool LocateInSolutionExplorerCommand { get; set; }

        [Category(category)]
        [DisplayName("Scaffold xunit theory member data")]
        [Description("Add a sample member data property and wire it up to the xunit theory test method.")]
        [TypeConverter(typeof(EnabledDisabledConverter))]
        public bool ScaffoldXunitTheoryMemberDataRefactoring { get; set; }

        [Category(category)]
        [DisplayName("Scaffold xunit theory inline data")]
        [Description("Add sample inline parameters to the xunit theory test method.")]
        [TypeConverter(typeof(EnabledDisabledConverter))]
        public bool ScaffoldXunitTheoryInlineDataRefactoring { get; set; }

        [Category(category)]
        [DisplayName("Generate GUID")]
        [Description("Generate a new GUID with by typing in nguid and pressing TAB.")]
        [TypeConverter(typeof(EnabledDisabledConverter))]
        public bool GenerateGuid { get; set; }

        [Category(category)]
        [DisplayName("Add braces around single line statements")]
        [Description("Offer to add braces around single line statements.")]
        [TypeConverter(typeof(EnabledDisabledConverter))]
        public bool AddBraces { get; set; }

        [Category(category)]
        [DisplayName("Remove braces from single line statements")]
        [Description("Offer to braces from single line statements.")]
        [TypeConverter(typeof(EnabledDisabledConverter))]
        public bool RemoveBraces { get; set; }

        [Category(category)]
        [DisplayName("Make field readonly")]
        [Description("Make field readonly. Up to VS2017 version 15.6.")]
        [TypeConverter(typeof(EnabledDisabledConverter))]
        public bool FieldCanBeMadeReadOnly { get; set; }
        
        public GeneralOptions()
        {
            var features = OptionsBucket.Instance.Features;

            AddConstructorParameterRefactoring = features[FeatureIdentifiers.AddConstructorParameterRefactoring].Enabled;
            AddInitializedFieldRefactoring = features[FeatureIdentifiers.AddInitializedFieldRefactoring].Enabled;
            AddNewConstructorWithParameterRefactoring = features[FeatureIdentifiers.AddNewConstructorWithParameterRefactoring].Enabled;

            ChangeAccessModifierRefactoring = features[FeatureIdentifiers.ChangeAccessModifierRefactoring].Enabled;

            TypeAndDocumentNameAnalyzer = features[FeatureIdentifiers.TypeAndDocumentNameAnalyzer].Enabled;
            RenameFileToMatchTypeNameCodeFix = features[FeatureIdentifiers.RenameFileToMatchTypeNameCodeFix].Enabled;
            RenameTypeToMatchFileNameCodeFix = features[FeatureIdentifiers.RenameTypeToMatchFileNameCodeFix].Enabled;

            NamespaceNormalizationAnalyzer = features[FeatureIdentifiers.NamespaceNormalizationAnalyzer].Enabled;
            NamespaceNormalizationCodeFix = features[FeatureIdentifiers.NamespaceNormalizationCodeFix].Enabled;

            FixConstructorNameCodeFix = features[FeatureIdentifiers.FixConstructorNameCodeFix].Enabled;

            LocateInSolutionExplorerCommand = features[FeatureIdentifiers.LocateInSolutionExplorerCommand].Enabled;

            ScaffoldXunitTheoryMemberDataRefactoring = features[FeatureIdentifiers.ScaffoldXunitTheoryMemberData].Enabled;
            ScaffoldXunitTheoryInlineDataRefactoring = features[FeatureIdentifiers.ScaffoldXunitTheoryInlineData].Enabled;

            GenerateGuid = features[FeatureIdentifiers.GenerateGuid].Enabled;

            AddBraces = features[FeatureIdentifiers.AddBraces].Enabled;
            RemoveBraces = features[FeatureIdentifiers.RemoveBraces].Enabled;

            FieldCanBeMadeReadOnly = features[FeatureIdentifiers.FieldCanBeMadeReadOnly].Enabled;
        }

        protected override void OnApply(PageApplyEventArgs e)
        {
            if (e.ApplyBehavior == ApplyKind.Apply)
            {
                UpdateFeatureStates();
            }

            base.OnApply(e);
        }

        public void UpdateFeatureStates()
        {
            var features = OptionsBucket.Instance.Features;

            features[FeatureIdentifiers.AddConstructorParameterRefactoring].Enabled = AddConstructorParameterRefactoring;
            features[FeatureIdentifiers.AddInitializedFieldRefactoring].Enabled = AddInitializedFieldRefactoring;
            features[FeatureIdentifiers.AddNewConstructorWithParameterRefactoring].Enabled = AddNewConstructorWithParameterRefactoring;

            features[FeatureIdentifiers.ChangeAccessModifierRefactoring].Enabled = ChangeAccessModifierRefactoring;

            features[FeatureIdentifiers.TypeAndDocumentNameAnalyzer].Enabled = TypeAndDocumentNameAnalyzer;
            features[FeatureIdentifiers.RenameFileToMatchTypeNameCodeFix].Enabled = RenameFileToMatchTypeNameCodeFix;
            features[FeatureIdentifiers.RenameTypeToMatchFileNameCodeFix].Enabled = RenameTypeToMatchFileNameCodeFix;

            features[FeatureIdentifiers.NamespaceNormalizationAnalyzer].Enabled = NamespaceNormalizationAnalyzer;
            features[FeatureIdentifiers.NamespaceNormalizationCodeFix].Enabled = NamespaceNormalizationCodeFix;

            features[FeatureIdentifiers.FixConstructorNameCodeFix].Enabled = FixConstructorNameCodeFix;

            features[FeatureIdentifiers.LocateInSolutionExplorerCommand].Enabled = LocateInSolutionExplorerCommand;

            features[FeatureIdentifiers.ScaffoldXunitTheoryMemberData].Enabled = ScaffoldXunitTheoryMemberDataRefactoring;
            features[FeatureIdentifiers.ScaffoldXunitTheoryInlineData].Enabled = ScaffoldXunitTheoryInlineDataRefactoring;

            features[FeatureIdentifiers.GenerateGuid].Enabled = GenerateGuid;

            features[FeatureIdentifiers.AddBraces].Enabled = AddBraces;
            features[FeatureIdentifiers.RemoveBraces].Enabled = RemoveBraces;

            features[FeatureIdentifiers.FieldCanBeMadeReadOnly].Enabled = FieldCanBeMadeReadOnly;
        }
    }
}
