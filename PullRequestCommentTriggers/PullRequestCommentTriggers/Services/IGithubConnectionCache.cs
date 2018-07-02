using System.Threading.Tasks;
using Octokit;

namespace PullRequestCommentTriggers.Services
{
    public interface IGithubConnectionCache
    {
        Task<IConnection> GetConnectionAsync(int installationId);
    }
}
