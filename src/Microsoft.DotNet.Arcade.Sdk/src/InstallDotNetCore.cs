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
using System.Diagnostics;

namespace Microsoft.DotNet.Arcade.Sdk
{
#if NET472

    [LoadInSeparateAppDomain]
    public class InstallDotNetCore : AppDomainIsolatedTask
    {
        static InstallDotNetCore() => AssemblyResolution.Initialize();
#else
    public class InstallDotNetCore : Task
    {
#endif
        public string VersionsPropsPath { get; set; }

        [Required]
        public string DotNetInstallScript { get; set; }
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
            if (!File.Exists(DotNetInstallScript))
            {
                Log.LogError($"Unable to find dotnet install script '{DotNetInstallScript} exiting");
                return !Log.HasLoggedErrors;

            }

            // Documentation says that UTF8 is best for performance - https://github.com/dotnet/corefx/tree/master/src/System.Text.Json/porting_guide#use-text-encoded-as-utf-8-for-best-performance
            var jsonContent = File.ReadAllText(GlobalJsonPath);
            var bytes = Encoding.UTF8.GetBytes(jsonContent);

            using (JsonDocument jsonDocument = JsonDocument.Parse(bytes))
            {
                if (jsonDocument.RootElement.TryGetProperty("tools", out JsonElement toolsElement))
                {
                    if (toolsElement.TryGetProperty("dotnet", out JsonElement dotnetLocalElement))
                    {
                        var runtimeItems = new Dictionary<string, IEnumerable<KeyValuePair<string, string>>>();
                        foreach (var dotnetLocalChildren in dotnetLocalElement.EnumerateObject())
                        {
                            // Parses 
                            // { "tools": { 
                            //     "dotnet": {
                            //       "runtimes": 
                            //         "dotnet/arch": []
                            //         "aspnetcore/arch": []
                            // } } }

                            /*
                             * dotnet:
                             *   1.1.0: x64
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
                            // Only load Versions.props if there's a need to look for a version identifier.
                            if (runtimeItems.SelectMany(r => r.Value).Select(r => r.Key).FirstOrDefault(f => !SemanticVersion.TryParse(f, out SemanticVersion version)) != null)
                            {
                                if (!File.Exists(VersionsPropsPath))
                                {
                                    Log.LogError($"Found placeholder identifier in {GlobalJsonPath} but unable to find translation file {VersionsPropsPath}");
                                    return !Log.HasLoggedErrors;
                                }
                                else
                                {
                                    var proj = Project.FromFile(VersionsPropsPath, new Build.Definition.ProjectOptions());
                                    properties = proj.AllEvaluatedProperties.ToLookup(p => p.Name);
                                }
                            }
                            
                            foreach(var runtimeItem in runtimeItems)
                            {
                                System.Console.WriteLine($"{runtimeItem.Key}:");
                                foreach(var item in runtimeItem.Value)
                                {
                                    SemanticVersion version = null;
                                    if (SemanticVersion.TryParse(item.Key, out version))
                                    {
                                        System.Console.WriteLine($"- {item.Key}, {item.Value}");
                                    }
                                    else
                                    {
                                        string evaluatedValue = properties[item.Key].FirstOrDefault().EvaluatedValue;
                                        if (!SemanticVersion.TryParse(evaluatedValue, out version))
                                        {
                                            Log.LogError($"Unable to parse {version} from identifier '{item.Key}'");
                                        }
                                        System.Console.WriteLine($"- {item.Key} == {properties[item.Key].FirstOrDefault().EvaluatedValue}, {item.Value}");
                                    }
                                    if(version != null)
                                    {
                                        string arguments = $"-runtime \"{runtimeItem.Key}\" -version \"{version.ToNormalizedString()}\"";
                                        if (!string.IsNullOrWhiteSpace(item.Value))
                                        {
                                            arguments += " -architecture {item.Value}";
                                        }
                                        var process = Process.Start(new ProcessStartInfo()
                                        {
                                            FileName = DotNetInstallScript,
                                            Arguments = arguments,
                                            UseShellExecute = false
                                        });
                                        process.WaitForExit();
                                        if(process.ExitCode != 0)
                                        {
                                            Log.LogError("dotnet-install failed");
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return !Log.HasLoggedErrors;
        }

        private IEnumerable<KeyValuePair<string, string>> GetItemsFromJsonElementArray(JsonElement.ObjectEnumerator tokens)
        {
            var items = new List<KeyValuePair<string, string>>();

            foreach(var token in tokens)
            {
                string runtime = token.Name;
                string architecture = string.Empty;
                if(runtime.Contains('/'))
                {
                    var parts = runtime.Split(new char[] { '/' }, 2);
                    runtime = parts[0];
                    architecture = parts[1];
                }
                foreach (var version in token.Value.EnumerateArray())
                {
                    items.Add(new KeyValuePair<string, string>(version.GetString(), architecture));
                }
            }
            return items.ToArray();
        }
    }
}
