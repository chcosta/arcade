using Kusto.Cloud.Platform.Data;
using Kusto.Data.Net.Client;
using System;
using System.Linq;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using Kusto.Data;
using Kusto.Ingest;
using Microsoft.DotNet.AzureDevOpsTelemetry.Shared;
using Microsoft.DotNet.KustoIngest.ObjectData;
using System.Data;

namespace Microsoft.DotNet.KustoIngest
{ 
    public class Program
    {
        public static int Main(string[] args)
        {
            string connectionString = args[0];
            string database = args[1];
            string table = args[2];
            string resultsFilePath = args[3];
            string[] lightIngestArgs = new string[] {
                connectionString,
                $"-database:{database}",
                $"-table:{table}",
                $"-source:{Path.GetDirectoryName(resultsFilePath)}",
                $"-pattern:{Path.GetFileName(resultsFilePath)}",
            };

            if(database != null)
            {
                connectionString += $";Initial Catalog={database}";
            }

            if(!File.Exists(resultsFilePath))
            {
                Console.WriteLine($"Data file '{resultsFilePath}' does not exist");
                return 1;
            }

            Dictionary<int, BuildData> builds = new Dictionary<int, BuildData>();
            Dictionary<string, RecordData> records = new Dictionary<string, RecordData>();
            List<IssueData> issues = new List<IssueData>();
            string recordGuid = null;
            foreach(var line in File.ReadAllLines(resultsFilePath))
            {
                var item = FileData.Row.Parse(line);
                if (!item.IsDefault)
                {
                    if (item.Label == "Build")
                    {
                        var buildData = BuildData.Parse(line);
                        builds.Add(buildData.BuildId, buildData);
                    }
                    else if(item.Label == "Record")
                    {
                        var recordData = RecordData.Parse(line);
                        recordGuid = Guid.NewGuid().ToString();
                        recordData.Id = recordGuid;
                        records.Add(recordGuid, recordData);
                    }
                    else if(item.Label == "Issue")
                    {
                        var issueData = IssueData.Parse(line);
                        issueData.RecordId = recordGuid.ToString();
                        issues.Add(issueData);
                    }
                }
            }
            foreach(var issue in issues)
            {
                records[issue.RecordId].AddIssue(issue);
            }
            foreach(var record in records)
            {
                builds[record.Value.BuildId].AddRecord(record.Value);
            }

            var lastBuildDateTime = new DateTime();

            var client = KustoClientFactory.CreateCslQueryProvider(connectionString);
            using (DataTableReader2 reader = (DataTableReader2) client.ExecuteQuery($"{table} | project FinishTime | sort by FinishTime | take 1"))
            {
                if (reader.HasRows)
                {
                    lastBuildDateTime = reader.GetDateTime(0);
                }
            }
            Console.WriteLine($"Last finish time: {lastBuildDateTime}");

            List<string> buildRows = new List<string>();
            List<string> recordRows = new List<string>();
            List<string> issueRows = new List<string>();

            foreach(var build in builds)
            {
                if(build.Value.FinishTime > lastBuildDateTime)
                {
                    buildRows.Add(build.Value.ToCsvString());
                    foreach(var record in build.Value.Records)
                    {
                        recordRows.Add(record.ToCsvString());
                        foreach(var issue in record.Issues)
                        {
                            issueRows.Add(issue.ToCsvString());
                        }
                    }
                }
            }
            string ingestBuildsFilename = GenerateIngestFile("build", buildRows);
            string ingestRecordsFilename = GenerateIngestFile("record", recordRows);
            string ingestIssuesFilename = GenerateIngestFile("issue", issueRows);

            // ToDo: Move these out of the app and make them parameters to the app
            var kustoConnectionStringBuilderDM = new KustoConnectionStringBuilder($"https://ingest-engdata.kusto.windows.net").WithAadApplicationKeyAuthentication(
                applicationClientId: "17eda2bc-2021-4fa3-9d9e-97feeab3be64",
                applicationKey: "rHOB4r]Ryb[In0rpu0Cqg1mNqWWV@U=V",
                authority: "72f988bf-86f1-41af-91ab-2d7cd011db47");

            IKustoIngestClient ingestClient = KustoIngestFactory.CreateQueuedIngestClient(kustoConnectionStringBuilderDM);
            var kustoBuildsIngestionProperties = new KustoIngestionProperties(databaseName: "engineeringdata", tableName: "TimelineBuilds")
            {
                IgnoreFirstRecord = true,
                Format = Kusto.Data.Common.DataSourceFormat.csv
            };
            ingestClient.IngestFromSingleFile(ingestBuildsFilename, deleteSourceOnSuccess: true, ingestionProperties: kustoBuildsIngestionProperties);

            var kustoRecordsIngestionProperties = new KustoIngestionProperties(databaseName: "engineeringdata", tableName: "TimelineRecords")
            {
                IgnoreFirstRecord = true,
                Format = Kusto.Data.Common.DataSourceFormat.csv
            };
            ingestClient.IngestFromSingleFile(ingestRecordsFilename, deleteSourceOnSuccess: true, ingestionProperties: kustoRecordsIngestionProperties);

            var kustoIssuesIngestionProperties = new KustoIngestionProperties(databaseName: "engineeringdata", tableName: "TimelineIssues")
            {
                IgnoreFirstRecord = true,
                Format = Kusto.Data.Common.DataSourceFormat.csv
            };
            ingestClient.IngestFromSingleFile(ingestIssuesFilename, deleteSourceOnSuccess: true, ingestionProperties: kustoIssuesIngestionProperties);

            return 0;
        }

        private static string GenerateIngestFile(string schemaCategory, List<string> rows)
        {
            Guid guid = Guid.NewGuid();
            string filename = $"{schemaCategory}-{guid.ToString()}.txt";
            if (rows.Count > 0)
            {
                using (var writer = File.CreateText(Path.Combine(Environment.CurrentDirectory, filename)))
                {
                    writer.WriteLine(string.Join(",", DataSchema.Schemas[schemaCategory]));
                    foreach (var row in rows)
                    {
                        writer.WriteLine(row);
                    }
                }
            }
            return filename;
        }
    }
}
