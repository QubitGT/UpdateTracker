using UnityEngine;

namespace Technie.PhysicsCreator;

public class ConstructionPlane
{
	public Vector3 center;

	public Vector3 normal;

	public Vector3 tangent;

	public Quaternion rotation;

	public Matrix4x4 planeToWorld;

	public Matrix4x4 worldToPlane;

	public ConstructionPlane(Vector3 c)
	{
		center = c;
		normal = Vector3.forward;
		tangent = Vector3.up;
		Init();
	}

	public ConstructionPlane(Vector3 c, Vector3 n, Vector3 t)
	{
		center = c;
		normal = n;
		tangent = t;
		Init();
	}

	public ConstructionPlane(ConstructionPlane basePlane, float angle)
	{
		Vector3 vector = Quaternion.AngleAxis(angle, basePlane.normal) * basePlane.tangent;
		center = basePlane.center;
		normal = basePlane.normal;
		tangent = vector;
		Init();
	}

	public ConstructionPlane(ConstructionPlane basePlane, Vector3 positionOffset)
	{
		center = basePlane.center + positionOffset;
		normal = basePlane.normal;
		tangent = basePlane.tangent;
		Init();
	}

	private void Init()
	{
		if (normal.magnitude < 0.01f)
		{
			Debug.LogError("!");
		}
		rotation = Quaternion.LookRotation(normal, tangent);
		planeToWorld = Matrix4x4.TRS(center, rotation, Vector3.one);
		worldToPlane = planeToWorld.inverse;
	}
}
