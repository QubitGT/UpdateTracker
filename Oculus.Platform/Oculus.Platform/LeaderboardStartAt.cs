using System.ComponentModel;

namespace Oculus.Platform;

public enum LeaderboardStartAt
{
	[Description("TOP")]
	Top,
	[Description("CENTERED_ON_VIEWER")]
	CenteredOnViewer,
	[Description("CENTERED_ON_VIEWER_OR_TOP")]
	CenteredOnViewerOrTop,
	[Description("UNKNOWN")]
	Unknown
}
