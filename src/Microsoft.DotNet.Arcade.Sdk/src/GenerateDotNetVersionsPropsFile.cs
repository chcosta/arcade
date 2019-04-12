using Microsoft.Build.Construction;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using NuGet.Versioning;
using System.IO;
using System.Text.Json;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Build.Evaluation;

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
        public string VersionsPropsPath { get; set; }

        [Required]
        public string GlobalJsonPath { get; set; }

        public override bool Execute()
        {
            System.Diagnostics.Debugger.Launch();

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
                        var runtimeItems = new Dictionary<string, IEnumerable<KeyValuePair<string, string>>>();
                        foreach (var dotnetLocalChildren in dotnetLocalElement.EnumerateObject())
                        {
                            // Parses 
                            // { "tools": { 
                            //     "dotnet-local": {
                            //       "runtimes": 
                            //         "dotnet": []
                            //         "aspnetcore": []
                            // } } }

                            /*
                             * dotnet:
                             *   architecture: x64
                             *   version: 1.1.0
                             */
                            if (dotnetLocalChildren.Name == "runtimes")
                            {
                                foreach (var runtime in dotnetLocalChildren.Value.EnumerateObject())
                                {
                                    runtimeItems.Add(runtime.Name, GetItemsFromJsonElementArray(runtime.Value.EnumerateObject()));
                                }
                            }
                        }
                        if (runtimeItems.Count > 0)
                        {
                            System.Linq.ILookup<string, ProjectProperty> properties = null;
                            if (File.Exists(VersionsPropsPath))
                            {
                                var proj = Project.FromFile(VersionsPropsPath, new Build.Definition.ProjectOptions());
                                properties = proj.AllEvaluatedProperties.ToLookup(p => p.Name);
                            }
                            
                            foreach(var runtimeItem in runtimeItems)
                            {
                                System.Console.WriteLine($"{runtimeItem.Key}:");
                                foreach(var item in runtimeItem.Value)
                                {
                                    if (SemanticVersion.TryParse(item.Key, out SemanticVersion version))
                                    {
                                        System.Console.WriteLine($"- {item.Key}, {item.Value}");
                                    }
                                    else
                                    {
                                        System.Console.WriteLine($"- {item.Key} == {properties[item.Key].FirstOrDefault().EvaluatedValue}, {item.Value}");
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return true;
        }

        private IEnumerable<KeyValuePair<string, string>> GetItemsFromJsonElementArray(JsonElement.ObjectEnumerator tokens)
        {
            var items = new List<KeyValuePair<string, string>>();

            foreach(var architecture in tokens)
            {
                foreach (var version in architecture.Value.EnumerateArray())
                {
                    items.Add(new KeyValuePair<string, string>(version.GetString(), architecture.Name));
                }
            }
            return items.ToArray();
        }
    }
}
