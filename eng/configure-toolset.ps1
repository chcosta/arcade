function Test-FilesUseTelemetryOutput {
    $requireTelemetryExcludeFiles = @(
        "pipeline-logging-functions.ps1", 
        "enable-cross-org-publishing.ps1",
        "performance-setup.ps1" )

    $filesMissingTelemetry = Get-ChildItem -File -Recurse -Path $engCommonRoot -Include "*.ps1" -Exclude $requireTelemetryExcludeFiles |
        Where-Object { -Not( $_ | Select-String -Pattern "Write-PipelineTelemetryError" )}

    If($filesMissingTelemetry) {
        Write-Host "All ps1 files under eng/common are required to use pipeline logging functions for writing pipeline telemetry errors."
        Write-Host "See https://github.com/dotnet/arcade/blob/master/eng/common/pipeline-logging-functions.ps1"
        Write-Host "The following ps1 files do not include telmetry categorization output:"
    }
    ForEach($file In $filesMissingTelemetry) {
        Write-Host $file
    }
    If($filesMissingTelemetry) {
        Exit 1
    }
}

$engCommonRoot = Join-Path $PSScriptRoot "common"
$failOnConfigureToolsetError = $true
Test-FilesUseTelemetryOutput
