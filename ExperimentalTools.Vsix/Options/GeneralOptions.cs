﻿using Microsoft.VisualStudio.Shell;
using System.ComponentModel;

namespace ExperimentalTools.Vsix.Options
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
        [Description("Rename a top level type to match its file name.")]
        [TypeConverter(typeof(EnabledDisabledConverter))]
        public bool RenameTypeToMatchFileNameCodeFix { get; set; }

        [Category(category)]
        [DisplayName("Rename file to match type name")]
        [Description("Rename a file to match the name of the top level type declared in it.")]
        [TypeConverter(typeof(EnabledDisabledConverter))]
        public bool RenameFileToMatchTypeNameCodeFix { get; set; }

        [Category(category)]
        [DisplayName("Warn if namespace does not match file path")]
        [Description("Analyze if a top level namespace does not match the path of the file where it is declared and display a warning. Requires VS restart.")]
        [TypeConverter(typeof(EnabledDisabledConverter))]
        public bool NamespaceNormalizationAnalyzer { get; set; }

        public GeneralOptions()
        {
            var features = OptionsBucket.Instance.Features;

            AddConstructorParameterRefactoring = features[FeatureIdentifiers.AddConstructorParameterRefactoring];
            AddInitializedFieldRefactoring = features[FeatureIdentifiers.AddInitializedFieldRefactoring];
            AddNewConstructorWithParameterRefactoring = features[FeatureIdentifiers.AddNewConstructorWithParameterRefactoring];

            ChangeAccessModifierRefactoring = features[FeatureIdentifiers.ChangeAccessModifierRefactoring];

            TypeAndDocumentNameAnalyzer = features[FeatureIdentifiers.TypeAndDocumentNameAnalyzer];
            RenameFileToMatchTypeNameCodeFix = features[FeatureIdentifiers.RenameFileToMatchTypeNameCodeFix];
            RenameTypeToMatchFileNameCodeFix = features[FeatureIdentifiers.RenameTypeToMatchFileNameCodeFix];

            NamespaceNormalizationAnalyzer = features[FeatureIdentifiers.NamespaceNormalizationAnalyzer];
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

            features[FeatureIdentifiers.AddConstructorParameterRefactoring] = AddConstructorParameterRefactoring;
            features[FeatureIdentifiers.AddInitializedFieldRefactoring] = AddInitializedFieldRefactoring;
            features[FeatureIdentifiers.AddNewConstructorWithParameterRefactoring] = AddNewConstructorWithParameterRefactoring;

            features[FeatureIdentifiers.ChangeAccessModifierRefactoring] = ChangeAccessModifierRefactoring;

            features[FeatureIdentifiers.TypeAndDocumentNameAnalyzer] = TypeAndDocumentNameAnalyzer;
            features[FeatureIdentifiers.RenameFileToMatchTypeNameCodeFix] = RenameFileToMatchTypeNameCodeFix;
            features[FeatureIdentifiers.RenameTypeToMatchFileNameCodeFix] = RenameTypeToMatchFileNameCodeFix;

            features[FeatureIdentifiers.NamespaceNormalizationAnalyzer] = NamespaceNormalizationAnalyzer;
        }
    }
}