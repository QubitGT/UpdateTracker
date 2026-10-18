namespace g3;

public struct SetGroupBehavior
{
	public enum Modes
	{
		Ignore,
		AutoGenerate,
		UseConstant
	}

	private Modes Mode;

	private int SetGroupID;

	public static SetGroupBehavior Ignore => new SetGroupBehavior(Modes.Ignore);

	public static SetGroupBehavior AutoGenerate => new SetGroupBehavior(Modes.AutoGenerate);

	public SetGroupBehavior(Modes mode, int id = 0)
	{
		Mode = mode;
		SetGroupID = id;
	}

	public int GetGroupID(DMesh3 mesh)
	{
		if (Mode == Modes.Ignore)
		{
			return -1;
		}
		if (Mode == Modes.AutoGenerate)
		{
			return mesh.AllocateTriangleGroup();
		}
		return SetGroupID;
	}

	public static SetGroupBehavior SetTo(int groupID)
	{
		return new SetGroupBehavior(Modes.UseConstant, groupID);
	}
}
