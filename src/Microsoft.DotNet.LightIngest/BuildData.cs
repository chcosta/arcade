using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.DotNet.LightIngest
{
    class BuildData
    {
        public string BuildId { get; set; }
        public string Status { get; set; }
        public string Result { get; set; }
        public string Repository { get; set; }
        public string Reason { get; set; }
        public string BuildNumber { get; set; }
        public string QueueTime { get; set; }
        public string StartTime { get; set; }
        public string FinishTime { get; set; }
        public List<MessageData> Message { get; set; }

        public BuildData()
        {
            Message = new List<MessageData>();
        }

        public static BuildData Parse(string line)
        {
            if (!string.IsNullOrWhiteSpace(line))
            {
                var parts = line.Split(new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);

            }
        }
    }
}
