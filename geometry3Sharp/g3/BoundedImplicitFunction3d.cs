namespace g3;

public interface BoundedImplicitFunction3d : ImplicitFunction3d
{
	AxisAlignedBox3d Bounds();
}
