# Post-Build signing telemetry

The Post-Build signing feature needs to provide adequate telemetry to diagnose performance related issues.

## Goal

Gather data that is usable to investigate release pipeline signing performance in an effort to understand trends and make reasonable decisions about which components are primary contributors to signing time.

## Scope

- Telemetry for these metrics
  - Time for unpacking in signtool
  - Number of files signed in signtool
  - Time for repacking in signtool
  - Time for signing in signtool
- Telemetry for pipeline timings (this data is available via Azure DevOps Timeline API)
- Applicable to the Stage-DotNet pipeline Production runs

## Out of scope

- Telemetry for other pieces of the release pipeline (publishing, validation, etc...)
- Telemetry for other branches of the release pipeline
- Alerting on signing metrics
- Additional reports on signing metrics
