using System.Threading.Tasks;

namespace PullRequestCommentTriggers.Services
{
    public interface IRepositorySettingsProvider
    {
        Task<RepositorySettings> GetRepositorySettingsAsync(PullRequestContext context);
    }
}
