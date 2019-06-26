using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.DotNet.KustoIngest.ObjectData
{
    public sealed class RecordData
    {
        public int BuildId { get; private set; }
        public string Id { get; private set; }
        public string ParentId { get; private set; }
        public string Name { get; private set; }
        public DateTime StartTime { get; private set; }
        public DateTime FinishTime { get; private set; }
        public string CurrentOperation { get; private set; }
        public string PercentComplete { get; private set; }
        public string Result { get; private set; }
        public string ResultCode { get; private set; }
        public string ChangeId { get; private set; }
        public DateTime LastModified { get; private set; }
        public string WorkerName { get; private set; }
        public int Order { get; private set; }
        public string Details { get; private set; }
        public int ErrorCount { get; private set; }
        public int WarningCount { get; private set; }
        public string Url { get; private set; }
        public string LogId { get; private set; }
        public string LogType { get; private set; }
        public string LogUrl { get; private set; }
        public string TaskId { get; private set; }
        public string TaskName { get; private set; }
        public string TaskVersion { get; private set; }
        public int Attempt { get; private set; }
        public List<IssueData> Issues { get; private set; }

        public RecordData()
        {
            Issues = new List<IssueData>();
        }
        
        public void AddIssue(IssueData data)
        {
            Issues.Add(data);
        }
        public string ToCsvString()
        {
            return string.Join(",", BuildId, Id, ParentId, Name, StartTime, FinishTime, CurrentOperation,
                PercentComplete, Result, ResultCode, ChangeId, LastModified, WorkerName, Order, Details,
                ErrorCount, WarningCount, Url, LogId, LogType, LogUrl, TaskId, TaskName, TaskVersion, Attempt);
        }
        public static RecordData Parse(string line)
        {
            if (!string.IsNullOrWhiteSpace(line))
            {
                var parts = line.Split(new char[] { '\t' });
                if (parts[0].Trim().Equals("record", StringComparison.OrdinalIgnoreCase))
                {

                    RecordData recordData = new RecordData()
                    {
                        BuildId = int.Parse(parts[1]),
                        Id = parts[2],
                        ParentId = parts[3],
                        Name = parts[4],
                        StartTime = DateTime.Parse(parts[5]),
                        FinishTime = DateTime.Parse(parts[6]),
                        CurrentOperation = parts[7],
                        PercentComplete = parts[8],
                        Result = parts[9],
                        ResultCode = parts[10],
                        ChangeId = parts[11],
                        LastModified = DateTime.Parse(parts[12]),
                        WorkerName = parts[13],
                        Order = int.Parse(parts[14]),
                        Details = parts[15],
                        ErrorCount = int.Parse(parts[16]),
                        WarningCount = int.Parse(parts[17]),
                        Url = parts[18],
                        LogId = parts[19],
                        LogType = parts[20],
                        LogUrl = parts[21],
                        TaskId = parts[22],
                        TaskName = parts[23],
                        TaskVersion = parts[24],
                        Attempt = int.Parse(parts[25])
                    };
                    return recordData;
                }
            }
            return null;
        }
    }
}
