# Multiple Shared Framework Installations in Arcade

## Proposal

- define frameworks in global.json
- no generated props file or use of a `DotNetCoreRuntimeVersions` item group
- if runtimes section is present, assume we can't fulfill the specs globally and force use of `.dotnet` folder

Notes: This will require updating darc to support the format change - https://github.com/dotnet/arcade-services/commit/9a486718cdb33c82f9bb24cf25d1207e364f71c7

## Scenario 1 - Fixed set of shared frameworks for testing

global.json

```json
{
  "tools": {
    "dotnet": {
      "sdk": "2.2.202",
      "runtimes": {
        "dotnet": [ "2.1.1" ],
        "aspnetcore": [ "2.2.3" ]
      }
    }
  },
  "msbuild-sdks": {
    "Microsoft.DotNet.Arcade.Sdk": "1.0.0-beta.19207.1",
    "Microsoft.DotNet.Helix.Sdk": "2.0.0-beta.19207.1"
  }
}
```

Define frameworks (runtimes) in global.json.

Applies to:

- repos with a fixed set of shared frameworks used for testing

Behavior:

- SDK is installed locally (into `.dotnet` folder)
- Frameworks are installed locally (into `.dotnet` folder)
- We print a message during install indicating that location of the installed sdk / frameworks

## Scenario 2 - Dependency flow defined versions of shared frameworks for testing

global.json

```json
{
  "tools": {
    "dotnet": {
      "sdk": "2.2.202",
      "runtimes": {
        "dotnet": [ "2.1.1", "MicrosoftNetCoreAppVersion" ],
        "aspnetcore": [ "2.2.3", "MicrosoftAspNetCoreAppVersion" ]
      }
  },
  "msbuild-sdks": {
    "Microsoft.DotNet.Arcade.Sdk": "1.0.0-beta.19207.1",
    "Microsoft.DotNet.Helix.Sdk": "2.0.0-beta.19207.1"
  }
}
```

eng/Versions.props

```XML
<Project>
  <PropertyGroup>
    <MicrosoftNetCoreAppVersion>3.0.0-preview4-19207.1</MicrosoftNetCoreAppVersion>
    <MicrosoftAspNetCoreAppVersion>3.0.0-preview4-19207.1</MicrosoftAspNetCoreAppVersion>
     ...
  </PropertyGroup>
</Project>
```

Applies to:

- repos which rely on dependency flow for defining framework versions

Behavior:

- SDK is installed locally (into `.dotnet` folder)
- Frameworks are installed locally (into `.dotnet` folder)
- For non-explicit framework versions, we read in eng/Version.props and translate identified property group values into versions
- We print a message during install indicating that location of the installed sdk / frameworks
