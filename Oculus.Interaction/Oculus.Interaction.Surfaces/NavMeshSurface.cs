using UnityEngine;
using UnityEngine.AI;

namespace Oculus.Interaction.Surfaces;

public class NavMeshSurface : MonoBehaviour, ISurface
{
	[SerializeField]
	[Optional]
	[Tooltip("Allows the specification of an area name to be used in association with Unity's NavMesh Areas feature.For more information, see Unity's documentation on NavMesh Areas.")]
	private string _areaName = string.Empty;

	[SerializeField]
	[Optional]
	[Tooltip("Allows the specification of the agent index to be used in association with Unity's NavMesh Agent feature.For more information, see Unity's documentation on NavMesh Agents.")]
	private int _agentIndex;

	[SerializeField]
	[Min(0f)]
	private float _snapDistance;

	[SerializeField]
	[Min(0f)]
	private float _voxelSize = 0.01f;

	[SerializeField]
	private bool _calculateNormals;

	[InspectorButton("OpenUnityNavigation")]
	[SerializeField]
	private string _openUnityNavigation;

	private int _areaMask;

	private NavMeshQueryFilter _navMeshQuery;

	protected bool _started;

	public float SnapDistance
	{
		get
		{
			return _snapDistance;
		}
		set
		{
			_snapDistance = value;
		}
	}

	public float VoxelSize
	{
		get
		{
			return _voxelSize;
		}
		set
		{
			_voxelSize = Mathf.Max(0f, value);
		}
	}

	public bool CalculateHitNormals
	{
		get
		{
			return _calculateNormals;
		}
		set
		{
			_calculateNormals = value;
		}
	}

	public Transform Transform => null;

	protected virtual void Start()
	{
		this.BeginStart(ref _started);
		if (!string.IsNullOrEmpty(_areaName))
		{
			_areaMask = 1 << NavMesh.GetAreaFromName(_areaName);
		}
		else
		{
			_areaMask = -1;
		}
		_navMeshQuery = new NavMeshQueryFilter
		{
			agentTypeID = NavMesh.GetSettingsByIndex(_agentIndex).agentTypeID,
			areaMask = _areaMask
		};
		this.EndStart(ref _started);
	}

	public bool ClosestSurfacePoint(in Vector3 point, out SurfaceHit surfaceHit, float maxDistance = 0f)
	{
		if (NavMesh.SamplePosition(point, out var hit, maxDistance + _snapDistance, _navMeshQuery))
		{
			surfaceHit = new SurfaceHit
			{
				Point = hit.position,
				Normal = Vector3.up,
				Distance = hit.distance
			};
			return true;
		}
		surfaceHit = default(SurfaceHit);
		return false;
	}

	public bool Raycast(in Ray ray, out SurfaceHit surfaceHit, float maxDistance = 0f)
	{
		Vector3 direction = ray.direction;
		Vector3 origin = ray.origin;
		int num = Mathf.Max(1, Mathf.CeilToInt(Vector3.ProjectOnPlane(origin + direction * maxDistance - origin, Vector3.up).magnitude / _voxelSize));
		float num2 = maxDistance / (float)num;
		bool flag = false;
		surfaceHit = default(SurfaceHit);
		surfaceHit.Distance = float.PositiveInfinity;
		Vector3 vector = origin + direction * num2 * 0.5f;
		float num3 = Mathf.Max(num2, _snapDistance);
		float maxDistance2 = num2 + Mathf.Sqrt(num3 * num3 * 2f);
		for (int i = 0; i < num; i++)
		{
			if (NavMesh.SamplePosition(vector, out var hit, maxDistance2, _areaMask))
			{
				float num4 = Vector3.Distance(hit.position, vector);
				if (!(num4 < surfaceHit.Distance))
				{
					break;
				}
				flag = true;
				surfaceHit.Distance = num4;
				surfaceHit.Point = hit.position;
				surfaceHit.Normal = Vector3.up;
			}
			vector += direction * num2;
		}
		if (flag)
		{
			Vector3 point = surfaceHit.Point;
			Vector3 navMeshNormal = GetNavMeshNormal(point);
			if (!AlignHits(point, navMeshNormal, ray, ref surfaceHit, maxDistance))
			{
				return false;
			}
			return SnapSurfaceHit(ref surfaceHit, point);
		}
		return false;
	}

	private bool AlignHits(Vector3 point, Vector3 normal, Ray ray, ref SurfaceHit surfaceHit, float maxDistance)
	{
		if (new Plane(normal, point).Raycast(ray, out var enter) && enter <= maxDistance)
		{
			surfaceHit.Point = ray.GetPoint(enter);
			return true;
		}
		return false;
	}

	private bool SnapSurfaceHit(ref SurfaceHit surfaceHit, Vector3 navMeshPoint)
	{
		if (NavMesh.Raycast(navMeshPoint, surfaceHit.Point, out var hit, _navMeshQuery))
		{
			float num = Vector3.Distance(hit.position, surfaceHit.Point);
			surfaceHit.Point = hit.position;
			if (num > _snapDistance)
			{
				return false;
			}
		}
		return true;
	}

	private Vector3 GetNavMeshNormal(Vector3 navMeshPoint)
	{
		if (!CalculateHitNormals)
		{
			return Vector3.up;
		}
		Vector3 rhs = CalculateTangent(Vector3.right, navMeshPoint);
		return Vector3.Cross(CalculateTangent(Vector3.forward, navMeshPoint), rhs);
		bool CalculateStep(Vector3 centre, Vector3 stepDir, out Vector3 value)
		{
			if (NavMesh.SamplePosition(centre + stepDir * VoxelSize, out var hit, VoxelSize * 2f, _areaMask))
			{
				value = hit.position;
				return true;
			}
			value = Vector3.zero;
			return false;
		}
		Vector3 CalculateTangent(Vector3 direction, Vector3 centre)
		{
			Vector3 value2;
			bool flag = CalculateStep(centre, direction, out value2);
			Vector3 value3;
			bool flag2 = CalculateStep(centre, -direction, out value3);
			if (flag && flag2)
			{
				return (value2 - value3).normalized;
			}
			if (flag)
			{
				return (value2 - centre).normalized;
			}
			if (flag2)
			{
				return (centre - value3).normalized;
			}
			return direction;
		}
	}

	private void OpenUnityNavigation()
	{
	}

	public void InjectOptionalAreaName(string areaName)
	{
		_areaName = areaName;
	}

	public void InjectOptionalAgentIndex(int agentIndex)
	{
		_agentIndex = agentIndex;
	}

	bool ISurface.Raycast(in Ray ray, out SurfaceHit hit, float maxDistance)
	{
		return Raycast(in ray, out hit, maxDistance);
	}

	bool ISurface.ClosestSurfacePoint(in Vector3 point, out SurfaceHit hit, float maxDistance)
	{
		return ClosestSurfacePoint(in point, out hit, maxDistance);
	}
}
