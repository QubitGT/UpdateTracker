using Fusion.LagCompensation;
using UnityEngine;

namespace Fusion;

[ScriptHelp(BackColor = ScriptHeaderBackColor.Sand)]
[DisallowMultipleComponent]
public class RunnerLagCompensationGizmos : Behaviour
{
	public bool DrawSnapshotHistory;

	public bool DrawBroadphaseNodes;

	public Color StateAuthHitboxCollor = Color.green;

	public Color NonStateAuthHitboxCollor = Color.cyan;

	private NetworkRunner _runner;

	private void Awake()
	{
		_runner = GetComponentInParent<NetworkRunner>();
		if (_runner == null)
		{
			Debug.LogWarning($"{this} was not able to find the NetworkRunner reference. Destroying the component.");
			Object.Destroy(this);
		}
	}

	private void OnDrawGizmos()
	{
		if (!(_runner == null) && _runner.IsRunning && _runner.GetVisible() && _runner.LagCompensation?.DrawInfo != null)
		{
			if (DrawBroadphaseNodes)
			{
				RenderBHVBroadphase();
			}
			if (DrawSnapshotHistory)
			{
				RenderHitboxHistory();
			}
		}
	}

	private void RenderHitboxHistory()
	{
		Gizmos.color = (_runner.IsServer ? StateAuthHitboxCollor : NonStateAuthHitboxCollor);
		foreach (HitboxColliderContainerDraw item in _runner.LagCompensation.DrawInfo.SnapshotHistoryDraw)
		{
			foreach (ColliderDrawInfo item2 in item)
			{
				Gizmos.matrix = item2.LocalToWorldMatrix;
				switch (item2.Type)
				{
				case HitboxTypes.Box:
					Gizmos.DrawWireCube(item2.Offset, item2.BoxExtents * 2f);
					break;
				case HitboxTypes.Sphere:
					Gizmos.DrawWireSphere(item2.Offset, item2.Radius);
					break;
				case HitboxTypes.Capsule:
					LagCompensationDraw.GizmosDrawWireCapsule(item2.CapsuleTopCenter, item2.CapsuleBottomCenter, item2.Radius);
					break;
				default:
					Debug.LogWarning($"HitboxType {item2.Type} not supported to draw.");
					break;
				}
			}
		}
		Gizmos.matrix = Matrix4x4.identity;
	}

	private void RenderBHVBroadphase()
	{
		Color green = Color.green;
		foreach (BVHNodeDrawInfo item in _runner.LagCompensation.DrawInfo.BVHDraw)
		{
			Gizmos.color = green + Color.red * item.Depth / item.MaxDepth;
			Gizmos.DrawWireCube(item.Bounds.center, item.Bounds.size);
		}
	}
}
