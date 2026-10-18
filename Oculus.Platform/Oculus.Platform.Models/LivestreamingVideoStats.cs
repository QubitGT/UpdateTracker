using System;

namespace Oculus.Platform.Models;

public class LivestreamingVideoStats
{
	public readonly int CommentCount;

	public readonly int ReactionCount;

	public readonly string TotalViews;

	public LivestreamingVideoStats(IntPtr o)
	{
		CommentCount = CAPI.ovr_LivestreamingVideoStats_GetCommentCount(o);
		ReactionCount = CAPI.ovr_LivestreamingVideoStats_GetReactionCount(o);
		TotalViews = CAPI.ovr_LivestreamingVideoStats_GetTotalViews(o);
	}
}
