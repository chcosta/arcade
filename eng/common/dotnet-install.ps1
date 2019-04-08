[CmdletBinding(PositionalBinding=$false)]
Param(
  [string] $verbosity = "minimal",
  [string] $architecture = "",
  [string] $version = "Latest"
)

. $PSScriptRoot\tools.ps1

try {
  $dotnetRoot = Join-Path $RepoRoot ".dotnet"
  InstallDotNet $dotnetRoot $version $architecture "dotnet" $true
} 
catch {
  Write-Host $_
  Write-Host $_.Exception
  Write-Host $_.ScriptStackTrace
  ExitWithExitCode 1
}

ExitWithExitCode 0