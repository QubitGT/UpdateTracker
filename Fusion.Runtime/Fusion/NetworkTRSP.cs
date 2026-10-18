using UnityEngine;

namespace Fusion;

[DisallowMultipleComponent]
[NetworkBehaviourWeaved(14)]
public class NetworkTRSP : NetworkBehaviour
{
	private PlayerRef _previousRenderStateAuth;

	private Vector3? _stateAuthorityChangePositionError;

	private Quaternion? _stateAuthorityChangeRotationError;

	[SerializeField]
	[InlineHelp]
	private float _stateAuthorityChangeErrorCorrectionDelta = 0f;

	protected Tick reenabledTick;

	public bool IsMainTRSP { get; internal set; }

	public NetworkTRSPData Data => base.StateBufferIsValid ? ReinterpretState<NetworkTRSPData>() : default(NetworkTRSPData);

	protected ref NetworkTRSPData State => ref ReinterpretState<NetworkTRSPData>();

	public virtual void SetAreaOfInterestOverride(NetworkObject obj)
	{
		if ((bool)obj)
		{
			Assert.Always(obj.RuntimeFlags.CheckFlag(NetworkObjectRuntimeFlags.HasMainNetworkTRSP), "area of interest proxy must have a main network trsp");
		}
		State.AreaOfInterestOverride = obj;
	}

	protected static void Teleport(NetworkTRSP behaviour, Transform transform, Vector3? position = null, Quaternion? rotation = null)
	{
		if (position.HasValue)
		{
			transform.position = position.Value;
			behaviour.State.Position = transform.localPosition;
		}
		if (rotation.HasValue)
		{
			transform.rotation = rotation.Value;
			behaviour.State.Rotation = transform.localRotation;
		}
		behaviour.State.TeleportKey++;
	}

	protected static void SetParentTransform(NetworkTRSP behaviour, Transform transform, NetworkBehaviourId parentId)
	{
		if (parentId.IsValid)
		{
			if (behaviour.Runner.TryFindBehaviour(parentId, out var behaviour2) && (object)transform.parent != behaviour2.transform)
			{
				transform.parent = behaviour2.transform;
			}
		}
		else if (parentId.Behaviour == 0 && (bool)transform.parent)
		{
			transform.parent = null;
		}
	}

	protected static void ResolveAOIOverride(NetworkTRSP behaviour, Transform parent)
	{
		Transform transform = parent;
		while ((bool)transform)
		{
			if (transform.TryGetComponent<NetworkObject>(out var component))
			{
				Assert.Always((object)component != behaviour.Object, "objects parent NetworkObject is itself?");
				behaviour.State.AreaOfInterestOverride = component;
				break;
			}
			transform = transform.parent;
		}
		if (!transform)
		{
			behaviour.State.AreaOfInterestOverride = default(NetworkId);
		}
	}

	private void OnEnable()
	{
		if (base.Object != null && (base.Object.RuntimeFlags & NetworkObjectRuntimeFlags.Spawned) != 0)
		{
			reenabledTick = base.Runner.Tick;
		}
		NetworkBehaviourUtils.InternalOnEnable(this);
	}

	protected static void Render(NetworkTRSP behaviour, Transform transform, bool syncScale, bool syncParent, bool local, ref Tick initial)
	{
		if (behaviour.TryGetSnapshotsBuffers(out var from, out var to, out var alpha))
		{
			NetworkTRSPData networkTRSPData = from.ReinterpretState<NetworkTRSPData>();
			NetworkTRSPData networkTRSPData2 = to.ReinterpretState<NetworkTRSPData>();
			PlayerRef stateAuthority = behaviour.Object.Meta.StateAuthority;
			if (initial == 0)
			{
				initial = to.Tick;
			}
			if (initial == to.Tick || behaviour.reenabledTick == to.Tick)
			{
				alpha = 1f;
			}
			if (networkTRSPData.TeleportKey != networkTRSPData2.TeleportKey)
			{
				alpha = ((alpha < 0.5f) ? 0f : 1f);
			}
			if (syncParent)
			{
				if (networkTRSPData.Parent != networkTRSPData2.Parent)
				{
					SetParentTransform(behaviour, transform, (alpha < 0.5f) ? networkTRSPData.Parent : networkTRSPData2.Parent);
					alpha = ((alpha < 0.5f) ? 0f : 1f);
				}
				else
				{
					SetParentTransform(behaviour, transform, networkTRSPData.Parent);
				}
			}
			if (syncScale)
			{
				Vector3Compressed scale = networkTRSPData.Scale;
				Vector3Compressed scale2 = networkTRSPData2.Scale;
				if (scale2 == scale)
				{
					if ((Vector3Compressed)transform.localScale != scale)
					{
						transform.localScale = scale;
					}
				}
				else
				{
					transform.localScale = Vector3.Lerp(scale, scale2, alpha);
				}
			}
			Vector3 vector = Vector3.Lerp(networkTRSPData.Position, networkTRSPData2.Position, alpha);
			Quaternion quaternion = Quaternion.Slerp(networkTRSPData.Rotation, networkTRSPData2.Rotation, alpha);
			if (stateAuthority != behaviour._previousRenderStateAuth)
			{
				Vector3 value = (local ? (transform.localPosition - vector) : (transform.position - vector));
				Quaternion value2 = (local ? (quaternion * Quaternion.Inverse(transform.localRotation)) : (quaternion * Quaternion.Inverse(transform.rotation)));
				behaviour._stateAuthorityChangePositionError = value;
				behaviour._stateAuthorityChangeRotationError = value2;
			}
			if (behaviour._stateAuthorityChangePositionError.HasValue && behaviour._stateAuthorityChangeErrorCorrectionDelta > 0f)
			{
				Vector3 value3 = behaviour._stateAuthorityChangePositionError.Value;
				vector += value3;
				value3 = Vector3.MoveTowards(value3, Vector3.zero, behaviour._stateAuthorityChangeErrorCorrectionDelta);
				behaviour._stateAuthorityChangePositionError = value3;
				if (value3.Equals(Vector3.zero))
				{
					behaviour._stateAuthorityChangePositionError = null;
				}
			}
			if (behaviour._stateAuthorityChangeRotationError.HasValue && behaviour._stateAuthorityChangeErrorCorrectionDelta > 0f)
			{
				Quaternion value4 = behaviour._stateAuthorityChangeRotationError.Value;
				quaternion *= value4;
				value4 = Quaternion.RotateTowards(value4, Quaternion.identity, behaviour._stateAuthorityChangeErrorCorrectionDelta);
				behaviour._stateAuthorityChangeRotationError = value4;
				if (value4.Equals(Quaternion.identity))
				{
					behaviour._stateAuthorityChangeRotationError = null;
				}
			}
			if (local)
			{
				transform.localPosition = vector;
				transform.localRotation = quaternion;
			}
			else
			{
				transform.SetPositionAndRotation(vector, quaternion);
			}
		}
		behaviour._previousRenderStateAuth = behaviour.Object.Meta.StateAuthority;
	}
}
