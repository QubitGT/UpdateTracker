namespace g3;

public struct EdgeConstraint
{
	private EdgeRefineFlags refineFlags;

	public IProjectionTarget Target;

	public int TrackingSetID;

	public static readonly EdgeConstraint Unconstrained = new EdgeConstraint
	{
		refineFlags = EdgeRefineFlags.NoConstraint,
		TrackingSetID = -1
	};

	public static readonly EdgeConstraint NoFlips = new EdgeConstraint
	{
		refineFlags = EdgeRefineFlags.NoFlip,
		TrackingSetID = -1
	};

	public static readonly EdgeConstraint FullyConstrained = new EdgeConstraint
	{
		refineFlags = EdgeRefineFlags.FullyConstrained,
		TrackingSetID = -1
	};

	public bool CanFlip => (refineFlags & EdgeRefineFlags.NoFlip) == 0;

	public bool CanSplit => (refineFlags & EdgeRefineFlags.NoSplit) == 0;

	public bool CanCollapse => (refineFlags & EdgeRefineFlags.NoCollapse) == 0;

	public bool NoModifications => (refineFlags & EdgeRefineFlags.FullyConstrained) == EdgeRefineFlags.FullyConstrained;

	public bool IsUnconstrained
	{
		get
		{
			if (refineFlags == EdgeRefineFlags.NoConstraint)
			{
				return Target == null;
			}
			return false;
		}
	}

	public EdgeConstraint(EdgeRefineFlags rflags)
	{
		refineFlags = rflags;
		Target = null;
		TrackingSetID = -1;
	}

	public EdgeConstraint(EdgeRefineFlags rflags, IProjectionTarget target)
	{
		refineFlags = rflags;
		Target = target;
		TrackingSetID = -1;
	}
}
