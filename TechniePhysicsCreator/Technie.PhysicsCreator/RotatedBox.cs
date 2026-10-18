using UnityEngine;

namespace Technie.PhysicsCreator;

public class RotatedBox
{
	public ConstructionPlane plane;

	public Vector3 localCenter;

	public Vector3 center;

	public Vector3 size;

	public float volume;

	public float VolumeCm3 => volume * 1000000f;

	public RotatedBox(ConstructionPlane p, Vector3 localCenter, Vector3 c, Vector3 s)
	{
		plane = p;
		this.localCenter = localCenter;
		center = c;
		size = s;
		volume = size.x * size.y * size.z;
	}

	public void DrawWireframe()
	{
		Gizmos.matrix = Matrix4x4.TRS(center, plane.rotation, Vector3.one);
		Gizmos.DrawWireCube(Vector3.zero, size);
		Gizmos.matrix = Matrix4x4.identity;
	}
}
