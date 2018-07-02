
using System.IO;
using System.Linq;
using System.Web.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using Microsoft.Extensions.Primitives;
using PullRequestCommentTriggers.Services;
using System.Threading.Tasks;
using System.Net.Http;
using Octokit.Internal;
using System.Collections.Generic;
using System;
using System.Net;

namespace PullRequestCommentTriggers
{
    public static class PullRequestWebHook
    {
        private static readonly string[] PullRequestActions =
        {
            "labeled",
            "unlabeled",
            "opened",
            "edited",
            "reopened",
            "synchronize"
        };

        private static readonly IGithubSettingsProvider SettingsProvider = new DefaultGithubSettingsProvider();

        private static readonly IGithubConnectionCache GithubConnectionCache = new GithubConnectionCache(new GithubAppTokenService(SettingsProvider));

        private static readonly IPullRequestHandler PullRequestHandler =
            new PullRequestHandler(
                new PullRequestInfoProvider(),
                new RepositorySettingsProvider(),
                new WorkInProgressPullRequestPolicy(),
                new CommitStatusWriter(SettingsProvider));

        [FunctionName("PullRequestWebHook")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger("post", WebHookType = "github")]HttpRequestMessage request, TraceWriter log)
        {
            IEnumerable<string> eventNames;
            IEnumerable<string> deliveryIds;
            request.Headers.TryGetValues("X-GitHub-Event", out eventNames);
            request.Headers.TryGetValues("X-GitHub-Delievery", out deliveryIds);

            log.Info($"Webhook delivery: Delivery id = '{deliveryIds.FirstOrDefault()}', Event name = '{eventNames.FirstOrDefault()}'");

            if(eventNames.FirstOrDefault() == "pull_request")
            {
                var payload = await DeserializeBody<PullRequestPayload>(request.Content);
                if(PullRequestActions.ToList().Contains(payload.Action))
                {
                    try
                    {
                        log.Info($"Handling action '{payload.Action}' for pull reuqest #{payload.Number}");
                        var connection = await GithubConnectionCache.GetConnectionAsync((int)payload.Installation.Id);
                        var context = new PullRequestContext(payload, connection, log);
                        await PullRequestHandler.HandleWebhookEventAsync(context);
                    }
                    catch (Exception ex)
                    {
                        log.Error($"Error processing pull request webhook event {deliveryIds.FirstOrDefault()}", ex);
                        return request.CreateErrorResponse(HttpStatusCode.InternalServerError, new HttpError(ex.Message));
                    }
                }
                else
                {
                    log.Info($"Ignoring pull request action '{payload.Action}'");
                }
            }
            else
            {
                log.Info($"Unknown event '{eventNames}', ignoring");
            }

            return request.CreateResponse(HttpStatusCode.NoContent);
        }


        private static async Task<T> DeserializeBody<T>(HttpContent content)
        {
            string json = await content.ReadAsStringAsync();
            var serializer = new SimpleJsonSerializer();
            return serializer.Deserialize<T>(json);
        }
    }
}
