using System.Diagnostics;
using System.Globalization;

namespace Octokit.Extensions.Models;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
public class ActionRunsResponse
{
    public ActionRunsResponse()
    {
    }

    public ActionRunsResponse(int totalCount, IReadOnlyList<ActionRun> workflowRuns)
    {
        TotalCount = totalCount;
        WorkflowRuns = workflowRuns;
    }

    public int TotalCount { get; protected set; }

    public IReadOnlyList<ActionRun> WorkflowRuns { get; protected set; }

    internal string DebuggerDisplay => string.Format(CultureInfo.CurrentCulture, "TotalCount: {0}, CheckSuites: {1}",
        TotalCount, WorkflowRuns.Count);
}