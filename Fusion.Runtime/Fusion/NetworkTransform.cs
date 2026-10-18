using System.Runtime.CompilerServices;
using UnityEngine;

namespace Fusion;

[DisallowMultipleComponent]
[NetworkBehaviourWeaved(14)]
public sealed class NetworkTransform : NetworkTRSP, INetworkTRSPTeleport, IBeforeAllTicks, IPublicFacingInterface, IAfterAllTicks, IBeforeCopyPreviousState
{
	[SerializeField]
	[InlineHelp]
	public bool SyncScale = false;

	[SerializeField]
	[InlineHelp]
	public bool SyncParent = false;

	private Tick _initial;

	private Transform _transform;

	private bool _simulation;

	private bool _aoiEnabled;

	private bool _aoiAutoUpdateOriginal;

	[SerializeField]
	[InlineHelp]
	private bool _autoAOIOverride = true;

	[SerializeField]
	[InlineHelp]
	public bool DisableSharedModeInterpolation = false;

	private bool _render;

	private Vector3 _renderPosition;

	private Quaternion _renderRotation;

	private Transform _renderParent;

	public bool AutoUpdateAreaOfInterestOverride
	{
		get
		{
			return _autoAOIOverride;
		}
		set
		{
			_autoAOIOverride = (_aoiAutoUpdateOriginal = value);
		}
	}

	private void Awake()
	{
		_aoiAutoUpdateOriginal = _autoAOIOverride;
		TryGetComponent<Transform>(out _transform);
	}

	private void CopyToEngine()
	{
		if (base.IsMainTRSP && SyncParent)
		{
			NetworkTRSP.SetParentTransform(this, _transform, base.State.Parent);
		}
		_transform.localPosition = base.State.Position;
		_transform.localRotation = base.State.Rotation;
		if (SyncScale)
		{
			_transform.localScale = base.State.Scale;
		}
	}

	private void CopyToBuffer()
	{
		Transform transform = _transform;
		base.State.Position = transform.localPosition;
		base.State.Rotation = transform.localRotation;
		if (SyncScale)
		{
			base.State.Scale = transform.localScale;
		}
		if (!base.IsMainTRSP)
		{
			return;
		}
		Transform parent = transform.parent;
		bool flag = parent;
		if (flag && _aoiEnabled && _autoAOIOverride)
		{
			NetworkTRSP.ResolveAOIOverride(this, parent);
		}
		else
		{
			SetAreaOfInterestOverride(null);
		}
		if (!SyncParent)
		{
			return;
		}
		if (flag)
		{
			if (parent.TryGetComponent<NetworkBehaviour>(out var component))
			{
				base.State.Parent = component;
			}
			else
			{
				base.State.Parent = NetworkTRSPData.NonNetworkedParent;
			}
		}
		else
		{
			base.State.Parent = default(NetworkBehaviourId);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private bool CanInterpolate()
	{
		if (base.Runner.Mode == SimulationModes.Server || (DisableSharedModeInterpolation && ((base.Runner.Topology == Topologies.Shared && base.HasStateAuthority) || base.Runner.GameMode == GameMode.Single)))
		{
			return false;
		}
		return true;
	}

	void IBeforeAllTicks.BeforeAllTicks(bool resimulation, int tickCount)
	{
		if (!CanInterpolate())
		{
			return;
		}
		if (resimulation)
		{
			CopyToEngine();
			return;
		}
		if (_render && _simulation)
		{
			if (base.IsMainTRSP && SyncParent && base.transform.parent == _renderParent)
			{
				NetworkTRSP.SetParentTransform(this, _transform, base.State.Parent);
			}
			if (_transform.localPosition == _renderPosition && _transform.localRotation == _renderRotation)
			{
				_transform.localPosition = base.State.Position;
				_transform.localRotation = base.State.Rotation;
			}
		}
		_render = false;
		_simulation = false;
	}

	void IAfterAllTicks.AfterAllTicks(bool resimulation, int tickCount)
	{
		CopyToBuffer();
		_simulation = true;
	}

	void IBeforeCopyPreviousState.BeforeCopyPreviousState()
	{
		CopyToBuffer();
	}

	public void Teleport(Vector3? position = null, Quaternion? rotation = null)
	{
		NetworkTRSP.Teleport(this, _transform, position, rotation);
	}

	public override void SetAreaOfInterestOverride(NetworkObject obj)
	{
		base.SetAreaOfInterestOverride(obj);
		if ((bool)obj)
		{
			_autoAOIOverride = false;
		}
		else
		{
			_autoAOIOverride = _aoiAutoUpdateOriginal;
		}
	}

	public override void Spawned()
	{
		if (!_transform)
		{
			Awake();
		}
		_aoiEnabled = base.Runner.Config.Simulation.AreaOfInterestEnabled;
		_initial = default(Tick);
		if (base.Object.HasStateAuthority && !base.Object.Meta.HasSnapshots)
		{
			CopyToBuffer();
		}
		else
		{
			CopyToEngine();
		}
	}

	public override void Render()
	{
		if (CanInterpolate())
		{
			NetworkTRSP.Render(this, _transform, SyncScale, SyncParent, local: true, ref _initial);
			_render = true;
			_renderPosition = _transform.localPosition;
			_renderRotation = _transform.localRotation;
			_renderParent = _transform.parent;
		}
	}
}
