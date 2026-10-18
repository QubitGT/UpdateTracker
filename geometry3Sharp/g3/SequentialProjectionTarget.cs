namespace g3;

public class SequentialProjectionTarget : IProjectionTarget
{
	public IProjectionTarget[] Targets { get; set; }

	public SequentialProjectionTarget()
	{
	}

	public SequentialProjectionTarget(params IProjectionTarget[] targets)
	{
		Targets = targets;
	}

	public Vector3d Project(Vector3d vPoint, int identifier = -1)
	{
		Vector3d vector3d = vPoint;
		for (int i = 0; i < Targets.Length; i++)
		{
			vector3d = Targets[i].Project(vector3d, identifier);
		}
		return vector3d;
	}
}
