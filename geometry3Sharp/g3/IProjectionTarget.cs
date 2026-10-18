namespace g3;

public interface IProjectionTarget
{
	Vector3d Project(Vector3d vPoint, int identifier = -1);
}
