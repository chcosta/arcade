# Jenkins Shutdown Status

## Overview

Jenkins for CI has been replaced by Azure DevOps.  **The last day for Jenkins support of .NET Core 3.0 branches is May 3rd, 2019.**  It is expected that all .NET Core 3.0 repos / branches will be using Azure DevOps for CI by May 3rd and that Jenkins is no longer used for CI.

This document is tracking the status of .NET Core 3.0 repos using Jenkins for CI and provides guidance for how to disable Jenkins for a repo / branch.

## Steps for disabling Jenkins

1. Remove entry from [dotnet-ci](https://github.com/dotnet/dotnet-ci/blob/master/data/repolist.txt)
2. Delete repos netci.groovy file

The Jenkins generator job and associated jobs *should* delete themselves. However, if it doesn’t you can delete the full folder for the repo/branch combo on the server:

1. Log into Jenkins instance
2. Find repo/branch folder
3. Hit Delete Folder on left
4. Confirm.

## Status Overview

| Repo                       | Owner            | Status      | Risk   | Curent Jenkins jobs | Notes |
| ---------------------------| ---------------- |:-----------:|:------:| ------------------- | ----- |
| Arcade                     | mawilkie         | -           | -      | - ||
| aspnet-AspNetCore          | namc             | -           | -      | - ||
| aspnet-AspNetCore-Tooling  | namc             | -           | -      | - ||
| aspnet-EntityFrameworkCore | namc             | -           | -      | - ||
| aspnet-Extensions          | namc             | -           | -      | - ||
| CLI                        | licavalc         | -           | -      | - ||
| CLICommandLineParser       | licavalc         | -           | -      | - ||
| CLI-Migrate                | licavalc         | -           | -      | - ||
| **CoreClr**                | russellk         | In progress | Medium | ci1, ci2, ci3 | Under discussion: the highest risk is getting the CoreFx jobs for CoreClr working in Azure DevOps. There are a lot of other jobs that need to be ported but they are low risk. (CoreClr really wants queue time parameters to be supported via comment triggers and that work is scheduled for Q2) |
| CoreFx                     | danmose          | -           |        | - ||
| Core-SDK                   | licavalc         | -           | -      | - ||
| Core-Setup                 | dleeapho         | -           | -      | - ||
| **dotnet-docker**          | msimons          | In progress | Low    | [ci1](https://ci.dot.net/job/dotnet_dotnet-docker/) | [Tracking issue](https://github.com/dotnet/dotnet-docker/issues/744), no known blocking issues at this time |
| **dotnet-framework-docker**| msimons          | In progress | Low    | [ci1](https://ci.dot.net/job/Microsoft_dotnet-framework-docker/) | [Tracking issue](https://github.com/Microsoft/dotnet-framework-docker/issues/225), no known blocking issues at this time |
| docker-tools               | msimons          | -           |        | - ||
|**dotnet-buildtools-prereqs-docker** | msimons | In progress | Low    | [ci1](https://ci.dot.net/job/dotnet_dotnet-buildtools-prereqs-docker/) | [Tracking issue](https://github.com/dotnet/dotnet-buildtools-prereqs-docker/issues/84), no known blocking issues at this time |
| MSBuild                    | licavalc         | -           | -      | - ||
| Roslyn                     | jaredpar         | -           | -      | - ||
| **SDK**                    | licavalc         | In progress | Low    | [ci2](https://ci2.dot.net/job/dotnet_sdk/) | Planned, moving perf job to Azure DevOps |
| Standard                   | danmose          | -           | -      | - ||
| SymReader                  | tmat             | -           | -      | - ||
| SymReader-Portable         | tmat             | -           | -      | - ||
| Templating                 | vramak           | -           | -      | - ||
| **TestFx**                 | sarabjot         | In progress | Low    | [ci1](https://ci.dot.net/job/Microsoft_testfx/job/master/) | Planned |
| Test-Templates             | sasin            | -           | -      | - ||
| Toolset                    | licavalc         | -           | -      | - ||
| VSTest                     | sarabjot         | -           | -      | - ||
| VisualFSharp               | brettfo          | -           | -      | - ||
| WebSDK                     | vramak           | -           | -      | - ||
| WinForms                   | mmcgaw           | -           | -      | - ||
| WPF                        | vatsan-madhavan  | -           | -      | - ||

`-` means an item is complete or not needed

## Additional Jenkins jobs

Additionally, here are other Jenkins jobs which are not specifically part of the core product repos, but are likely candidates for deletion or moving to Azure DevOps.  It would be great if we could get owners attached to these jenkins jobs and a plan for them being disabled from Jenkins.


| Job name                      | Jenkins link                                                                | Owner        | Removal plan |
| ----------------------------- | --------------------------------------------------------------------------- | ------------ | ------------ |
| aspnet_aspnet-docker          | [ci3](https://ci.dot.net/job/aspnet_aspnet-docker/)                         | dougbu       ||
| aspnet_IISIntegration         | [ci3](https://ci.dot.net/job/aspnet_IISIntegration/job/master/)             | dougbu       ||
| aspnet_KestrelHttpServer      | [ci3](https://ci.dot.net/job/aspnet_KestrelHttpServer/job/master/)          | dougbu       ||
| dotnet_CITest                 | [ci2](https://ci2.dot.net/job/dotnet_citest/)                               | mmitche      ||
| dotnet_CodeFormatter          | [ci2](https://ci2.dot.net/job/dotnet_codeformatter/)                        | ericstj      ||
| dotnet_Core                   | [ci2](https://ci2.dot.net/job/dotnet_core/)                                 | mmitche      ||
| dotnet_CoreFxLab              | [ci1](https://ci.dot.net/job/dotnet_corefxlab/job/master/)                  | ahka         ||
| dotnet_CoreRt                 | [ci1](https://ci.dot.net/job/dotnet_corert/job/master/)                     | sergeyk      ||
| dotnet_Diagnostics            | [ci1](https://ci.dot.net/job/dotnet_diagnostics/)                           | mikem        | Removed |
| dotnet_Interactive-Window     | [ci1](https://ci.dot.net/job/dotnet_Interactive-Window/)                    | tmat         | Already moved to Azure DevOps and can be removed? |
| dotnet_Metadata-Tools         | [ci2](https://ci2.dot.net/job/dotnet_metadata-tools/)                       | tmat         | Already moved to Azure DevOps and can be removed? |
| dotnet_Orleans                | [ci1](https://ci.dot.net/job/dotnet_orleans/)                               | mmitche      ||
| dotnet_Perf-Infra             | [ci2](https://ci2.dot.net/job/dotnet_perf-infra/)                           | anscoggi     ||
| dotnet_Performance            | [ci2](https://ci2.dot.net/job/dotnet_performance/)                          | michelm      ||
| dotnet_Platform-Compat        | [ci2](https://ci2.dot.net/job/dotnet_platform-compat/)                      | jmarolf      ||
| dotnet_ProjFileTools          | [ci1](https://ci.dot.net/job/dotnet_ProjFileTools/)                         | mmitche      ||
| dotnet_Roslyn-Analyzers       | [ci1](https://ci.dot.net/job/dotnet_roslyn-analyzers/job/master/)           | tmat         ||
| dotnet_Roslyn-Tools           | [ci1](https://ci.dot.net/job/dotnet_roslyn-tools/)                          | tmat         ||
| dotnet_SymReader-Converter    | [ci2](https://ci2.dot.net/job/dotnet_symreader-converter/)                  | tmat         ||
| dotnet_Versions               | [ci1](https://ci.dot.net/job/dotnet_versions/)                              | mmitche      ||
| dotnet_WCF                    | [ci1](https://ci.dot.net/job/dotnet_wcf/job/master/)                        | stebon       ||
| dotnet_Xliff-Tasks            | [ci1](https://ci.dot.net/job/dotnet_xliff-tasks/)                           | licavalc     ||
| drewscoggins_corefx           | [ci2](https://ci2.dot.net/job/drewscoggins_corefx/)                         | drewscoggins ||
| Microsoft_ConcordExtensibilitySamples | [ci1](https://ci.dot.net/job/Microsoft_ConcordExtensibilitySamples/)| greggm       ||
| Microsoft_MIEngine            | [ci1](https://ci.dot.net/job/Microsoft_MIEngine/)                           | piel         ||
| Microsoft_PartsUnlimited      | [ci1](https://ci.dot.net/job/Microsoft_PartsUnlimited/)                     | enewman      ||
| mono_linker                   | [ci1](https://ci.dot.net/job/mono_linker/)                                  | svbomer      ||
