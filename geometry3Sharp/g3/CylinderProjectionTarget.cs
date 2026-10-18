namespace g3;

public class CylinderProjectionTarget : IProjectionTarget
{
	public Cylinder3d Cylinder;

	public Vector3d Project(Vector3d vPoint, int identifer = -1)
	{
		DistPoint3Cylinder3 distPoint3Cylinder = new DistPoint3Cylinder3(vPoint, Cylinder);
		distPoint3Cylinder.GetSquared();
		return distPoint3Cylinder.CylinderClosest;
	}
}
