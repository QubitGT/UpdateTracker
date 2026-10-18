namespace Fusion;

public readonly struct NetworkObjectReleaseContext
{
	public readonly NetworkObject Object;

	public readonly NetworkObjectTypeId TypeId;

	public readonly bool IsBeingDestroyed;

	public readonly bool IsNestedObject;

	public NetworkObjectReleaseContext(NetworkObject obj, NetworkObjectTypeId typeId, bool isBeingDestroyed, bool isNested)
	{
		Object = obj;
		IsBeingDestroyed = isBeingDestroyed;
		TypeId = typeId;
		IsNestedObject = isNested;
	}

	public override string ToString()
	{
		return $"[{Object}, TypeId={TypeId}, IsBeingDestroyed={IsBeingDestroyed}, IsNestedObject={IsNestedObject}]";
	}
}
