using System.Threading.Tasks;
using Octokit;

namespace PullRequestCommentTriggers.Services
{
    public interface ICommitStatusWriter
    {
        Task WriteCommitStatusAsync(PullRequestContext context, CommitState state, string description);
    }
}
