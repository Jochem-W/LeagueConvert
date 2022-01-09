using System.Diagnostics;
using System.Globalization;

namespace Octokit.Extensions.Models;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
public class ActionRun
{
    public ActionRun()
    {
    }

    public ActionRun(string headSha, string htmlUrl)
    {
        HeadSha = headSha;
        HtmlUrl = htmlUrl;
    }
    
    public string HeadSha { get; protected set; }
    
    public string HtmlUrl { get; protected set; }
    
    internal string DebuggerDisplay => string.Format(CultureInfo.InvariantCulture, "HeadSha: {2}", HeadSha);
}