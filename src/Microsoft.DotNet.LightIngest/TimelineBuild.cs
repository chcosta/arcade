using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.DotNet.LightIngest
{
    public class TimelineBuild
    {
        public int BuildId { get; set; }

        public string Status { get; set; }

        public string Result { get; set; }

        public string Repository { get; set; }

        public string Reason { get; set; }

        public string BuildNumber { get; set; }

        public DateTime QueueTime { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime FinishTime { get; set; }
    }
}
