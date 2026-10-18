using Meta.XR.MRUtilityKit.Extensions;
using UnityEngine;

namespace Meta.XR.MRUtilityKit;

internal static class WorleyNoise
{
	private const float K = 1f / 7f;

	private const float Ko = 0.42857143f;

	private const float jitter = 1f;

	private static Vector2 mod289(Vector2 v)
	{
		return new Vector2(v.x - Mathf.Floor(v.x * 0.0034602077f * 289f), v.y - Mathf.Floor(v.y * 0.0034602077f * 289f));
	}

	private static Vector3 mod289(Vector3 v)
	{
		return new Vector3(v.x - Mathf.Floor(v.x * 0.0034602077f * 289f), v.y - Mathf.Floor(v.y * 0.0034602077f * 289f), v.z - Mathf.Floor(v.z * 0.0034602077f * 289f));
	}

	private static Vector3 permute(Vector3 x)
	{
		return mod289(Vector3.Scale(new Vector3(x.x * 34f + 1f, x.y * 34f + 1f, x.z * 34f + 1f), x));
	}

	private static float mod7(float v)
	{
		return v - Mathf.Floor(v / 7f) * 7f;
	}

	private static Vector3 mod7(Vector3 v)
	{
		return new Vector3(v.x - Mathf.Floor(v.x / 7f) * 7f, v.y - Mathf.Floor(v.y / 7f) * 7f, v.z - Mathf.Floor(v.z / 7f) * 7f);
	}

	internal static Vector2 cellular(Vector2 P)
	{
		Vector2 vector = mod289(P.Floor());
		Vector2 vector2 = P - P.Floor();
		Vector3 a = new Vector3(-1f, 0f, 1f);
		Vector3 a2 = new Vector3(-0.5f, 0.5f, 1.5f);
		Vector3 vector3 = permute(a.Add(vector.x));
		Vector3 vector4 = permute(a.Add(vector3.x).Add(vector.y));
		Vector3 vector5 = mod289(vector4 * (1f / 7f)).Subtract(0.42857143f);
		Vector3 vector6 = (mod7(vector4 * (1f / 7f)).Floor() * (1f / 7f)).Subtract(0.42857143f);
		Vector3 vector7 = vector5 * (vector2.x + 0.5f + 1f);
		Vector3 vector8 = a2.Subtract(vector2.y) + 1f * vector6;
		Vector3 lhs = Vector3.Scale(vector7, vector7) + Vector3.Scale(vector8, vector8);
		Vector3 vector9 = permute(a.Add(vector3.y + vector.y));
		vector5 = mod289(vector9 * (1f / 7f)).Subtract(0.42857143f);
		vector6 = (mod7(vector9 * (1f / 7f)).Floor() * (1f / 7f)).Subtract(0.42857143f);
		Vector3 vector10 = vector5 * (vector2.x - 0.5f + 1f);
		vector8 = Vector3.Scale(vector6, a2.Subtract(vector2.y)).Add(1f);
		Vector3 rhs = Vector3.Scale(vector10, vector10) + Vector3.Scale(vector8, vector8);
		Vector3 vector11 = permute(a.Add(vector3.z + vector.y));
		vector5 = mod289(vector11 * (1f / 7f)).Subtract(0.42857143f);
		vector6 = mod7(vector11.Floor() * (1f / 7f) * (1f / 7f)).Subtract(0.42857143f);
		Vector3 vector12 = vector5 * (vector2.x - 1.5f + 1f);
		vector8 = Vector3.Scale(vector6, a2.Subtract(vector2.y).Add(1f));
		Vector3 rhs2 = Vector3.Scale(vector12, vector12) + Vector3.Scale(vector8, vector8);
		Vector3 lhs2 = Vector3.Min(lhs, rhs);
		rhs = Vector3.Max(lhs, rhs);
		rhs = Vector3.Min(rhs, rhs2);
		lhs = Vector3.Min(lhs2, rhs);
		rhs = Vector3.Max(lhs2, rhs);
		lhs.x = ((lhs.x < lhs.y) ? lhs.x : lhs.y);
		lhs.y = ((lhs.x < lhs.y) ? lhs.y : lhs.x);
		lhs.x = ((lhs.x < lhs.z) ? lhs.x : lhs.z);
		lhs.z = ((lhs.x < lhs.z) ? lhs.z : lhs.x);
		lhs.y = Mathf.Min(lhs.y, rhs.y);
		lhs.z = Mathf.Min(lhs.z, rhs.z);
		lhs.y = Mathf.Min(lhs.y, lhs.z);
		lhs.y = Mathf.Min(lhs.y, rhs.x);
		return new Vector2(Mathf.Sqrt(lhs.x), Mathf.Sqrt(lhs.y));
	}
}
