using System;
using GorillaExtensions;
using GorillaTag;
using Liv.Lck.Cosmetics;
using Liv.Lck.GorillaTag;
using Photon.Pun;
using UnityEngine;

public class LckSocialCamera : NetworkComponent, IGorillaSliceableSimple
{
	private enum CameraState
	{
		Empty = 0,
		Visible = 1,
		Recording = 2,
		OnNeck = 4
	}

	private enum CameraType
	{
		Cococam,
		Tablet
	}

	[SerializeField]
	private Transform _scaleTransform;

	[SerializeField]
	public GameObject CameraVisuals;

	[SerializeField]
	private VRRig _vrrig;

	[SerializeField]
	private VRRigSerializer m_rigNetworkController;

	[SerializeField]
	private CameraType m_cameraType;

	private bool m_isCorrupted = true;

	private bool m_lckDelegateRegistered;

	private IGtCameraVisuals m_CameraVisuals;

	private CameraState _localOwnedState;

	private CameraState _networkOwnedState;

	public VRRig VrRig => _vrrig;

	public LCKSocialCameraFollower SocialCameraFollower { get; private set; }

	public bool IsOnNeck
	{
		get
		{
			return GetFlag(base.IsLocallyOwned ? _localOwnedState : _networkOwnedState, CameraState.OnNeck);
		}
		set
		{
			if (base.IsLocallyOwned)
			{
				_localOwnedState = SetFlag(_localOwnedState, CameraState.OnNeck, value);
			}
		}
	}

	public bool visible
	{
		get
		{
			return GetFlag(base.IsLocallyOwned ? _localOwnedState : _networkOwnedState, CameraState.Visible);
		}
		set
		{
			if (base.IsLocallyOwned)
			{
				_localOwnedState = SetFlag(_localOwnedState, CameraState.Visible, value);
			}
		}
	}

	public bool recording
	{
		get
		{
			return GetFlag(base.IsLocallyOwned ? _localOwnedState : _networkOwnedState, CameraState.Recording);
		}
		set
		{
			if (base.IsLocallyOwned)
			{
				_localOwnedState = SetFlag(_localOwnedState, CameraState.Recording, value);
			}
		}
	}

	public override void OnSpawned()
	{
		if (base.IsLocallyOwned)
		{
			_localOwnedState = CameraState.Empty;
			visible = false;
			recording = false;
			IsOnNeck = false;
		}
	}

	protected override void WriteDataPUN(PhotonStream stream, PhotonMessageInfo info)
	{
		stream.SendNext(_localOwnedState);
	}

	protected override void ReadDataPUN(PhotonStream stream, PhotonMessageInfo info)
	{
		if (info.Sender == info.photonView.Owner && !m_isCorrupted)
		{
			CameraState newState = (CameraState)stream.ReceiveNext();
			ReadDataShared(newState);
		}
	}

	private void ReadDataShared(CameraState newState)
	{
		if (newState != _networkOwnedState)
		{
			ApplyVisualState(newState);
			_networkOwnedState = newState;
		}
	}

	private void ApplyVisualState(CameraState newState)
	{
		if (m_isCorrupted)
		{
			return;
		}
		bool flag = GetFlag(newState, CameraState.Visible);
		bool flag2 = GetFlag(newState, CameraState.Recording);
		bool flag3 = GetFlag(newState, CameraState.OnNeck);
		if (base.IsLocallyOwned)
		{
			m_CameraVisuals?.SetVisualsActive(active: false);
			m_CameraVisuals?.SetRecordingState(isRecording: false);
			return;
		}
		m_CameraVisuals?.SetNetworkedVisualsActive(flag);
		m_CameraVisuals?.SetRecordingState(flag2);
		if (m_cameraType == CameraType.Tablet)
		{
			if (flag3)
			{
				SocialCameraFollower.SetParentToRig();
			}
			else
			{
				SocialCameraFollower.SetParentNull();
			}
		}
	}

	private static bool GetFlag(CameraState currentState, CameraState flag)
	{
		return currentState.HasFlag(flag);
	}

	private static CameraState SetFlag(CameraState currentState, CameraState flag, bool shouldBeSet)
	{
		if (shouldBeSet)
		{
			return currentState | flag;
		}
		return currentState & ~flag;
	}

	protected override void Awake()
	{
		base.Awake();
		if (CameraVisuals != null && !CameraVisuals.TryGetComponent<IGtCameraVisuals>(out m_CameraVisuals))
		{
			Debug.LogError("LCK: LckSocialCamera failed to find IGtCameraVisuals component on CameraVisuals");
		}
		if (m_rigNetworkController.IsNull())
		{
			m_rigNetworkController = GetComponentInParent<VRRigSerializer>();
		}
		if (!m_rigNetworkController.IsNull())
		{
			InDelegateListProcessor<RigContainer, PhotonMessageInfoWrapped> succesfullSpawnEvent = m_rigNetworkController.SuccesfullSpawnEvent;
			InAction<RigContainer, PhotonMessageInfoWrapped> item = OnSuccesfullSpawn;
			succesfullSpawnEvent.Add(in item);
		}
	}

	private void OnDestroy()
	{
		if (m_lckDelegateRegistered)
		{
			LckSocialCameraManager.OnManagerSpawned = (Action<LckSocialCameraManager>)Delegate.Remove(LckSocialCameraManager.OnManagerSpawned, new Action<LckSocialCameraManager>(OnManagerSpawned));
		}
	}

	private void OnSuccesfullSpawn(in RigContainer rig, in PhotonMessageInfoWrapped info)
	{
		_vrrig = rig.Rig;
		LCKSocialCameraFollower lCKSocialCameraFollower = ((m_cameraType == CameraType.Cococam) ? rig.LckCococamFollower : rig.LCKTabletFollower);
		_scaleTransform = lCKSocialCameraFollower.ScaleTransform;
		CameraVisuals = lCKSocialCameraFollower.CameraVisualsRoot;
		m_CameraVisuals = CameraVisuals.GetComponent<IGtCameraVisuals>();
		if (!base.IsLocallyOwned && lCKSocialCameraFollower.GetComponent<ILckCosmeticDependantPlayerIdSupplier>() != null)
		{
			lCKSocialCameraFollower.GetComponent<ILckCosmeticDependantPlayerIdSupplier>().UpdatePlayerId();
		}
		SocialCameraFollower = lCKSocialCameraFollower;
		m_isCorrupted = false;
		if (_vrrig.isOfflineVRRig)
		{
			LckSocialCameraManager instance = LckSocialCameraManager.Instance;
			if (instance != null)
			{
				switch (m_cameraType)
				{
				case CameraType.Cococam:
					instance.SetLckSocialCococamCamera(this);
					break;
				case CameraType.Tablet:
					instance.SetLckSocialTabletCamera(this);
					break;
				default:
					throw new ArgumentOutOfRangeException();
				}
			}
			else
			{
				LckSocialCameraManager.OnManagerSpawned = (Action<LckSocialCameraManager>)Delegate.Combine(LckSocialCameraManager.OnManagerSpawned, new Action<LckSocialCameraManager>(OnManagerSpawned));
				m_lckDelegateRegistered = true;
			}
		}
		else
		{
			lCKSocialCameraFollower.SetNetworkController(this);
		}
	}

	public void SliceUpdate()
	{
		if (_vrrig.IsNull())
		{
			return;
		}
		if (m_cameraType == CameraType.Tablet)
		{
			if (IsOnNeck)
			{
				SocialCameraFollower.transform.localScale = Vector3.one * 0.3f;
			}
			else
			{
				SocialCameraFollower.transform.localScale = Vector3.one * 0.3f * _vrrig.scaleFactor;
			}
		}
		else if (m_cameraType == CameraType.Cococam)
		{
			SocialCameraFollower.transform.localScale = Vector3.one * _vrrig.scaleFactor;
		}
	}

	internal override void OnEnable()
	{
		base.OnEnable();
		GorillaSlicerSimpleManager.RegisterSliceable(this, GorillaSlicerSimpleManager.UpdateStep.Update);
	}

	internal override void OnDisable()
	{
		base.OnDisable();
		GorillaSlicerSimpleManager.UnregisterSliceable(this, GorillaSlicerSimpleManager.UpdateStep.Update);
		if (!m_isCorrupted)
		{
			if (SocialCameraFollower.IsNotNull())
			{
				SocialCameraFollower.RemoveNetworkController(this);
			}
			_scaleTransform = null;
			CameraVisuals = null;
		}
	}

	private void OnManagerSpawned(LckSocialCameraManager manager)
	{
		switch (m_cameraType)
		{
		case CameraType.Cococam:
			manager.SetLckSocialCococamCamera(this);
			break;
		case CameraType.Tablet:
			manager.SetLckSocialTabletCamera(this);
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	public void TurnOff()
	{
		m_isCorrupted = true;
		base.gameObject.SetActive(value: false);
	}
}
