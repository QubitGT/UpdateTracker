using System.Collections.Generic;
using Unity.Collections;

namespace UnityEngine.Rendering;

internal static class InstanceTypeInfo
{
	public const int kInstanceTypeBitCount = 1;

	public const int kMaxInstanceTypesCount = 2;

	public const uint kInstanceTypeMask = 1u;

	private static InstanceType[] s_ParentTypes;

	private static List<InstanceType>[] s_ChildTypes;

	static InstanceTypeInfo()
	{
		InitParentTypes();
		InitChildTypes();
		ValidateTypeRelationsAreCorrectlySorted();
	}

	private static void InitParentTypes()
	{
		s_ParentTypes = new InstanceType[2];
		s_ParentTypes[0] = InstanceType.MeshRenderer;
		s_ParentTypes[1] = InstanceType.MeshRenderer;
	}

	private static void InitChildTypes()
	{
		s_ChildTypes = new List<InstanceType>[2];
		for (int i = 0; i < 2; i++)
		{
			s_ChildTypes[i] = new List<InstanceType>();
		}
		for (int j = 0; j < 2; j++)
		{
			InstanceType instanceType = (InstanceType)j;
			InstanceType instanceType2 = s_ParentTypes[(int)instanceType];
			if (instanceType != instanceType2)
			{
				s_ChildTypes[(int)instanceType2].Add(instanceType);
			}
		}
	}

	private static InstanceType GetMaxChildTypeRecursively(InstanceType type)
	{
		InstanceType instanceType = type;
		foreach (InstanceType item in s_ChildTypes[(int)type])
		{
			instanceType = (InstanceType)Mathf.Max((int)instanceType, (int)GetMaxChildTypeRecursively(item));
		}
		return instanceType;
	}

	private static void FlattenChildInstanceTypes(InstanceType instanceType, NativeList<InstanceType> instanceTypes)
	{
		instanceTypes.Add(in instanceType);
		foreach (InstanceType item in s_ChildTypes[(int)instanceType])
		{
			FlattenChildInstanceTypes(item, instanceTypes);
		}
	}

	private static void ValidateTypeRelationsAreCorrectlySorted()
	{
		NativeList<InstanceType> instanceTypes = new NativeList<InstanceType>(2, Allocator.Temp);
		for (int i = 0; i < 2; i++)
		{
			InstanceType instanceType = (InstanceType)i;
			if (instanceType == s_ParentTypes[i])
			{
				FlattenChildInstanceTypes(instanceType, instanceTypes);
			}
		}
		for (int j = 0; j < instanceTypes.Length; j++)
		{
		}
	}

	public static InstanceType GetParentType(InstanceType type)
	{
		return s_ParentTypes[(int)type];
	}

	public static List<InstanceType> GetChildTypes(InstanceType type)
	{
		return s_ChildTypes[(int)type];
	}
}
