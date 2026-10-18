using System;
using Unity.Mathematics;

namespace UnityEngine.Splines.ExtrusionShapes;

[Serializable]
public sealed class Road : IExtrudeShape
{
	private static readonly float2[] k_Sides = new float2[4]
	{
		new float2(-0.6f, -0.1f),
		new float2(-0.5f, 0f),
		new float2(0.5f, 0f),
		new float2(0.6f, -0.1f)
	};

	public int SideCount => 3;

	public float2 GetPosition(float t, int index)
	{
		return k_Sides[3 - index];
	}
}
