using Fusion;
using Meta.XR.MultiplayerBlocks.Colocation;
using Meta.XR.MultiplayerBlocks.Colocation.Fusion;
using Meta.XR.MultiplayerBlocks.Shared;
using UnityEngine;

namespace Meta.XR.MultiplayerBlocks.Fusion;

[NetworkBehaviourWeaved(0)]
public class FusionNetworkBootstrapper : NetworkBehaviour
{
	[SerializeField]
	private GameObject anchorPrefab;

	[SerializeField]
	private FusionNetworkData networkData;

	[SerializeField]
	private FusionMessenger networkMessenger;

	private NetworkBootstrapperParams _params;

	private void Awake()
	{
		_params.ovrCameraRig = UnityEngine.Object.FindObjectOfType<OVRCameraRig>();
		_params.colocationController = UnityEngine.Object.FindObjectOfType<ColocationController>();
		_params.setupColocationReadyEvents = delegate
		{
			_params.colocationLauncher.ColocationReady += OnColocationReady;
		};
	}

	public override void Spawned()
	{
		PlatformInit.GetEntitlementInformation(delegate(PlatformInfo info)
		{
			if (info.OculusUser != null)
			{
				NetworkBootstrapperUtils.SetEntitlementIds(info, ref _params);
				NetworkBootstrapperUtils.SetUpAndStartAutomaticColocation(ref _params, anchorPrefab, networkData, networkMessenger);
			}
		});
	}

	private void OnColocationReady()
	{
		if (_params.colocationController != null)
		{
			_params.colocationController.ColocationReadyCallbacks.Invoke();
		}
		Meta.XR.MultiplayerBlocks.Colocation.Logger.Log("FusionNetworkBootstrapper: Colocation is successful and ready", LogLevel.Info);
	}

	[WeaverGenerated]
	public override void CopyBackingFieldsToState(bool P_0)
	{
	}

	[WeaverGenerated]
	public override void CopyStateToBackingFields()
	{
	}
}
