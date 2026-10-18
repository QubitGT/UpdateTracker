using System.Collections.Generic;
using UnityEngine;

namespace DigitalOpus.MB.Core;

public class MB3_MeshBakerGrouperPie : MB3_MeshBakerGrouperBehaviour
{
	public override Dictionary<string, List<Renderer>> FilterIntoGroups(List<GameObject> selection, GrouperData d)
	{
		Dictionary<string, List<Renderer>> dictionary = new Dictionary<string, List<Renderer>>();
		if (d.pieNumSegments == 0)
		{
			Debug.LogError("pieNumSegments must be greater than zero.");
			return dictionary;
		}
		if (d.pieAxis.magnitude <= 1E-06f)
		{
			Debug.LogError("Pie axis vector is too short.");
			return dictionary;
		}
		if (d.ringSpacing <= 1E-06f)
		{
			Debug.LogError("Ring spacing is too small.");
			return dictionary;
		}
		d.pieAxis.Normalize();
		Quaternion quaternion = Quaternion.FromToRotation(d.pieAxis, Vector3.up);
		Debug.Log("Collecting renderers in each cell");
		foreach (GameObject item in selection)
		{
			if (item == null)
			{
				continue;
			}
			Renderer component = item.GetComponent<Renderer>();
			if (!(component is MeshRenderer) && !(component is SkinnedMeshRenderer))
			{
				continue;
			}
			Vector3 vector = component.bounds.center - d.origin;
			vector = quaternion * vector;
			float magnitude = new Vector2(vector.x, vector.z).magnitude;
			vector.Normalize();
			float num = 0f;
			if (Mathf.Abs(vector.x) < 0.0001f && Mathf.Abs(vector.z) < 0.0001f)
			{
				num = 0f;
			}
			else
			{
				num = Mathf.Atan2(vector.x, vector.z) * 57.29578f;
				if (num < 0f)
				{
					num = 360f + num;
				}
			}
			int num2 = Mathf.FloorToInt(num / 360f * (float)d.pieNumSegments);
			int num3 = Mathf.FloorToInt(magnitude / d.ringSpacing);
			if (num3 == 0 && d.combineSegmentsInInnermostRing)
			{
				num2 = 0;
			}
			List<Renderer> list = null;
			string key = "seg_" + num2 + "_ring_" + num3;
			if (dictionary.ContainsKey(key))
			{
				list = dictionary[key];
			}
			else
			{
				list = new List<Renderer>();
				dictionary.Add(key, list);
			}
			if (!list.Contains(component))
			{
				list.Add(component);
			}
		}
		return dictionary;
	}

	public override void DrawGizmos(Bounds sourceObjectBounds, GrouperData d)
	{
		if (d.pieAxis.magnitude < 0.1f || d.pieNumSegments < 1)
		{
			return;
		}
		Gizmos.color = MB3_MeshBakerGrouper.WHITE_TRANSP;
		int b = Mathf.CeilToInt(sourceObjectBounds.extents.magnitude / d.ringSpacing);
		b = Mathf.Max(1, b);
		for (int i = 0; i < b; i++)
		{
			DrawCircle(d.pieAxis.normalized, d.origin, d.ringSpacing * (float)(i + 1), 24);
		}
		Quaternion quaternion = Quaternion.FromToRotation(Vector3.up, d.pieAxis);
		Quaternion quaternion2 = Quaternion.AngleAxis(180f / (float)d.pieNumSegments, Vector3.up);
		Vector3 vector = Vector3.forward;
		for (int j = 0; j < d.pieNumSegments; j++)
		{
			Vector3 vector2 = quaternion * vector;
			Vector3 vector3 = d.origin;
			int num = b;
			if (d.combineSegmentsInInnermostRing)
			{
				vector3 = d.origin + vector2.normalized * d.ringSpacing;
				num = b - 1;
			}
			if (num != 0)
			{
				Gizmos.DrawLine(vector3, vector3 + (float)num * d.ringSpacing * vector2.normalized);
				vector = quaternion2 * vector;
				vector = quaternion2 * vector;
				continue;
			}
			break;
		}
	}

	private static int MaxIndexInVector3(Vector3 v)
	{
		int result = 0;
		float num = v.x;
		if (v.y > num)
		{
			result = 1;
			num = v.y;
		}
		if (v.z > num)
		{
			result = 2;
			num = v.z;
		}
		return result;
	}

	public static void DrawCircle(Vector3 axis, Vector3 center, float radius, int subdiv)
	{
		Quaternion quaternion = Quaternion.AngleAxis(360 / subdiv, axis);
		int num = MaxIndexInVector3(axis);
		int index = ((num == 0) ? (num + 1) : (num - 1));
		Vector3 vector = axis;
		float num2 = vector[num];
		vector[num] = vector[index];
		vector[index] = 0f - num2;
		vector = Vector3.ProjectOnPlane(vector, axis);
		vector.Normalize();
		vector *= radius;
		for (int i = 0; i < subdiv + 1; i++)
		{
			Vector3 vector2 = quaternion * vector;
			Gizmos.color = MB3_MeshBakerGrouper.WHITE_TRANSP;
			Gizmos.DrawLine(center + vector, center + vector2);
			vector = vector2;
		}
	}

	public override MB3_MeshBakerGrouper.ClusterType GetClusterType()
	{
		return MB3_MeshBakerGrouper.ClusterType.pie;
	}
}
