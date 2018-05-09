[CmdletBinding(PositionalBinding=$false)]
Param (
    [Parameter(Mandatory=$True)]
    [string] $InstallDirectory,
    [switch] $Force = $False
)

$CommonLibraryPath = Join-Path $PSScriptRoot "CommonLibrary.psm1"
$InstallPath = Join-Path $PSScriptRoot "CommonLibrary.psm1"

if ($Force) {
    if (Test-Path $InstallPath) {
        Remove-Item $InstallPath -Force
    }
}

if (-Not (Test-Path $InstallDirectory)) {
    New-Item -path $InstallDirectory -force -itemType "Directory" | Out-Null
}

Copy-Item -Path $CommonLibraryPath -Destination $InstallPath