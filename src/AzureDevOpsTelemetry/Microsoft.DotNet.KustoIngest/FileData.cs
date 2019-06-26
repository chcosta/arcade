using Microsoft.DotNet.AzureDevOpsTelemetry.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.DotNet.KustoIngest
{
    public static class FileData
    {
        public struct Row
        {
            public string Label;
            public Dictionary<string, string> Columns;
            public bool IsDefault { get { return string.IsNullOrWhiteSpace(Label) || Columns == null || Columns.Count == 0; } }

            public static Row Parse(string line)
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    var parts = line.Split(new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);

                    var result = new Row()
                    {
                        Label = parts[0].Trim(),
                        Columns = new Dictionary<string, string>()
                    };

                    string[] schema = null;
                    if (DataSchema.Schemas.TryGetValue(result.Label.ToLower(), out schema))
                    {
                        // a schema was found
                        for (int i = 0; i < schema.Length; i++)
                        {
                            if ((i + 1) < parts.Length) result.Columns.Add(schema[i], parts[i + 1].Trim().Replace("#", ""));
                            else result.Columns.Add(schema[i].Trim(), "");
                        }

                        return result;
                    }
                }
                return default(Row);
            }
        }
    }
}
