using Octokit;

namespace PullRequestCommentTriggers
{
    public class PullRequestPayload : PullRequestEventPayload
    {
        public Installation Installation { get; set; }
    }
}
