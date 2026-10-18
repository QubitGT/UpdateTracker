using System;
using Oculus.Interaction.Input;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR;

namespace Oculus.Interaction.UnityXR;

public class FromUnityXRHmdDataSource : DataSource<HmdDataAsset>
{
	[Header("Shared Configuration")]
	[SerializeField]
	[Interface(typeof(ITrackingToWorldTransformer), new Type[] { })]
	private UnityEngine.Object _trackingToWorldTransformer;

	private ITrackingToWorldTransformer TrackingToWorldTransformer;

	private HmdDataAsset _hmdDataAsset = new HmdDataAsset();

	private HmdDataSourceConfig _config;

	[SerializeField]
	private XROrigin _origin;

	private HmdDataSourceConfig Config
	{
		get
		{
			if (_config != null)
			{
				return _config;
			}
			_config = new HmdDataSourceConfig
			{
				TrackingToWorldTransformer = TrackingToWorldTransformer
			};
			return _config;
		}
	}

	protected override HmdDataAsset DataAsset => _hmdDataAsset;

	protected void Awake()
	{
		TrackingToWorldTransformer = _trackingToWorldTransformer as ITrackingToWorldTransformer;
	}

	protected override void Start()
	{
		this.BeginStart(ref _started, delegate
		{
			base.Start();
		});
		this.EndStart(ref _started);
	}

	protected override void UpdateData()
	{
		_hmdDataAsset.Config = Config;
		_hmdDataAsset.Root = _origin.Camera.transform.GetLocalPose();
		_hmdDataAsset.IsTracked = XRSettings.isDeviceActive;
		_hmdDataAsset.FrameId = Time.frameCount;
	}

	public void InjectAllFromOVRHmdDataSource(UpdateModeFlags updateMode, IDataSource updateAfter, bool useOvrManagerEmulatedPose, ITrackingToWorldTransformer trackingToWorldTransformer)
	{
		InjectAllDataSource(updateMode, updateAfter);
		InjectTrackingToWorldTransformer(trackingToWorldTransformer);
	}

	public void InjectTrackingToWorldTransformer(ITrackingToWorldTransformer trackingToWorldTransformer)
	{
		_trackingToWorldTransformer = trackingToWorldTransformer as UnityEngine.Object;
		TrackingToWorldTransformer = trackingToWorldTransformer;
	}
}
