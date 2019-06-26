using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.DotNet.AzureDevOpsTelemetry.Shared
{
    public class DataSchema
    {
        public static Dictionary<string /*name*/, string[] /*schema*/> Schemas = new Dictionary<string, string[]>()
        {
            {"build", new string[] { "id", "status", "result", "repository", "reason", "buildnumber", "queuetime", "starttime", "finishtime" } },
            {"record", new string[] { "build_id", "id", "parentid", "name", "starttime", "finishtime", "currentoperation", "percentcomplete", "result", "resultcode", "changeid", "lastmodified", "workername", "order", "details", "errorcount", "warningcount", "url", "logid", "logtype", "logurl", "taskid", "taskname", "taskversion", "attempt" } },
            {"issue", new string[] { "record_id", "type", "category", "message", "datatype", "datalogfilelinenumber"} }
        };
    }
}
