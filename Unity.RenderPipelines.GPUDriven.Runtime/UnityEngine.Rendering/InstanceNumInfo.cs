namespace UnityEngine.Rendering;

internal struct InstanceNumInfo
{
	public unsafe fixed int InstanceNums[2];

	public unsafe void InitDefault()
	{
		for (int i = 0; i < 2; i++)
		{
			InstanceNums[i] = 0;
		}
	}

	public unsafe InstanceNumInfo(InstanceType type, int instanceNum)
	{
		InitDefault();
		InstanceNums[(int)type] = instanceNum;
	}

	public unsafe InstanceNumInfo(int meshRendererNum = 0, int speedTreeNum = 0)
	{
		InitDefault();
		InstanceNums[0] = meshRendererNum;
		InstanceNums[1] = speedTreeNum;
	}

	public unsafe int GetInstanceNum(InstanceType type)
	{
		return InstanceNums[(int)type];
	}

	public int GetInstanceNumIncludingChildren(InstanceType type)
	{
		int num = GetInstanceNum(type);
		foreach (InstanceType childType in InstanceTypeInfo.GetChildTypes(type))
		{
			num += GetInstanceNumIncludingChildren(childType);
		}
		return num;
	}

	public unsafe int GetTotalInstanceNum()
	{
		int num = 0;
		for (int i = 0; i < 2; i++)
		{
			num += InstanceNums[i];
		}
		return num;
	}
}
