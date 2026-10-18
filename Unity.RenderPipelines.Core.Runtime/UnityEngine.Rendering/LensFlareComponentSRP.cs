using System;
using UnityEngine.Serialization;

namespace UnityEngine.Rendering;

[ExecuteAlways]
[AddComponentMenu("Rendering/Lens Flare (SRP)")]
public sealed class LensFlareComponentSRP : MonoBehaviour
{
	private enum Version
	{
		Initial
	}

	[SerializeField]
	private LensFlareDataSRP m_LensFlareData;

	[SerializeField]
	private Version version;

	[Min(0f)]
	public float intensity = 1f;

	[Min(1E-05f)]
	public float maxAttenuationDistance = 100f;

	[Min(1E-05f)]
	public float maxAttenuationScale = 100f;

	public AnimationCurve distanceAttenuationCurve = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, 0f));

	public AnimationCurve scaleByDistanceCurve = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, 0f));

	public bool attenuationByLightShape = true;

	public AnimationCurve radialScreenAttenuationCurve = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, 1f));

	public bool useOcclusion;

	[Obsolete("Replaced by environmentOcclusion.")]
	public bool useBackgroundCloudOcclusion;

	[FormerlySerializedAs("volumetricCloudOcclusion")]
	[FormerlySerializedAs("useFogOpacityOcclusion")]
	public bool environmentOcclusion;

	[Obsolete("Replaced by environmentOcclusion.")]
	public bool useWaterOcclusion;

	[Min(0f)]
	public float occlusionRadius = 0.1f;

	[Range(1f, 64f)]
	public uint sampleCount = 32u;

	public float occlusionOffset = 0.05f;

	[Min(0f)]
	public float scale = 1f;

	public bool allowOffScreen;

	[Obsolete("Please use environmentOcclusion instead.")]
	public bool volumetricCloudOcclusion;

	private static float sCelestialAngularRadius = 0.057595868f;

	public TextureCurve occlusionRemapCurve;

	public Light lightOverride;

	public LensFlareDataSRP lensFlareData
	{
		get
		{
			return m_LensFlareData;
		}
		set
		{
			m_LensFlareData = value;
			OnValidate();
		}
	}

	public float celestialProjectedOcclusionRadius(Camera mainCam)
	{
		float num = (float)Math.Tan(sCelestialAngularRadius) * mainCam.farClipPlane;
		return occlusionRadius * num;
	}

	private void Awake()
	{
	}

	private void OnEnable()
	{
		if ((bool)lensFlareData)
		{
			LensFlareCommonSRP.Instance.AddData(this);
		}
		else
		{
			LensFlareCommonSRP.Instance.RemoveData(this);
		}
	}

	private void OnDisable()
	{
		LensFlareCommonSRP.Instance.RemoveData(this);
	}

	private void OnValidate()
	{
		if (base.isActiveAndEnabled && lensFlareData != null)
		{
			LensFlareCommonSRP.Instance.AddData(this);
		}
		else
		{
			LensFlareCommonSRP.Instance.RemoveData(this);
		}
	}

	private void OnDestroy()
	{
		occlusionRemapCurve.Release();
	}

	public LensFlareComponentSRP()
	{
		AnimationCurve baseCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
		Vector2 bounds = new Vector2(0f, 1f);
		occlusionRemapCurve = new TextureCurve(baseCurve, 1f, loop: false, in bounds);
		base._002Ector();
	}
}
