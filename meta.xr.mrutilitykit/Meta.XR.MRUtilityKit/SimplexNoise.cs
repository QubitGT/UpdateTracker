using UnityEngine;

namespace Meta.XR.MRUtilityKit;

internal static class SimplexNoise
{
	internal static Vector3 srdnoise(Vector2 pos, float rot)
	{
		Vector2 vector = pos * 100f;
		Vector2 vector2 = new Vector2(Mathf.FloorToInt(vector.x), Mathf.FloorToInt(vector.y));
		Vector2 lhs = vector - vector2;
		float t = Vector2.Dot(lhs, new Vector2(0.5f, 0.5f));
		Vector2.Dot(lhs, new Vector2(0.70710677f, -0.70710677f));
		float t2 = Vector2.Dot(lhs, new Vector2(-0.70710677f, 0.70710677f));
		Vector2.Dot(lhs, new Vector2(0.5f, -0.5f));
		float a = Mathf.PerlinNoise(vector2.x, vector2.y);
		float b = Mathf.PerlinNoise(vector2.x + 1f, vector2.y);
		float a2 = Mathf.PerlinNoise(vector2.x, vector2.y + 1f);
		float b2 = Mathf.PerlinNoise(vector2.x + 1f, vector2.y + 1f);
		float a3 = Mathf.Lerp(a, b, t);
		float b3 = Mathf.Lerp(a2, b2, t);
		float x = Mathf.Lerp(a3, b3, t2);
		float x2 = Mathf.Cos(rot);
		float y = Mathf.Sin(rot);
		Vector2 vector3 = new Vector2(x2, y);
		Vector2 vector4 = new Vector2(1f, 0f);
		Vector2 vector5 = new Vector2(0f, 1f);
		Vector2 vector6 = vector3.x * vector4 + vector3.y * vector5;
		Vector2 vector7 = vector3.y * vector4 - vector3.x * vector5;
		return new Vector3(x, vector6.x, vector7.x);
	}
}
