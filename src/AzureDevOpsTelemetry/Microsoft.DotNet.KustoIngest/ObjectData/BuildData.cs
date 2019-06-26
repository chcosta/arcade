using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.DotNet.KustoIngest.ObjectData
{
    class BuildData
    {
        public int BuildId { get; private set; }
        public string Status { get; private set; }
        public string Result { get; private set; }
        public string Repository { get; private set; }
        public string Reason { get; private set; }
        public string BuildNumber { get; private set; }
        public DateTime QueueTime { get; private set; }
        public DateTime StartTime { get; private set; }
        public DateTime FinishTime { get; private set; }
        public List<RecordData> Records { get; private set; }

        public BuildData()
        {
            Records= new List<RecordData>();
        }

        public void AddRecord(RecordData data)
        {
            Records.Add(data);
        }
        public string ToCsvString()
        {
            return string.Join(",", BuildId, Status, Result, Repository, Reason, BuildNumber, QueueTime, StartTime, FinishTime);
        }
        public static BuildData Parse(string line)
        {
            if (!string.IsNullOrWhiteSpace(line))
            {
                var parts = line.Split(new char[] { '\t' });
                if (parts[0].Trim().Equals("build", StringComparison.OrdinalIgnoreCase))
                {
                    BuildData buildData = new BuildData()
                    {
                        BuildId = int.Parse(parts[1]),
                        Status = parts[2],
                        Result = parts[3],
                        Repository = parts[4],
                        Reason = parts[5],
                        BuildNumber = parts[6],
                        QueueTime = DateTime.Parse(parts[7]),
                        StartTime = DateTime.Parse(parts[8]),
                        FinishTime = DateTime.Parse(parts[9])
                    };
                    return buildData;
                }
            }
            return null;
        }
    }
}
