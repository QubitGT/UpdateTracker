namespace g3;

public struct VertexConstraint
{
	public bool Fixed;

	public int FixedSetID;

	public IProjectionTarget Target;

	public const int InvalidSetID = -1;

	public static readonly VertexConstraint Unconstrained = new VertexConstraint
	{
		Fixed = false,
		FixedSetID = -1,
		Target = null
	};

	public static readonly VertexConstraint Pinned = new VertexConstraint
	{
		Fixed = true,
		FixedSetID = -1,
		Target = null
	};

	public VertexConstraint(bool isFixed, int setID = -1)
	{
		Fixed = isFixed;
		FixedSetID = setID;
		Target = null;
	}

	public VertexConstraint(IProjectionTarget target)
	{
		Fixed = false;
		FixedSetID = -1;
		Target = target;
	}
}
