using System.Collections.Generic;

namespace UnityEngine.XR.Interaction.Toolkit;

public static class GizmoHelpers
{
	private static readonly Color s_XAxisColor = new Color(73f / 85f, 0.24313726f, 0.11372549f, 0.93f);

	private static readonly Color s_YAxisColor = new Color(0.6039216f, 81f / 85f, 24f / 85f, 0.93f);

	private static readonly Color s_ZAxisColor = new Color(0.22745098f, 0.47843137f, 0.972549f, 0.93f);

	private static readonly Dictionary<Vector3, (Vector3, Vector3)> s_AxisMapping = new Dictionary<Vector3, (Vector3, Vector3)>
	{
		{
			Vector3.up,
			(Vector3.forward, Vector3.right)
		},
		{
			Vector3.forward,
			(Vector3.right, Vector3.up)
		},
		{
			Vector3.right,
			(Vector3.up, Vector3.forward)
		}
	};

	public static void DrawWirePlaneOriented(Vector3 position, Quaternion rotation, float size)
	{
		float num = size / 2f;
		Vector3 vector = new Vector3(num, 0f, 0f - num);
		Vector3 vector2 = new Vector3(num, 0f, num);
		Vector3 vector3 = new Vector3(0f - num, 0f, 0f - num);
		Vector3 vector4 = new Vector3(0f - num, 0f, num);
		Gizmos.DrawLine(rotation * vector + position, rotation * vector2 + position);
		Gizmos.DrawLine(rotation * vector2 + position, rotation * vector4 + position);
		Gizmos.DrawLine(rotation * vector4 + position, rotation * vector3 + position);
		Gizmos.DrawLine(rotation * vector3 + position, rotation * vector + position);
	}

	public static void DrawWireCubeOriented(Vector3 position, Quaternion rotation, float size)
	{
		float num = size / 2f;
		Vector3 vector = new Vector3(num, 0f, 0f - num);
		Vector3 vector2 = new Vector3(num, 0f, num);
		Vector3 vector3 = new Vector3(0f - num, 0f, 0f - num);
		Vector3 vector4 = new Vector3(0f - num, 0f, num);
		Vector3 vector5 = new Vector3(num, size, 0f - num);
		Vector3 vector6 = new Vector3(num, size, num);
		Vector3 vector7 = new Vector3(0f - num, size, 0f - num);
		Vector3 vector8 = new Vector3(0f - num, size, num);
		Gizmos.DrawLine(rotation * vector + position, rotation * vector2 + position);
		Gizmos.DrawLine(rotation * vector2 + position, rotation * vector4 + position);
		Gizmos.DrawLine(rotation * vector4 + position, rotation * vector3 + position);
		Gizmos.DrawLine(rotation * vector3 + position, rotation * vector + position);
		Gizmos.DrawLine(rotation * vector5 + position, rotation * vector6 + position);
		Gizmos.DrawLine(rotation * vector6 + position, rotation * vector8 + position);
		Gizmos.DrawLine(rotation * vector8 + position, rotation * vector7 + position);
		Gizmos.DrawLine(rotation * vector7 + position, rotation * vector5 + position);
		Gizmos.DrawLine(rotation * vector5 + position, rotation * vector + position);
		Gizmos.DrawLine(rotation * vector6 + position, rotation * vector2 + position);
		Gizmos.DrawLine(rotation * vector8 + position, rotation * vector4 + position);
		Gizmos.DrawLine(rotation * vector7 + position, rotation * vector3 + position);
	}

	public static void DrawAxisArrows(Transform transform, float size)
	{
		Vector3 position = transform.position;
		Gizmos.color = s_ZAxisColor;
		Gizmos.DrawRay(position, transform.forward * size);
		Gizmos.color = s_YAxisColor;
		Gizmos.DrawRay(position, transform.up * size);
		Gizmos.color = s_XAxisColor;
		Gizmos.DrawRay(position, transform.right * size);
	}

	internal static void DrawCapsule(Vector3 center, float height, float radius, Vector3 axis, Color color)
	{
	}
}
