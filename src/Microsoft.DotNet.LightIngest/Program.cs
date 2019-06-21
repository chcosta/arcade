using Kusto.Cloud.Platform.Data;
using Kusto.Data.Net.Client;
using System;
using System.Linq;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using Kusto.Data;
using Kusto.Ingest;

namespace Microsoft.DotNet.LightIngest
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

            var lastBuildDateTime = new DateTime();

            var client = KustoClientFactory.CreateCslQueryProvider(connectionString);
            using (var reader = client.ExecuteQuery($"{table}" +
                                             $" | sort by FinishTime" +
                                             $" | limit 1"))
            {
                var objectReader = new ObjectReader<TimelineBuild>(reader, true);

                var objectReaderEnumerator = objectReader.GetEnumerator();
                if (objectReaderEnumerator.MoveNext())
                {
                    var build = objectReaderEnumerator.Current;

                    lastBuildDateTime = build.FinishTime;
                }
            }
            Console.WriteLine($"Last finish time: {lastBuildDateTime}");

            if(!File.Exists(resultsFilePath))
            {
                Console.WriteLine($"Data file '{resultsFilePath}' does not exist");
                return 1;
            }

            List<FileData.Row> buildRows = new List<FileData.Row>();
            List<FileData.Row> messageRows = new List<FileData.Row>();
            foreach(var line in File.ReadAllLines(resultsFilePath))
            {
                var item = FileData.Row.Parse(line);
                if (!item.IsDefault)
                {
                    if (item.Label == "Build")
                    {
                        buildRows.Add(item);
                    }
                    else if(item.Label == "Message")
                    {
                        messageRows.Add(item);
                    }
                }
            }

            Dictionary<string, BuildData> builds = new Dictionary<string, BuildData>();
            Guid guid = Guid.NewGuid();

            string ingestBuildsFilename = $"builds-{guid.ToString()}.txt";
            if(buildRows.Count > 0)
            {
                using (var writer = File.CreateText(Path.Combine(Environment.CurrentDirectory, ingestBuildsFilename)))
                {
                    writer.WriteLine("buildid,status,result,repository,reason,buildnumber,queuetime,starttime,finishtime");
                    foreach (var row in buildRows)
                    {
                        var finishTime = row.Columns["finishtime"];
                        if (DateTime.TryParse(finishTime, out DateTime finishDateTime))
                        {
                            if (finishDateTime > lastBuildDateTime)
                            {

                                var r = String.Join(",", row.Columns.Select(c => c.Value));
                                writer.WriteLine(r);
                            }
                        }
                    }
                }
            }

            string ingestMessagesFilename = $"messages-{guid.ToString()}.txt";
            if(messageRows.Count > 0)
            {
                using (var writer = File.CreateText(Path.Combine(Environment.CurrentDirectory)))
                {
                    writer.WriteLine("buildid,issuetype,name,logurl,attempt,message,count,order");
                    foreach (var row in messageRows)
                    {

                    }
                }
            }
            // ToDo: Move these out of the app and make them parameters to the app
            var kustoConnectionStringBuilderDM = new KustoConnectionStringBuilder($"https://ingest-engdata.kusto.windows.net").WithAadApplicationKeyAuthentication(
                applicationClientId: "17eda2bc-2021-4fa3-9d9e-97feeab3be64",
                applicationKey: "rHOB4r]Ryb[In0rpu0Cqg1mNqWWV@U=V",
                authority: "72f988bf-86f1-41af-91ab-2d7cd011db47");

            IKustoIngestClient ingestClient = KustoIngestFactory.CreateQueuedIngestClient(kustoConnectionStringBuilderDM);
            var kustoIngestionProperties = new KustoIngestionProperties(databaseName: "engineeringdata", tableName: "TimelineBuilds")
            {
                IgnoreFirstRecord = true,
                Format = Kusto.Data.Common.DataSourceFormat.csv
            };
            ingestClient.IngestFromSingleFile(ingestBuildsFilename, deleteSourceOnSuccess: true, ingestionProperties: kustoIngestionProperties);
            return 0;

            // return RunLightIngest(lightIngestArgs);
        }

        private static int RunLightIngest(string[] args)
        { 
            var exeDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..");
            var exe = Path.Combine(exeDirectory, "lightingest.exe");
            var psi = new ProcessStartInfo(exe, string.Join(" ", args));
            var process = Process.Start(psi);
            process.WaitForExit();
            return process.ExitCode;
        }
    }
}
