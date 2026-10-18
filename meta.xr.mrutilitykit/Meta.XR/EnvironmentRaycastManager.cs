using System;
using Meta.XR.EnvironmentDepth;
using UnityEngine;

namespace Meta.XR;

public class EnvironmentRaycastManager : MonoBehaviour
{
	private class EnvironmentRaycastProviderDepthManager : IEnvironmentRaycastProvider
	{
		private EnvironmentDepthManager _depthManager;

		bool IEnvironmentRaycastProvider.IsReady
		{
			get
			{
				EnsureDepthManagerIsPresent();
				if (!_depthManager.enabled || !_depthManager.gameObject.activeInHierarchy)
				{
					Debug.LogError("Please enable the 'EnvironmentDepthManager' component and its GameObject.", _depthManager);
					return false;
				}
				return true;
			}
		}

		bool IEnvironmentRaycastProvider.IsSupported => EnvironmentDepthManager.IsSupported;

		private void EnsureDepthManagerIsPresent()
		{
			if (_depthManager == null)
			{
				_depthManager = UnityEngine.Object.FindAnyObjectByType<EnvironmentDepthManager>(FindObjectsInactive.Include);
				if (_depthManager == null)
				{
					_depthManager = new GameObject("EnvironmentDepthManager").AddComponent<EnvironmentDepthManager>();
					Debug.LogWarning("EnvironmentDepthManager was added to the scene by EnvironmentRaycastManager. Please add EnvironmentDepthManager to prevent this warning.");
				}
			}
		}

		void IEnvironmentRaycastProvider.SetEnabled(bool isEnabled)
		{
			if (isEnabled)
			{
				if (IsSupported)
				{
					EnsureDepthManagerIsPresent();
					_depthManager.SetRaycastWarmUpEnabled(value: true);
					return;
				}
				string text = "EnvironmentRaycastManager is not supported. Requirements: Quest 3 or newer, Unity >= 2022.3.\n";
				if (Application.isEditor)
				{
					text += "To run the EnvironmentRaycastManager in Editor, please use Meta Quest Link.\n";
				}
				Debug.LogError(text);
			}
			else if (IsSupported && _depthManager != null)
			{
				_depthManager.SetRaycastWarmUpEnabled(value: false);
			}
		}

		bool IEnvironmentRaycastProvider.Raycast(Ray ray, out EnvironmentRaycastHit hit, float maxDistance, bool reconstructNormal, bool allowOccludedRayOrigin)
		{
			DepthRaycastHit hitInfo;
			bool result = _depthManager.Raycast(ray, out hitInfo, maxDistance, Eye.Both, reconstructNormal, allowOccludedRayOrigin);
			hit = ToEnvRaycastHit(hitInfo);
			return result;
		}
	}

	private static EnvironmentRaycastManager _instance;

	private static readonly IEnvironmentRaycastProvider _provider = CreateProvider();

	private static bool? _isSupported;

	public static bool IsSupported
	{
		get
		{
			bool valueOrDefault = _isSupported.GetValueOrDefault();
			if (!_isSupported.HasValue)
			{
				valueOrDefault = _provider.IsSupported;
				_isSupported = valueOrDefault;
			}
			return _isSupported.Value;
		}
	}

	private bool IsReady
	{
		get
		{
			if (!base.enabled || !base.gameObject.activeInHierarchy)
			{
				Debug.LogError("Please enable the 'EnvironmentRaycastManager' component and its GameObject.", this);
				return false;
			}
			return _provider.IsReady;
		}
	}

	private static IEnvironmentRaycastProvider CreateProvider()
	{
		return new EnvironmentRaycastProviderDepthManager();
	}

	private void Awake()
	{
		if (!IsSupported)
		{
			Debug.LogError("EnvironmentRaycastManager is not supported. Please check the 'IsSupported' property before enabling this component.");
		}
		_instance = this;
	}

	private void OnDestroy()
	{
		_instance = null;
	}

	private void Start()
	{
		OVRTelemetry.Start(651891190, 0, -1L).Send();
	}

	private void OnEnable()
	{
		SetProviderEnabled(isEnabled: true);
	}

	private void OnDisable()
	{
		SetProviderEnabled(isEnabled: false);
	}

	private static void SetProviderEnabled(bool isEnabled)
	{
		if (IsSupported)
		{
			_provider.SetEnabled(isEnabled);
		}
	}

	public bool Raycast(Ray ray, out EnvironmentRaycastHit hit, float maxDistance = 100f)
	{
		if (!IsReady)
		{
			hit = new EnvironmentRaycastHit
			{
				status = EnvironmentRaycastHitStatus.NotReady
			};
			return false;
		}
		if (!IsSupported)
		{
			hit = new EnvironmentRaycastHit
			{
				status = EnvironmentRaycastHitStatus.NotSupported
			};
			return false;
		}
		return _provider.Raycast(ray, out hit, maxDistance);
	}

	private static EnvironmentRaycastHit ToEnvRaycastHit(DepthRaycastHit depthHit)
	{
		EnvironmentRaycastHit result = default(EnvironmentRaycastHit);
		result.status = ToStatus(depthHit.result);
		result.point = depthHit.point;
		result.normal = depthHit.normal;
		result.normalConfidence = depthHit.normalConfidence;
		return result;
		static EnvironmentRaycastHitStatus ToStatus(DepthRaycastResult depthHitResult)
		{
			return depthHitResult switch
			{
				DepthRaycastResult.Success => EnvironmentRaycastHitStatus.Hit, 
				DepthRaycastResult.NotReady => EnvironmentRaycastHitStatus.NotReady, 
				DepthRaycastResult.HitPointOccluded => EnvironmentRaycastHitStatus.HitPointOccluded, 
				DepthRaycastResult.RayOutsideOfDepthCameraFrustum => EnvironmentRaycastHitStatus.HitPointOutsideOfCameraFrustum, 
				DepthRaycastResult.RayOccluded => EnvironmentRaycastHitStatus.RayOccluded, 
				DepthRaycastResult.NoHit => EnvironmentRaycastHitStatus.NoHit, 
				_ => throw new Exception($"Invalid result type: {depthHitResult}."), 
			};
		}
	}

	public bool PlaceBox(Ray ray, Vector3 boxSize, Vector3 upwards, out EnvironmentRaycastHit hit)
	{
		if (!IsReady)
		{
			hit = new EnvironmentRaycastHit
			{
				status = EnvironmentRaycastHitStatus.NotReady
			};
			return false;
		}
		if (!IsSupported)
		{
			hit = new EnvironmentRaycastHit
			{
				status = EnvironmentRaycastHitStatus.NotSupported
			};
			return false;
		}
		return _provider.PlaceBox(ray, boxSize, upwards, out hit);
	}

	public bool CheckBox(Vector3 center, Vector3 halfExtents, Quaternion orientation)
	{
		if (!IsReady)
		{
			return false;
		}
		if (IsSupported)
		{
			return _provider.CheckBox(center, halfExtents, orientation);
		}
		return false;
	}
}
