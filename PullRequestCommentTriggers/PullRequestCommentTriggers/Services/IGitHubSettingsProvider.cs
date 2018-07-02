using System;
using System.Collections.Generic;
using System.Text;

namespace PullRequestCommentTriggers.Services
{
    public interface IGithubSettingsProvider
    {
        GithubSettings Settings { get; }
    }
}
