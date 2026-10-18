namespace UnityEngine.Rendering;

internal struct GPUInstanceComponentDesc
{
	public int propertyID;

	public int byteSize;

	public bool isOverriden;

	public bool isPerInstance;

	public InstanceType instanceType;

	public InstanceComponentGroup componentGroup;

	public GPUInstanceComponentDesc(int inPropertyID, int inByteSize, bool inIsOverriden, bool inPerInstance, InstanceType inInstanceType, InstanceComponentGroup inComponentType)
	{
		propertyID = inPropertyID;
		byteSize = inByteSize;
		isOverriden = inIsOverriden;
		isPerInstance = inPerInstance;
		instanceType = inInstanceType;
		componentGroup = inComponentType;
	}
}
