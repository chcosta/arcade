
using System.Threading.Tasks;
namespace PullRequestCommentTriggers.Services
{
    internal interface IPullRequestHandler
    {
        Task HandleWebhookEventAsync(PullRequestContext context);
    }
}
