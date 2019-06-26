using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.DotNet.KustoIngest.ObjectData
{
    public sealed class IssueData
    {
        public string RecordId { get; private set; }
        public string Type { get; private set; }
        public string Category { get; private set; }
        public string Message { get; private set; }
        public string DataType { get; private set; }
        public string DataLogFileNumber { get; private set; }
        public string Metadata { get; private set; }

        public string ToCsvString()
        {
            return string.Join(",", RecordId, Type, Category, Message, DataType, DataLogFileNumber, Metadata);
        }
        public static IssueData Parse(string line)
        {
            if (!string.IsNullOrWhiteSpace(line))
            {
                var parts = line.Split(new char[] { '\t' });
                if (parts[0].Trim().Equals("issue", StringComparison.OrdinalIgnoreCase))
                {
                    IssueData issueData = new IssueData()
                    {
                        RecordId = parts[1],
                        Type = parts[2],
                        Category = parts[3],
                        Message = parts[4],
                    };
                    if(parts.Length >= 6)
                    {
                        issueData.DataType = parts[5];
                        issueData.DataLogFileNumber = parts[6];
                    }
                    var regex = System.Text.RegularExpressions.Regex.Match(issueData.Message, "NETCORE_ENGINEERING_TELEMETRY=([^\\)]*)\\)");
                    if (regex.Success)
                    {
                        var hint = regex.Groups[1].Value;
                        issueData.Metadata = hint;
                    }
                    return issueData;
                }
            }
            return null;
        }
    }
}
