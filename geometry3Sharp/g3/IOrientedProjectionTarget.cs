namespace g3;

public interface IOrientedProjectionTarget : IProjectionTarget
{
	Vector3d Project(Vector3d vPoint, out Vector3d vProjectNormal, int identifier = -1);
}
