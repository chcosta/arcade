# Azure Util

Small utility library for querying azure services. Mostly focusing on Azuer DevOps at the moment

https://docs.microsoft.com/en-us/rest/api/azure/devops/build/timeline/get?view=azure-devops-rest-5.0#timelinereference

## Installation

This is packaged as a .NET global tool.  You can install it like this...

> dotnet tool install Microsoft.DotNet.AzureDevOpsTelemetry --tool-path e:\chcosta\azdot --add-source https://dotnetfeed.blob.core.windows.net/dotnet-core/index.json --version 1.0.0-beta.19317.8

## Usage

```TEXT
Usage: azdot -organization (organization name) -project (project name) [optional parameters]
  Optional parameters:
    -pat (value)    # Personal authentication token for Azure DevOps
    -mindatetime (value)  # earliest date time to return build records from (Default is yesterday
    -maxdatetime (value)  # latest date time to return build records from (Default is now)
    -outfile (value)      # tsv file to write results to (Default is results.tsv)
    -buildreasonfilter (value)  # Azure DevOps build reasons to filter results to (Default is 'IndividualCI,BatchedCI')
```