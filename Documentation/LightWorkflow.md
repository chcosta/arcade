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

There's no current runtime build that has been "promoted" and published the Light packages.  This Runtime build is a viable candidate though, https://dev.azure.com/dnceng/internal/_build/results?buildId=649910.  The LightPackages are listed in the `AssetManifests/manifest_installers.xml` file


Manifest_Installers.xml: 

```
... Location="https://dotnetfeed.blob.core.windows.net/dotnet-core/index.json">
  <Blob Id="Runtime/5.0.0-preview.6.20268.4/LightCommandPackage-dotnet-apphost-pack-5.0.0-preview.6.20268.4-win-x64.zip" />
  <Blob Id="Runtime/5.0.0-preview.6.20268.4/LightCommandPackage-dotnet-apphost-pack-5.0.0-preview.6.20268.4-win-x64.zip.sha512" />
  <Blob Id="Runtime/5.0.0-preview.6.20268.4/LightCommandPackage-dotnet-apphost-pack-5.0.0-preview.6.20268.4-win-x64_arm.zip" />
  <Blob Id="Runtime/5.0.0-preview.6.20268.4/LightCommandPackage-dotnet-apphost-pack-5.0.0-preview.6.20268.4-win-x64_arm.zip.sha512" />
  <Blob Id="Runtime/5.0.0-preview.6.20268.4/LightCommandPackage-dotnet-apphost-pack-5.0.0-preview.6.20268.4-win-x64_arm64.zip" />
  <Blob Id="Runtime/5.0.0-preview.6.20268.4/LightCommandPackage-dotnet-apphost-pack-5.0.0-preview.6.20268.4-win-x64_arm64.zip.sha512" />
  <Blob Id="Runtime/5.0.0-preview.6.20268.4/LightCommandPackage-dotnet-apphost-pack-5.0.0-preview.6.20268.4-win-x64_x86.zip" />
  <Blob Id="Runtime/5.0.0-preview.6.20268.4/LightCommandPackage-dotnet-apphost-pack-5.0.0-preview.6.20268.4-win-x64_x86.zip.sha512" />
  <Blob Id="Runtime/5.0.0-preview.6.20268.4/LightCommandPackage-dotnet-apphost-pack-5.0.0-preview.6.20268.4-win-x86.zip" />
  <Blob Id="Runtime/5.0.0-preview.6.20268.4/LightCommandPackage-dotnet-apphost-pack-5.0.0-preview.6.20268.4-win-x86.zip.sha512" />
  <Blob Id="Runtime/5.0.0-preview.6.20268.4/LightCommandPackage-dotnet-apphost-pack-5.0.0-preview.6.20268.4-win-x86_arm.zip" />
  <Blob Id="Runtime/5.0.0-preview.6.20268.4/LightCommandPackage-dotnet-apphost-pack-5.0.0-preview.6.20268.4-win-x86_arm.zip.sha512" />
  <Blob Id="Runtime/5.0.0-preview.6.20268.4/LightCommandPackage-dotnet-apphost-pack-5.0.0-preview.6.20268.4-win-x86_arm64.zip" />
  <Blob Id="Runtime/5.0.0-preview.6.20268.4/LightCommandPackage-dotnet-apphost-pack-5.0.0-preview.6.20268.4-win-x86_arm64.zip.sha512" />
  <Blob Id="Runtime/5.0.0-preview.6.20268.4/LightCommandPackage-dotnet-apphost-pack-5.0.0-preview.6.20268.4-win-x86_x64.zip" />
  <Blob Id="Runtime/5.0.0-preview.6.20268.4/LightCommandPackage-dotnet-apphost-pack-5.0.0-preview.6.20268.4-win-x86_x64.zip.sha512" />
  <Blob Id="Runtime/5.0.0-preview.6.20268.4/LightCommandPackage-dotnet-crossgen2-pack-5.0.0-preview.6.20268.4-win-x64.zip" />
  <Blob Id="Runtime/5.0.0-preview.6.20268.4/LightCommandPackage-dotnet-crossgen2-pack-5.0.0-preview.6.20268.4-win-x64.zip.sha512" />
  <Blob Id="Runtime/5.0.0-preview.6.20268.4/LightCommandPackage-dotnet-host-5.0.0-preview.6.20268.4-win-x64.zip" />
  <Blob Id="Runtime/5.0.0-preview.6.20268.4/LightCommandPackage-dotnet-host-5.0.0-preview.6.20268.4-win-x64.zip.sha512" />
  <Blob Id="Runtime/5.0.0-preview.6.20268.4/LightCommandPackage-dotnet-host-5.0.0-preview.6.20268.4-win-x86.zip" />
  <Blob Id="Runtime/5.0.0-preview.6.20268.4/LightCommandPackage-dotnet-host-5.0.0-preview.6.20268.4-win-x86.zip.sha512" />
  <Blob Id="Runtime/5.0.0-preview.6.20268.4/LightCommandPackage-dotnet-hostfxr-5.0.0-preview.6.20268.4-win-x64.zip" />
  <Blob Id="Runtime/5.0.0-preview.6.20268.4/LightCommandPackage-dotnet-hostfxr-5.0.0-preview.6.20268.4-win-x64.zip.sha512" />
  <Blob Id="Runtime/5.0.0-preview.6.20268.4/LightCommandPackage-dotnet-hostfxr-5.0.0-preview.6.20268.4-win-x86.zip" />
  <Blob Id="Runtime/5.0.0-preview.6.20268.4/LightCommandPackage-dotnet-hostfxr-5.0.0-preview.6.20268.4-win-x86.zip.sha512" />
  <Blob Id="Runtime/5.0.0-preview.6.20268.4/LightCommandPackage-dotnet-runtime-5.0.0-preview.6.20268.4-win-x64.zip" />
  <Blob Id="Runtime/5.0.0-preview.6.20268.4/LightCommandPackage-dotnet-runtime-5.0.0-preview.6.20268.4-win-x64.zip.sha512" />
  <Blob Id="Runtime/5.0.0-preview.6.20268.4/LightCommandPackage-dotnet-runtime-5.0.0-preview.6.20268.4-win-x86.zip" />
  <Blob Id="Runtime/5.0.0-preview.6.20268.4/LightCommandPackage-dotnet-runtime-5.0.0-preview.6.20268.4-win-x86.zip.sha512" />
  <Blob Id="Runtime/5.0.0-preview.6.20268.4/LightCommandPackage-dotnet-targeting-pack-5.0.0-preview.6.20268.4-win-x64.zip" />
  <Blob Id="Runtime/5.0.0-preview.6.20268.4/LightCommandPackage-dotnet-targeting-pack-5.0.0-preview.6.20268.4-win-x64.zip.sha512" />
  <Blob Id="Runtime/5.0.0-preview.6.20268.4/LightCommandPackage-dotnet-targeting-pack-5.0.0-preview.6.20268.4-win-x86.zip" />
  <Blob Id="Runtime/5.0.0-preview.6.20268.4/LightCommandPackage-dotnet-targeting-pack-5.0.0-preview.6.20268.4-win-x86.zip.sha512" />
  ...
```