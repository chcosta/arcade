using Microsoft.Build.Construction;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System.IO;
using System.Text.Json;
using System.Text;
using System.Linq;
using System.Collections.Generic;

namespace Microsoft.DotNet.Arcade.Sdk
{
#if NET472

    [LoadInSeparateAppDomain]
    public class GenerateDotNetVersionsPropsFile : AppDomainIsolatedTask
    {
        static GenerateDotNetVersionsPropsFile() => AssemblyResolution.Initialize();
#else
    public class GenerateDotNetVersionsPropsFile : Task
    {
#endif
        [Required]
        public string DotNetVersionPropsPath { get; set; }

        [Required]
        public string GlobalJsonPath { get; set; }

        private static readonly string s_RuntimeItemGroupName = "DotNetCoreRuntimeVersion";

        public override bool Execute()
        {
            if(File.Exists(DotNetVersionPropsPath))
            {
                Log.LogMessage($"Generated file {DotNetVersionPropsPath} already exists, exiting");
                return true;
            }

            if(!File.Exists(GlobalJsonPath))
            {
                Log.LogWarning($"Unable to find global.json file '{GlobalJsonPath} exiting");
                return true;
            }

            // Documentation says that UTF8 is best for performance - https://github.com/dotnet/corefx/tree/master/src/System.Text.Json/porting_guide#use-text-encoded-as-utf-8-for-best-performance
            var jsonContent = File.ReadAllText(GlobalJsonPath);
            var bytes = Encoding.UTF8.GetBytes(jsonContent);

            using (JsonDocument jsonDocument = JsonDocument.Parse(bytes))
            {
                if (jsonDocument.RootElement.TryGetProperty("tools", out JsonElement toolsElement))
                {
                    if (toolsElement.TryGetProperty("dotnet-local", out JsonElement dotnetLocalElement))
                    {
                        var runtimeItems = new List<KeyValuePair<string, IEnumerable<KeyValuePair<string, string>>>>();
                        foreach (var dotnetLocalChildren in dotnetLocalElement.EnumerateObject())
                        {
                            // Parses '{ "tools": { "dotnet-local": { "runtimes": [] } } }'
                            if (dotnetLocalChildren.Name == "runtimes")
                            {
                                runtimeItems.AddRange(GetItemsFromJsonElementArray(dotnetLocalChildren.Value.EnumerateArray()));
                            }
                            else
                            {
                                // Parses '{ "tools": { "dotnet-local": { "x64": { "runtimes": [] } } } }'
                                if (dotnetLocalChildren.Value.TryGetProperty("runtimes", out JsonElement runtimes))
                                {
                                    runtimeItems.AddRange(GetItemsFromJsonElementArray(runtimes.EnumerateArray(), dotnetLocalChildren.Name));
                                }
                            }
                        }
                        if (runtimeItems.Count > 0)
                        {
                            Log.LogMessage($"Generating file {DotNetVersionPropsPath}");
                            StringBuilder versionPropsContent = new StringBuilder();
                            var project = ProjectRootElement.Create();
                            var itemGroup = project.AddItemGroup();

                            foreach (var item in runtimeItems)
                            {
                                itemGroup.AddItem(s_RuntimeItemGroupName, item.Key, item.Value);
                            }
                            project.Save(DotNetVersionPropsPath, Encoding.UTF8);
                        }
                    }
                }
            }
            return true;
        }

        private IEnumerable<KeyValuePair<string, IEnumerable<KeyValuePair<string, string>>>> GetItemsFromJsonElementArray(JsonElement.ArrayEnumerator tokens, string architecture = null)
        {
            var items = new Dictionary<string, IEnumerable<KeyValuePair<string, string>>>();
            var metadata = new KeyValuePair<string, string>();
            if(architecture != null)
            {
                metadata = new KeyValuePair<string, string>("architecture", architecture);
            }

            foreach(var property in tokens)
            {
                items.Add(property.GetString(), architecture != null ? new KeyValuePair<string, string>[] { metadata } : null);
            }
            return items.ToArray();
        }
    }
}
