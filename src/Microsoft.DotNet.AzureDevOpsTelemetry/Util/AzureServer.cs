using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.DotNet.AzureDevOpsTelemetry.Util
{
    public sealed class AzureServer
    {
        public string Organization { get; }

        private string _personalAccessToken;

        public AzureServer(string organization, string personalAccessToken = null)
        {
            Organization = organization;
            _personalAccessToken = personalAccessToken;
        }

        /// <summary>
        /// https://docs.microsoft.com/en-us/rest/api/azure/devops/build/builds/list?view=azure-devops-rest-5.0
        /// </summary>
        public async Task<JsonResult> ListBuildsRaw(string project, string continuationToken, string minDateTime, string maxDateTime, string buildReason)
        {
            var builder = GetProjectApiRootBuilder(project);
            builder.Append("/build/builds?");
            builder.Append($"continuationToken={continuationToken}&");
            builder.Append($"queryOrder=finishTimeDescending&");

            if (! string.IsNullOrEmpty(minDateTime))
            {
                builder.Append($"minTime={minDateTime}&");
            }
            if (!string.IsNullOrEmpty(maxDateTime))
            {
                builder.Append($"maxTime={maxDateTime}&");
            }
            if(!string.IsNullOrEmpty(buildReason))
            {
                builder.Append($"reasonFilter={buildReason}&");
            }
            builder.Append("api-version=5.0");
            return await GetJsonResult(builder.ToString());
        }

        /// <summary>
        /// https://docs.microsoft.com/en-us/rest/api/azure/devops/build/builds/list?view=azure-devops-rest-5.0
        /// </summary>
        public async Task<Build[]> ListBuilds(string project, string minDateTime, string maxDateTime, string [] buildReasons)
        {
            List<Build> buildList = new List<Build>();
            foreach(var buildReason in buildReasons)
            {
                buildList.AddRange(await ListBuilds(project, minDateTime, maxDateTime, buildReason));
            }

            return buildList.ToArray();
        }

        public async Task<Build[]> ListBuilds(string project, string minDateTime, string maxDateTime, string buildReason = null)
        {
            List<Build> buildList = new List<Build>();
            string continuationToken = null;
            Build[] builds;
            do
            {
                var result = await ListBuildsRaw(project, continuationToken, minDateTime, maxDateTime, buildReason);
                continuationToken = result.ContinuationToken;
                var root = JObject.Parse(result.Body);
                var array = (JArray)root["value"];
                builds = array.ToObject<Build[]>();
                buildList.AddRange(builds);
            }
            while (continuationToken != null);
            return buildList.ToArray();
        }

        public async Task<JsonResult> GetBuildLogsRaw(string project, int buildId)
        {
            var builder = GetProjectApiRootBuilder(project);
            builder.Append($"/build/builds/{buildId}/logs?api-version=5.0");
            return await GetJsonResult(builder.ToString());
        }

        public async Task<BuildLog[]> GetBuildLogs(string project, int buildId)
        {
            var result = await GetBuildLogsRaw(project, buildId);
            var root = JObject.Parse(result.Body);
            var array = (JArray)root["value"];
            return array.ToObject<BuildLog[]>();
        }

        public async Task<string> GetBuildLog(string project, int buildId, int logId, int? startLine = null, int? endLine = null)
        {
            var builder = GetProjectApiRootBuilder(project);
            builder.Append($"/build/builds/{buildId}/logs/{logId}?");

            var first = true;
            if (startLine.HasValue)
            {
                builder.Append($"startLine={startLine}");
                first = false;
            }

            if (endLine.HasValue)
            {
                if (!first)
                {
                    builder.Append("&");
                }

                builder.Append($"endLine={endLine}");
                first = false;
            }

            if (!first)
            {
                builder.Append("&");
            }

            builder.Append("api-version=5.0");
            using (var client = new HttpClient())
            {
                using (var response = await client.GetAsync(builder.ToString()))
                {
                    response.EnsureSuccessStatusCode();
                    string responseBody = await response.Content.ReadAsStringAsync();
                    return responseBody;
                }
            }
        }

        public async Task<string> GetTimelineRaw(string project, int buildId)
        {
            var builder = GetProjectApiRootBuilder(project);
            builder.Append($"/build/builds/{buildId}/timeline?api-version=5.0");
            return (await GetJsonResult(builder.ToString())).Body;
        }

        public async Task<string> GetTimelineRaw(string project, int buildId, string timelineId, int? changeId = null)
        {
            var builder = GetProjectApiRootBuilder(project);
            builder.Append($"/build/builds/{buildId}/timeline/{timelineId}?");

            if (changeId.HasValue)
            {
                builder.Append($"changeId={changeId}&");
            }

            return (await GetJsonResult(builder.ToString())).Body;
        }

        public async Task<Timeline> GetTimeline(string project, int buildId)
        {
            var json = await GetTimelineRaw(project, buildId);
            return JsonConvert.DeserializeObject<Timeline>(json);
        }

        public async Task<Timeline> GetTimeline(string project, int buildId, string timelineId, int? changeId = null)
        {
            var json = await GetTimelineRaw(project, buildId, timelineId, changeId);
            return JsonConvert.DeserializeObject<Timeline>(json);
        }

        public async Task<string> ListArtifactsRaw(string project, int buildId)
        {
            var builder = GetProjectApiRootBuilder(project);
            builder.Append($"/build/builds/{buildId}/artifacts?api-version=5.0");
            return (await GetJsonResult(builder.ToString())).Body;
        }

        public async Task<BuildArtifact[]> ListArtifacts(string project, int buildId)
        {
            var root = JObject.Parse(await ListArtifactsRaw(project, buildId));
            var array = (JArray)root["value"];
            return array.ToObject<BuildArtifact[]>();
        }

        private StringBuilder GetProjectApiRootBuilder(string project)
        {
            var builder = new StringBuilder();
            builder.Append($"https://dev.azure.com/{Organization}/{project}/_apis");
            return builder;
        }

        private async Task<JsonResult> GetJsonResult(string uri)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Add(
                    new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                if (_personalAccessToken != null)
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                        Convert.ToBase64String(
                            System.Text.ASCIIEncoding.ASCII.GetBytes(
                                string.Format("{0}:{1}", "", _personalAccessToken))));
                }

                using (var response = await client.GetAsync(uri))
                {
                    response.EnsureSuccessStatusCode();
                    string responseBody = await response.Content.ReadAsStringAsync();
                    IEnumerable<string> continuationTokenHeaders;
                    response.Headers.TryGetValues("x-ms-continuationtoken", out continuationTokenHeaders);
                    var continuationToken = continuationTokenHeaders?.FirstOrDefault();
                    JsonResult result = new JsonResult(responseBody, continuationToken);
                    
                    return result;
                }
            }
        }
    }

    public sealed class JsonResult
    {
        public string Body { get; }
        public string ContinuationToken { get; }

        public JsonResult(string body) : this(body, null) { }

        public JsonResult(string body, string continuationToken)
        {
            Body = body;
            ContinuationToken = continuationToken;
        }
    }
}
