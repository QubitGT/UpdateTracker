using System;

[Serializable]
public struct VoxelAction
{
	public OperationType operation;

	public float radius;

	public float strength;

	public byte material;

	public VoxelAction(OperationType operation, float radius, float strength, byte material = 0)
	{
		this.operation = operation;
		this.radius = radius;
		this.strength = strength;
		this.material = material;
	}

	public bool IsValid()
	{
		if (float.IsFinite(radius) && radius > 0f && float.IsFinite(strength) && strength > 0f)
		{
			if (operation != OperationType.Add)
			{
				return operation == OperationType.Subtract;
			}
			return true;
		}
		return false;
	}
}
