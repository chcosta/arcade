using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.DotNet.LightIngest
{
    class MessageData
    {
        public string BuildId { get; set; }
        public string IssueType { get; set; }
        public string Name { get; set; }
        public string LogUrl { get; set; }
        public string Attempt { get; set; }
        public string Message { get; set; }
        public string Order { get; set; }
    }
}
