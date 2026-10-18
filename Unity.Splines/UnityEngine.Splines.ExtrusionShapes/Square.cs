using System;
using Unity.Mathematics;

namespace UnityEngine.Splines.ExtrusionShapes;

[Serializable]
public sealed class Square : IExtrudeShape
{
	private static readonly float2[] k_Sides = new float2[4]
	{
		new float2(-0.5f, -0.5f),
		new float2(0.5f, -0.5f),
		new float2(0.5f, 0.5f),
		new float2(-0.5f, 0.5f)
	};

	public int SideCount => 4;

	public float2 GetPosition(float t, int index)
	{
		return k_Sides[index];
	}
}
