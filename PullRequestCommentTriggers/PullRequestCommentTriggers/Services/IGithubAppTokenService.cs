using System.Threading.Tasks;

namespace PullRequestCommentTriggers.Services
{
    interface IGithubAppTokenService
    {
        Task<string> GetTokenForApplicationAsync();
        Task<string> GetTokenForInstallationAsync(int installationId);
    }
}
