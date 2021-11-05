# Experimental Tools

<!-- Replace this badge with your own-->
[![Build status](https://ci.appveyor.com/api/projects/status/idvryqpirxbe39gt/branch/master?svg=true)](https://ci.appveyor.com/project/dzimchuk/experimental-tools)

<!-- Update the VS Gallery link after you upload the VSIX-->
Download this extension from the [VS Marketplace](https://marketplace.visualstudio.com/vsgallery/3c258fda-06c6-4740-b67c-a527a59c3f7b)
or get the [CI build](http://www.vsixgallery.com/extension/fe00c281-eed0-4c6e-901b-d8b845c82e35/).

---------------------------------------

A bunch of quality refactorings and code fixes that are going to improve your C# development experience in Visual Studio and remove some common pain.

See the [change log](CHANGELOG.md) for changes and road map.

### Note

As Visual Studio evolves more features get added to it and some of them start duplicating features from this extension. Duplicate features are disabled/removed from this extension to insure the best user experience possible with a given version of Visual Studio. A list of such features is provided near the end of this document.

## Features

- Add/Remove braces
- Change access modifier on type declarations
- Generate GUID (nguid)
- Initialize field in existing constructor
- Locate in Solution Explorer
- Make it a constructor (when copied from another class)
- Namespace vs file path analyzer
- Scaffold xunit data driven tests
- Settings page (ability to enable/disable individual features)
- Analyze if type name matches file name

### Add/Remove braces

Allows you to quickly add missing braces to single statements:

![Add braces](https://raw.githubusercontent.com/dzimchuk/experimental-tools/master/art/AddBraces.png)

It works both ways and allows you to remove braces if you prefer so.

### Change access modifier on type declarations

Ctrl+. on a type declaration (either top level or nested) and choose one of proposed options.

![Change access modifier on type declarations](https://raw.githubusercontent.com/dzimchuk/experimental-tools/master/art/ChangeTypeAccessModifier.png)

### Generate GUID (nguid)

Just type `nguid` where you want the new GUID to be inserted:

![Generate Guid - before](https://raw.githubusercontent.com/dzimchuk/experimental-tools/master/art/GenerateGuidBefore.png)

and press TAB:

![Generate Guid - After](https://raw.githubusercontent.com/dzimchuk/experimental-tools/master/art/GenerateGuidAfter.png)

### Initialize field in existing constructor

Ctrl+. on a field and choose *Initialize field in existing constructor*.

![Initialize field in existing constructor](https://raw.githubusercontent.com/dzimchuk/experimental-tools/master/art/InitializeFieldInExistingConstructor.png)

### Locate in Solution Explorer

There is a standard command in Solution Explorer called 'Sync with Active Document'. People coming from ReSharper will appreciate its Shift+Alt+L equivalent. To avoid conflicts with some internal VS commands ALT+L key binding was chosen by default which can be changed in VS settings if needed.

![Locate in Solution Explorer](https://raw.githubusercontent.com/dzimchuk/experimental-tools/master/art/LocateInSolutionExplorerCommand.png)

The command is available in the code editor either from the context menu or as a shortcut.

### Make it a constructor (when copied from another class)

Sometimes you copy code from another class into a new one and this quick fix allows you to update the constructor name.

![Make it a constructor](https://raw.githubusercontent.com/dzimchuk/experimental-tools/master/art/MakeItConstructorCodeFix.png)

### Namespace vs file path analyzer

Analyze if a top level namespace does not match the path of the file where it is declared or does not start with the project's default namespace.

![Namespace and file path analyzer](https://raw.githubusercontent.com/dzimchuk/experimental-tools/master/art/NamespaceNormalizationAnalyzer.png)

VS 2019 and above provide a corresponding code refactoring out of the box, hence only the analyzer part will remain enabled in this extension.

### Scaffold xunit data driven tests

If you're a fan of Xunit data driven tests this one's going to be a little time saver for you. You can scaffold `MemberData`:

![Scaffold Xunit MemberData](https://raw.githubusercontent.com/dzimchuk/experimental-tools/master/art/ScaffoldXunitMemberData.png)

As well as `InlineData`:

![Scaffold Xunit MemberData](https://raw.githubusercontent.com/dzimchuk/experimental-tools/master/art/ScaffoldXunitInlineData.png)

If your `InlineData` contains acceptable parameters they will be respected unless the test method already defines parameters (in which case neither of the scaffolding refactoring will work).

Note that this feature works with Xunit 2.x only.

### Settings page (ability to enable/disable individual features)

All features can be individually enabled or disabled.

![Type and file name analyzer](https://raw.githubusercontent.com/dzimchuk/experimental-tools/master/art/GeneralOptions.png)

### Analyze if type name matches file name

Analyzes if a top level type name matches the name of the file where it is declared and displays a warning if not.

![Type and file name analyzer](https://raw.githubusercontent.com/dzimchuk/experimental-tools/master/art/TypeAndDocumentNameAnalyzer.png)

VS 2017 and above provide code refactorings to rename types and file names to match each other out of the box. 

## Deactivated features

- Add constructor and initialize field (VS 2019 started to provide *Generate constructor ...* at some point)
- Field can be made readonly (VS 2017 version 15.7 and above provide a built-in analyzer and a code fix)
- Initialize field from constructor parameter (VS 2017 and above provide a similar refactoring out of the box)
- Namespace vs file path code fix (VS 2019 and above provide a corresponding code refactoring out of the box)
- Type name vs file name code fix (VS 2017 and above provide code refactorings to rename types and file names to match each other out of the box)

## Contribute
Check out the [contribution guidelines](CONTRIBUTING.md)
if you want to contribute to this project.

## License
[MIT](LICENSE)
