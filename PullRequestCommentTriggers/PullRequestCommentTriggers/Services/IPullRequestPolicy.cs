using Octokit;

namespace PullRequestCommentTriggers.Services
{
    public interface IPullRequestPolicy
    {
        (CommitState state, string description) GetStatus(PullRequestContext context);
    }
}
