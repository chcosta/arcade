Microsoft.DotNet.Build.Tasks.NuGet
==================================

Contains tasks related to file NuGet.

See ["Task Packages"](../../Documentation/TaskPackages.md#usage) for guidance on installing this package.

Tasks in this package

 - PackNuspc

## Tasks

This package contains the following MSBuild tasks.

### `PackNuspec`

Creates a NuGet package from a .nuspec file.

Task parameter           | Type        | Description
-------------------------|-------------|--------------------------------------------------------------------------------
File                     | string      | **[Required]** The path to the .nuspec file to pack
OutputPath               | string      | The full file path to where nupkg file will be placed.
DestinationFolder        | string      | The directory where nupkg file will be placed. Default file name is "$(packageId).$(version).nupkg"
Overwrite                | boolean     | Overwrite files if they exists already in DestinationFolder or OutputPath. Defaults to false.
Version                  | string      | The package version to use. Overrides any value in .nuspec or `Properties`.
Properties               | string[]    | Provides substitution in the nuspec for variables using `$varName$` syntax. Input should be "key=value" pairs.
BasePath                 | string      | The base path to use for any relative paths in the &lt;files&gt; section of nuspec.
Dependencies             | ITaskItem[] | Dependencies to add to the &lt;dependencies&gt; section of the spec. Metadata 'TargetFramework' can be specified to put dependencies into groups with targetFramework set.
PackageFiles             | ITaskItem[] | Files to add to the package. Must specify the PackagePath metadata.
IncludeEmptyDirectories  | boolean     | Pack empty directories.
Packages                 | ITaskItem[] | **[Output]** The full path to package files created.


Notes:
 - Either OutputPath or DestinationFolder must be specified.

Example:
```xml
<PackNuspec File="MyPackage.nuspec" Properties="version=$(PackageVersion)" DestinationFolder="$(PackagesDir)" />
```
