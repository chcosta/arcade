<#
.SYNOPSIS
Entry point script for installing native tools

.DESCRIPTION
Reads $RepoRoot\eng\NativeToolsVersion.txt file to determine native assets to install
and executes installers for those tools

.PARAMETER BaseUri
Base file directory or Url from which to acquire tool archives

.PARAMETER InstallDirectory
Directory to install native toolset.  This is a command-line override for the default
Install directory precedence order:
- InstallDirectory command-line override
- COMMONLIBRARY_NATIVEINSTALLDIRECTORY environment variable
- (default) %USERPROFILE%/.netcoreeng/native

.PARAMETER Clean
Switch specifying to not install anything, but cleanup native asset folders

.PARAMETER Force
Clean and then install tools

.PARAMETER DownloadRetries
Total number of retry attempts

.PARAMETER RetryWaitTimeInSeconds
Wait time between retry attempts in seconds

.PARAMETER ToolsVersionsFile
File path to tools versions file

.NOTES
#>
[CmdletBinding(PositionalBinding=$false)]
Param (
    [string] $BaseUri = "https://dotnetfeed.blob.core.windows.net/chcosta-test/nativeassets",
    [string] $InstallDirectory,
    [switch] $Clean = $False,
    [switch] $Force = $False,
    [int] $DownloadRetries = 5,
    [int] $RetryWaitTimeInSeconds = 30,
    [string] $ToolsVersionsFile = "$PSScriptRoot\..\..\eng\NativeToolsVersions.txt"
)

Set-StrictMode -version 2.0
$ErrorActionPreference="Stop"

Import-Module -Name (Join-Path $PSScriptRoot "native\Windows-CommonLibrary\CommonLibrary.psm1")

try {
    # Define verbose switch if undefined
    $Verbose = $VerbosePreference -Eq "Continue"

    $EngCommonBaseDir = Join-Path $PSScriptRoot "native\"
    $CommonLibraryDirectory = Join-Path $EngCommonBaseDir "Windows-CommonLibrary"

    $NativeBaseDir = $InstallDirectory
    if (!$NativeBaseDir) {
      $NativeBaseDir = CommonLibrary\Get-NativeInstallDirectory
    }
    $Env:CommonLibrary_NativeInstallDir = $NativeBaseDir
    $InstallBin = Join-Path $NativeBaseDir "bin"

    if ($Clean -Or $Force) {
        Write-Host "Cleaning '$NativeBaseDir'"
        if (Test-Path $NativeBaseDir) {
            Remove-Item $NativeBaseDir -Force -Recurse
        }

        if ($Clean) {
            exit 0
        }
    }

    # Process tools list
    Write-Host "Processing $ToolsVersionsFile"
    If (-Not (Test-Path $ToolsVersionsFile)) {
        Write-Host "No native tool dependencies are defined in '$ToolsVersionsFile'"
        exit 0
    }
    $ToolsList = ((Get-Content $ToolsVersionsFile) -replace ',','=') -join "`n" | ConvertFrom-StringData

    Write-Verbose "Required native tools:"
    $ToolsList.GetEnumerator() | ForEach-Object {
        $Key = $_.Key
        $Value = $_.Value
        Write-Verbose "- $Key ($Value)"
    }

    # Execute installers
    Write-Host "Executing installers"
    $ToolsList.GetEnumerator() | ForEach-Object {
        $ToolName = $_.Key
        $ToolVersion = $_.Value
        $InstallerFilename = "Windows-$ToolName\install-$ToolName.ps1"
        $LocalInstallerCommand = Join-Path $EngCommonBaseDir $InstallerFilename
        $LocalInstallerCommand += " -InstallPath $InstallBin"
        $LocalInstallerCommand += " -BaseUri $BaseUri"
        $LocalInstallerCommand += " -CommonLibraryDirectory $CommonLibraryDirectory"
        $LocalInstallerCommand += " -Version $ToolVersion"

        if ($Verbose) {
            $LocalInstallerCommand += " -Verbose"
        }
        if (Get-Variable 'Force' -ErrorAction 'SilentlyContinue') {
            if($Force) {
                $LocalInstallerCommand += " -Force"
            }
        }

        Write-Verbose "Installing $ToolName version $ToolVersion"
        Write-Verbose "Executing '$LocalInstallerCommand'"
        Invoke-Expression "$LocalInstallerCommand"
        if ($LASTEXITCODE -Ne "0") {
            Write-Error "Execution failed"
            exit 1
        }
    }

    if (Test-Path $InstallBin) {
        Write-Host "Native tools are available from" (Convert-Path -Path $InstallBin)
    }
    else {
        Write-Error "Native tools install directory does not exist, installation failed"
        exit $False
    }
    exit 0
}
catch {
    Write-Host $_
    Write-Host $_.Exception
    exit 1    
}
