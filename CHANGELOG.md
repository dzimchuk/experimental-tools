# Road map

- [ ] Add to interface (on a method; VS currently has one on a type)
- [ ] Method can be made static
- [ ] Class can be made static
- [ ] Member can be made private
- [ ] Member can be removed

Features that have a checkmark are complete and available for
download in the
[CI build](http://www.vsixgallery.com/extension/fe00c281-eed0-4c6e-901b-d8b845c82e35/).

# Change log

These are the changes to each version that has been released
on the official Visual Studio extension gallery.

## 1.4.2

**2021-11-04

- [x] Updated descriptions.

## 1.4.1

**2021-11-04

- [x] Disabled debugging messages.

## 1.4.0

**2021-11-03

- [x] Updated namespace analyzer to support running in an external process.
- [x] Changed key binding for *Locate In Solution Explorer* command to Alt+L.
- [x] Migrated VSIX to [PackageReference](https://docs.microsoft.com/en-us/nuget/consume-packages/migrate-packages-config-to-package-reference) to streamline VS SDK referencing.

## 1.3.3

**2019-03-19

- [x] Disabled *Add initialized field* refactoring for VS2019 and above.
- [x] Disabled namespace normalization code fix for VS2019 and above.

## 1.3.2

**2019-03-06**

- [x] Fixed a possible crash loading System.IO.FileSystem.Watcher assembly

## 1.3.1

**2019-03-05**

- [x] Fixed a crash in *Add initialized field* refactoring when a generic type has type parameter constraints

## 1.3.0

**2018-10-05**

- [x] Improved detection of the project's default namespace and assembly name
- [x] Disabled *Field can be made readonly* refactoring (VS 2017 version 15.7 and above)
- [x] Enabled support for VS 2019

## 1.2

**2018-08-08**

- [x] Support async package loading
- [x] Better xunit scaffolding trigger

## 1.1

**2017-12-20**

- [x] Fixed crashing FieldCanBeMadeReadOnly refactoring
- [x] Better handling of the default namespace in .NET Core and .NET Standard projects
- [x] Made constructor name code fix more robust

## 1.0

**2017-08-17**

- [x] Field can be made readonly
- [x] Fixed a crash of AddInitializedField refactoring when there are attributes on the type
- [x] Added one more edge case handling to AddConstructorParameter refactoring
- [x] Fixed incorrect brace removing with nested if statements (fixes #13)

## 0.9

**2017-07-06**

- [x] Fixed an issue with erroneous field initialization from a static constructor
- [x] Disable Add Braces refactoring when the cursor is at the parent statement as VS2017 has a built-in IDE0011 diagnostic
- [x] Fixed an issue with namespace vs file path analyzer (#12)

## 0.8

**2017-03-01**

- [x] Retargeted to Visual Studio 2017
- [x] Bug fixes

## 0.7

**2017-01-26**

- [x] Quick GUID generation (nguid)
- [x] Add/remove braces
- [x] Code fix for namespace vs file path analysis
- [x] Bug fixes

## 0.6

**2016-12-30**

- [x] Scaffold xunit data driven tests

## 0.5

**2016-12-09**

- [x] Make it a constructor (when copied from another class)
- [x] Locate in Solution Explorer (Shift+Alt+L)

## 0.4

**2016-11-25**

- [x] Namespace does not match file path analyzer

## 0.3

**2016-11-14**

- [x] Update file name to match type name (and vice versa)
- [x] Settings page (ability to enable/disable individual features)

## 0.2

**2016-10-27**

- [x] Change access modifier on type declarations
- [x] Fixed a bunch of issues

## 0.1

**2016-09-24**

- [x] Initial release
- [x] Initialize field from constructor parameter
- [x] Add constructor and initialize field
- [x] Initialize field in existing constructor
