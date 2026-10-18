using UnityEngine;
using UnityEngine.Serialization;

namespace Unity.Cinemachine;

[AddComponentMenu("Cinemachine/Procedural/Noise/Cinemachine Basic Multi Channel Perlin")]
[SaveDuringPlay]
[DisallowMultipleComponent]
[CameraPipeline(CinemachineCore.Stage.Noise)]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.cinemachine@3.1/manual/CinemachineBasicMultiChannelPerlin.html")]
public class CinemachineBasicMultiChannelPerlin : CinemachineComponentBase, CinemachineFreeLookModifier.IModifiableNoise
{
	[Tooltip("The asset containing the Noise Profile.  Define the frequencies and amplitudes there to make a characteristic noise profile.  Make your own or just use one of the many presets.")]
	[FormerlySerializedAs("m_Definition")]
	[FormerlySerializedAs("m_NoiseProfile")]
	public NoiseSettings NoiseProfile;

	[Tooltip("When rotating the camera, offset the camera's pivot position by this much (camera space)")]
	[FormerlySerializedAs("m_PivotOffset")]
	public Vector3 PivotOffset = Vector3.zero;

	[Tooltip("Gain to apply to the amplitudes defined in the NoiseSettings asset.  1 is normal.  Setting this to 0 completely mutes the noise.")]
	[FormerlySerializedAs("m_AmplitudeGain")]
	public float AmplitudeGain = 1f;

	[Tooltip("Scale factor to apply to the frequencies defined in the NoiseSettings asset.  1 is normal.  Larger magnitudes will make the noise shake more rapidly.")]
	[FormerlySerializedAs("m_FrequencyGain")]
	public float FrequencyGain = 1f;

	private bool m_Initialized;

	private float m_NoiseTime;

	[SerializeField]
	[HideInInspector]
	[NoSaveDuringPlay]
	[FormerlySerializedAs("mNoiseOffsets")]
	private Vector3 m_NoiseOffsets = Vector3.zero;

	(float, float) CinemachineFreeLookModifier.IModifiableNoise.NoiseAmplitudeFrequency
	{
		get
		{
			return (AmplitudeGain, FrequencyGain);
		}
		set
		{
			(AmplitudeGain, FrequencyGain) = value;
		}
	}

	public override bool IsValid
	{
		get
		{
			if (base.enabled)
			{
				return NoiseProfile != null;
			}
			return false;
		}
	}

	public override CinemachineCore.Stage Stage => CinemachineCore.Stage.Noise;

	public override void MutateCameraState(ref CameraState curState, float deltaTime)
	{
		if (!IsValid || deltaTime < 0f)
		{
			m_Initialized = false;
			return;
		}
		if (!m_Initialized)
		{
			Initialize();
		}
		if (TargetPositionCache.CacheMode == TargetPositionCache.Mode.Playback && TargetPositionCache.HasCurrentTime)
		{
			m_NoiseTime = TargetPositionCache.CurrentTime * FrequencyGain;
		}
		else
		{
			m_NoiseTime += deltaTime * FrequencyGain;
		}
		curState.PositionCorrection += curState.GetCorrectedOrientation() * NoiseSettings.GetCombinedFilterResults(NoiseProfile.PositionNoise, m_NoiseTime, m_NoiseOffsets) * AmplitudeGain;
		Quaternion quaternion = Quaternion.Euler(NoiseSettings.GetCombinedFilterResults(NoiseProfile.OrientationNoise, m_NoiseTime, m_NoiseOffsets) * AmplitudeGain);
		if (PivotOffset != Vector3.zero)
		{
			Matrix4x4 matrix4x = Matrix4x4.Translate(-PivotOffset);
			matrix4x = Matrix4x4.Rotate(quaternion) * matrix4x;
			matrix4x = Matrix4x4.Translate(PivotOffset) * matrix4x;
			curState.PositionCorrection += curState.GetCorrectedOrientation() * matrix4x.MultiplyPoint(Vector3.zero);
		}
		curState.OrientationCorrection *= quaternion;
	}

	public void ReSeed()
	{
		m_NoiseOffsets = new Vector3(Random.Range(-1000f, 1000f), Random.Range(-1000f, 1000f), Random.Range(-1000f, 1000f));
	}

	private void Initialize()
	{
		m_Initialized = true;
		m_NoiseTime = CinemachineCore.CurrentTime * FrequencyGain;
		if (m_NoiseOffsets == Vector3.zero)
		{
			ReSeed();
		}
	}
}
