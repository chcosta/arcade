
using System.Threading.Tasks;

namespace PullRequestCommentTriggers.Services
{
    public interface IPullRequestInfoProvider
    {
        Task<PullRequestInfo> GetPullRequestInfoAsync(PullRequestContext context);
    }
}
