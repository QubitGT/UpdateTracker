using System;

namespace Oculus.Interaction;

[Obsolete("Use HandDebugGizmos instead.")]
public class HandDebugVisual : HandDebugGizmos
{
	[Obsolete("This method has been deprecated.", true)]
	public void UpdateSkeleton()
	{
		throw new NotImplementedException();
	}
}
