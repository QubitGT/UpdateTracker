using System;
using System.Diagnostics;
using Meta.XR.EnvironmentDepth;
using UnityEngine;

namespace Meta.XR;

internal static class EnvironmentDepthManagerRaycastExtensions
{
	private const Eye DefaultEye = Eye.Both;

	internal const float MinXYSize = 0.05f;

	private static EnvironmentDepthRaycaster _depthRaycast;

	public static bool Raycast(this EnvironmentDepthManager depthManager, Ray ray, out DepthRaycastHit hitInfo, float maxDistance = 100f, Eye eye = Eye.Both, bool reconstructNormal = true, bool allowOccludedRayOrigin = true)
	{
		EnsureDepthRaycastComponentIsPresent(depthManager);
		if (reconstructNormal)
		{
			Vector3 position;
			Vector3 normal;
			float normalConfidence;
			DepthRaycastResult depthRaycastResult = _depthRaycast.Raycast(ray, out position, out normal, out normalConfidence, maxDistance, eye, allowOccludedRayOrigin);
			hitInfo = new DepthRaycastHit
			{
				result = depthRaycastResult,
				point = position,
				normal = normal,
				normalConfidence = normalConfidence
			};
			return depthRaycastResult == DepthRaycastResult.Success;
		}
		(DepthRaycastResult, Vector3, int) tuple = _depthRaycast.Raycast(ray, maxDistance, eye, allowOccludedRayOrigin);
		hitInfo = new DepthRaycastHit
		{
			result = tuple.Item1,
			point = tuple.Item2
		};
		return tuple.Item1 == DepthRaycastResult.Success;
	}

	public static void SetRaycastWarmUpEnabled(this EnvironmentDepthManager depthManager, bool value)
	{
		EnsureDepthRaycastComponentIsPresent(depthManager);
		_depthRaycast._warmUpRaycast = value;
	}

	private static void EnsureDepthRaycastComponentIsPresent(EnvironmentDepthManager depthManager)
	{
		if (_depthRaycast == null)
		{
			_depthRaycast = depthManager.gameObject.AddComponent<EnvironmentDepthRaycaster>();
			depthManager.onDepthTextureUpdate += _depthRaycast.OnDepthTextureUpdate;
			_depthRaycast.depthManager = depthManager;
		}
	}

	public static bool PlaceBox(this IEnvironmentRaycastProvider provider, Ray ray, Vector3 boxSize, Vector3 upwards, out EnvironmentRaycastHit hit, float maxDistance = 100f)
	{
		if (boxSize.x < 0.05f || boxSize.y < 0.05f)
		{
			UnityEngine.Debug.LogWarning(string.Format("'x' and 'y' components of the '{0}' should be greater than {1} to determine the surface normal.", "boxSize", 0.05f));
			hit = new EnvironmentRaycastHit
			{
				status = EnvironmentRaycastHitStatus.NoHit
			};
			return false;
		}
		if (boxSize.z < 0f)
		{
			UnityEngine.Debug.LogWarning("'z' component of the 'boxSize' should be >= 0f.");
			hit = new EnvironmentRaycastHit
			{
				status = EnvironmentRaycastHitStatus.NoHit
			};
			return false;
		}
		if (!provider.Raycast(ray, out hit, maxDistance) || hit.normalConfidence < 0.5f)
		{
			return false;
		}
		Span<Vector3> span = stackalloc Vector3[4]
		{
			new Vector3(-1f, -1f),
			new Vector3(-1f, 1f),
			new Vector3(1f, 1f),
			new Vector3(1f, -1f)
		};
		for (int i = 0; i < span.Length; i++)
		{
			span[i] = Vector3.Scale(boxSize * 0.5f, span[i]);
		}
		Quaternion quaternion = Quaternion.LookRotation(hit.normal, upwards);
		Span<Vector3> span2 = stackalloc Vector3[span.Length];
		float num = Mathf.Pow(Mathf.Max(boxSize.x, boxSize.y) * 0.2f, 2f);
		for (int j = 0; j < span2.Length; j++)
		{
			Vector3 vector = hit.point + quaternion * span[j];
			if (!provider.Raycast(new Ray(ray.origin, vector - ray.origin), out var hit2))
			{
				return false;
			}
			if (Vector3.Project(hit2.point - hit.point, hit.normal).sqrMagnitude > num)
			{
				return false;
			}
			if (Vector3.Dot(hit2.normal, hit.normal) < 0.6f)
			{
				return false;
			}
			span2[j] = hit2.point;
		}
		Vector3 vector2 = -Vector3.Cross(span2[1] - span2[0], span2[2] - span2[0]).normalized;
		Vector3 vector3 = -Vector3.Cross(span2[1] - span2[3], span2[2] - span2[3]).normalized;
		Vector3 vector4 = Vector3.Normalize(vector2 + vector3);
		if (Vector3.Dot(vector4, hit.normal) < 0.9f)
		{
			return false;
		}
		hit.normal = vector4;
		quaternion = Quaternion.LookRotation(hit.normal, upwards);
		if (boxSize.z >= 0.05f)
		{
			return !provider.CheckBox(hit.point + hit.normal * (boxSize.z * 0.5f + 0.05f), boxSize * 0.5f, quaternion);
		}
		Span<int> span3 = stackalloc int[12]
		{
			0, 1, 1, 2, 2, 3, 3, 0, 0, 2,
			1, 3
		};
		for (int k = 0; k < span3.Length; k += 2)
		{
			Vector3 vector5 = hit.point + quaternion * span[span3[k]] + hit.normal * 0.05f;
			Vector3 direction = hit.point + quaternion * span[span3[k + 1]] + hit.normal * 0.05f - vector5;
			Ray ray2 = new Ray(vector5, direction);
			if (provider.Raycast(ray2, out var _, direction.magnitude))
			{
				return false;
			}
		}
		return true;
	}

	public static bool CheckBox(this IEnvironmentRaycastProvider provider, Vector3 center, Vector3 halfExtents, Quaternion orientation)
	{
		Span<Vector3> span = stackalloc Vector3[3]
		{
			Vector3.right,
			Vector3.up,
			Vector3.forward
		};
		int length = span.Length;
		for (int i = 0; i < length; i++)
		{
			for (int j = -1; j <= 1; j += 2)
			{
				Vector3 vector = center - orientation * halfExtents * j;
				Vector3 vector2 = vector + orientation * span[i] * (halfExtents[i % 3] * 2f) * j;
				Vector3 vector3 = orientation * span[(i + 1) % length] * (halfExtents[(i + 1) % 3] * 2f) / 2f * j;
				for (int k = 0; k < 3; k++)
				{
					Vector3 direction = vector2 - vector;
					float magnitude = direction.magnitude;
					if (magnitude > 0.01f)
					{
						provider.Raycast(new Ray(vector, direction), out var hit, magnitude, reconstructNormal: false, allowOccludedRayOrigin: false);
						if (hit.status != EnvironmentRaycastHitStatus.NoHit)
						{
							return true;
						}
					}
					vector += vector3;
					vector2 += vector3;
				}
			}
		}
		return false;
	}

	[Conditional("DEBUG_DEPTH_RAYCAST")]
	private static void Log(string msg)
	{
		UnityEngine.Debug.Log($"{Time.frameCount} {msg}");
	}

	[Conditional("DEBUG_DEPTH_RAYCAST")]
	internal static void DrawLine(Vector3 start, Vector3 end, Color color)
	{
		UnityEngine.Debug.DrawLine(start, end, color);
	}
}
