## Producing Light Command packages from runtime repo

Build definition: https://dev.azure.com/dnceng/internal/_build?definitionId=679&_a=summary

Build branch: msi-drop

Building the above branch in the build will produce Light packages.   The Light packages are listed in the builds `AssetManifests/Manifest_Installers.xml` file.

The package which creates Light command packages was produced out of Arcade and is `Microsoft.DotNet.Build.Tasks.SharedFramework.Sdk.5.0.0-beta.20265.3.nupkg`.  Changes have been merged, so any recent build of Arcade should also produce a SharedFramework.sdk package that will create light command packages.

Make these changes to the runtime repo to produce / publish Light packages.

==== global.json ====

```
-    "Microsoft.DotNet.Build.Tasks.SharedFramework.Sdk": "5.0.0-blah"
+    "Microsoft.DotNet.Build.Tasks.SharedFramework.Sdk": "5.0.0-beta.20265.3"
```

==== NuGet.config ====

```
+    <add key="general-testing" value="https://pkgs.dev.azure.com/dnceng/public/_packaging/general-testing/nuget/v3/index.json" />
```

==== src/installer/pkg/Directory.Build.props ====

```
   <PropertyGroup>
     <Platform>$(TargetArchitecture)</Platform>
     <DotNetHostBinDir>$(BaseOutputRootPath)corehost</DotNetHostBinDir>
+    <EnableCreateLightCommandPackageDrop>true</EnableCreateLightCommandPackageDrop>
   </PropertyGroup>
```

