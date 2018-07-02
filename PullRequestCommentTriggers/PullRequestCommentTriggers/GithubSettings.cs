using System.Security.Cryptography;
using PullRequestCommentTriggers.Services;

namespace PullRequestCommentTriggers
{
    class GithubSettings
    {
        public GithubSettings(string appId, string privateKey, string statusContext)
        {
            AppId = appId;
            PrivateKey = privateKey;
            StatusContext = statusContext;
            RsaParameters =
                string.IsNullOrEmpty(privateKey)
                ? default
                : CryptoHelper.GetRsaParameters(privateKey);
        }

        public string AppId { get; }
        public string PrivateKey { get; }
        public RSAParameters RsaParameters { get; }
        public string StatusContext { get; set; }
    }
}
