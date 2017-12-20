# Road map

- [ ] Add to interface (on a method; VS currently has one on a type)
- [ ] Method can be made static
- [ ] Class can be made static
- [ ] Member can be made private
- [ ] Member can be removed

Features that have a checkmark are complete and available for
download in the
[CI build](http://vsixgallery.com/extension/f2ba275d-a5ca-4bf9-b8ef-2e580cb13cd3/).

# Change log

These are the changes to each version that has been released
on the official Visual Studio extension gallery.

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

**2017-06-09**

- [x] Fixed an issue with erroneous field initialization from a static constructor
- [x] Fixed an issue with namespace vs file path analyzer (#12)

## 0.8

**2017-03-01**

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