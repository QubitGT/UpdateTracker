namespace g3;

public class ImplicitBox3d : BoundedImplicitFunction3d, ImplicitFunction3d
{
	private Box3d box;

	private AxisAlignedBox3d local_aabb;

	private AxisAlignedBox3d bounds_aabb;

	public Box3d Box
	{
		get
		{
			return box;
		}
		set
		{
			box = value;
			local_aabb = new AxisAlignedBox3d(0.0 - Box.Extent.x, 0.0 - Box.Extent.y, 0.0 - Box.Extent.z, Box.Extent.x, Box.Extent.y, Box.Extent.z);
			bounds_aabb = box.ToAABB();
		}
	}

	public double Value(ref Vector3d pt)
	{
		double x = (pt - Box.Center).Dot(Box.AxisX);
		double y = (pt - Box.Center).Dot(Box.AxisY);
		double z = (pt - Box.Center).Dot(Box.AxisZ);
		return local_aabb.SignedDistance(new Vector3d(x, y, z));
	}

	public AxisAlignedBox3d Bounds()
	{
		return bounds_aabb;
	}
}
