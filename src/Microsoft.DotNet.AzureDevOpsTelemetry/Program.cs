using Microsoft.DotNet.AzureDevOpsTelemetry.Util;
using System;
using System.Threading.Tasks;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Security;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.DotNet.AzureDevOpsTelemetry
{ 
    public class Program
    {
        private static string s_Organization;
        private static string s_Project;
        private static AzureServer s_azureServer;
        private static string s_MetadataFile = "results.tsv";

        public static int Main(string[] args)
        {
            string personalAccessToken = null;
            DateTime minDateTime = DateTime.Now.Subtract(new TimeSpan(1, 0, 0, 0));
            DateTime maxDateTime = DateTime.Now;
            string buildReasonFilter = "individualCI,batchedCI";

            for (int i = 0; i < args.Length; i+=2)
            {
                string key = args[i];
                if(i + 1 >= args.Length)
                {
                    DisplayUsage();
                    return 1;
                }
                string value = args[i + 1];

                switch(key)
                {
                    case "-organization":
                        s_Organization = value;
                        break;
                    case "-project":
                        s_Project = value;
                        break;
                    case "-pat":
                        personalAccessToken = value;
                        break;
                    case "-mindatetime":
                        DateTime.TryParse(value, out minDateTime);
                        break;
                    case "-maxdatetime":
                        DateTime.TryParse(value, out maxDateTime);
                        break;
                    case "-outfile":
                        s_MetadataFile = value;
                        break;
                    case "-buildreason":
                        buildReasonFilter = value;
                        break;
                    default:
                        Console.WriteLine($"Unknown option '{key}'");
                        DisplayUsage();
                        return 1;
                }
            }

            if(string.IsNullOrWhiteSpace(s_Organization))
            {
                Console.WriteLine("Missing required parameter '-organization [organization name]'");
                DisplayUsage();
                return 1;
            }
            if (string.IsNullOrWhiteSpace(s_Project))
            {
                Console.WriteLine("Missing required parameter '-project [project name]'");
                DisplayUsage();
                return 1;
            }

            s_azureServer = new AzureServer(s_Organization, personalAccessToken);

            // Get builds in project since a specified dateTime
            var builds = GetBuilds(minDateTime, maxDateTime, buildReasonFilter).GetAwaiter().GetResult();

            var tasks = new Dictionary<Build, Task<Timeline>>();
            foreach (var build in builds)
            {
                tasks.Add(build, DumpTimeline(build.Id));
            }
            Task.WaitAll(tasks.Select(s => s.Value).ToArray());

            List<string> writeLines = new List<string>();
            foreach (var task in tasks)
            {
                var build = task.Key;
                writeLines.Add($"Build\t{build.Id}\t{build.Status}\t{build.Repository.id}\t{build.Reason}\t{build.BuildNumber}\t{build.Result}\t{build.QueueTime}\t{build.StartTime}\t{build.FinishTime}");
                foreach (var validationResult in build.ValidationResults)
                {
                    writeLines.Add($"Job\t{build.Id}\t{validationResult.Message}");
                }
                if (task.Value.Result != null)
                {
                    var timeline = task.Value.Result;
                    if (timeline.Records != null)
                    {
                        foreach (var job in timeline.Records)
                        {
                            if (job.Issues != null)
                            {
                                foreach (var issue in job.Issues)
                                {
                                    writeLines.Add($"    Message\t{task.Key.Id}\t{issue.Type}\t{job.Name}\t{job?.Log?.Url}\t{job.Attempt}\t{issue.Message}");
                                }
                            }
                        }
                    }
                }
            }

            Console.WriteLine($"Writing data to '{s_MetadataFile}'");
            File.WriteAllLines(s_MetadataFile, writeLines);
            Console.WriteLine("Done");
            return 0;
        }

        private static void DisplayUsage()
        {
            Console.WriteLine("Usage: azdot -organization (organization name) -project (project name) [optional parameters]");
            Console.WriteLine("  Optional parameters:");
            Console.WriteLine("    -pat (value)    # Personal authentication token for Azure DevOps");
            Console.WriteLine("    -mindatetime (value)  # earliest date time to return build records from (Default is yesterday");
            Console.WriteLine("    -maxdatetime (value)  # latest date time to return build records from (Default is now)");
            Console.WriteLine("    -outfile (value)      # tsv file to write results to (Default is results.tsv)");
            Console.WriteLine("    -buildreasonfilter (value)  # Azure DevOps build reasons to filter results to (Default is 'IndividualCI,BatchedCI')");
        }
        private static async Task<Build[]> GetBuilds(DateTime minDateTime, DateTime maxDateTime, string buildReasonFilter = null)
        {
            if (!string.IsNullOrWhiteSpace(buildReasonFilter))
            {
                var buildReasons = buildReasonFilter.Split(',');
                return await s_azureServer.ListBuilds(s_Project, minDateTime.ToString(), maxDateTime.ToString(), buildReasons);
            }
            else
            {
                return await s_azureServer.ListBuilds(s_Project, minDateTime.ToString(), maxDateTime.ToString());
            }
        }

        private static async Task<Timeline> DumpTimeline(int buildId)
        {
            var timeline = await s_azureServer.GetTimeline(s_Project, buildId);
            return timeline;
        }
    }
}
