using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.Cinemachine;

[AddComponentMenu("Cinemachine/Procedural/Extensions/Cinemachine FreeLook Modifier")]
[SaveDuringPlay]
[ExecuteAlways]
[DisallowMultipleComponent]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.cinemachine@3.1/manual/CinemachineFreeLookModifier.html")]
public class CinemachineFreeLookModifier : CinemachineExtension
{
	public interface IModifierValueSource
	{
		float NormalizedModifierValue { get; }
	}

	public interface IModifiablePositionDamping
	{
		Vector3 PositionDamping { get; set; }
	}

	public interface IModifiableComposition
	{
		ScreenComposerSettings Composition { get; set; }
	}

	public interface IModifiableDistance
	{
		float Distance { get; set; }
	}

	public interface IModifiableNoise
	{
		(float, float) NoiseAmplitudeFrequency { get; set; }
	}

	[Serializable]
	public abstract class Modifier
	{
		public virtual Type CachedComponentType => null;

		public virtual bool HasRequiredComponent => true;

		public virtual void Validate(CinemachineVirtualCameraBase vcam)
		{
		}

		public virtual void Reset(CinemachineVirtualCameraBase vcam)
		{
		}

		public virtual void RefreshCache(CinemachineVirtualCameraBase vcam)
		{
		}

		public virtual void BeforePipeline(CinemachineVirtualCameraBase vcam, ref CameraState state, float deltaTime, float modifierValue)
		{
		}

		public virtual void AfterPipeline(CinemachineVirtualCameraBase vcam, ref CameraState state, float deltaTime, float modifierValue)
		{
		}
	}

	public abstract class ComponentModifier<T> : Modifier
	{
		protected T CachedComponent;

		public override bool HasRequiredComponent => CachedComponent != null;

		public override Type CachedComponentType => typeof(T);

		public override void RefreshCache(CinemachineVirtualCameraBase vcam)
		{
			TryGetVcamComponent<T>(vcam, out CachedComponent);
		}
	}

	public class TiltModifier : Modifier
	{
		[HideFoldout]
		public TopBottomRigs<float> Tilt;

		public override void Validate(CinemachineVirtualCameraBase vcam)
		{
			Tilt.Top = Mathf.Clamp(Tilt.Top, -30f, 30f);
			Tilt.Bottom = Mathf.Clamp(Tilt.Bottom, -30f, 30f);
		}

		public override void Reset(CinemachineVirtualCameraBase vcam)
		{
			Tilt = new TopBottomRigs<float>
			{
				Top = -5f,
				Bottom = 5f
			};
		}

		public override void AfterPipeline(CinemachineVirtualCameraBase vcam, ref CameraState state, float deltaTime, float modifierValue)
		{
			float angle = ((modifierValue > 0f) ? Mathf.Lerp(0f, Tilt.Top, modifierValue) : Mathf.Lerp(Tilt.Bottom, 0f, modifierValue + 1f));
			Quaternion quaternion = state.RawOrientation * Quaternion.AngleAxis(angle, Vector3.right);
			state.OrientationCorrection = Quaternion.Inverse(state.GetCorrectedOrientation()) * quaternion;
		}
	}

	public class LensModifier : Modifier
	{
		[Tooltip("Value to take at the top of the axis range")]
		[LensSettingsHideModeOverrideProperty]
		public LensSettings Top;

		[Tooltip("Value to take at the bottom of the axis range")]
		[LensSettingsHideModeOverrideProperty]
		public LensSettings Bottom;

		public override void Validate(CinemachineVirtualCameraBase vcam)
		{
			Top.Validate();
			Bottom.Validate();
		}

		public override void Reset(CinemachineVirtualCameraBase vcam)
		{
			if (vcam == null)
			{
				Top = (Bottom = LensSettings.Default);
				return;
			}
			CameraState state = vcam.State;
			Top = (Bottom = state.Lens);
			Top.CopyCameraMode(ref state.Lens);
			Bottom.CopyCameraMode(ref state.Lens);
		}

		public override void BeforePipeline(CinemachineVirtualCameraBase vcam, ref CameraState state, float deltaTime, float modifierValue)
		{
			Top.CopyCameraMode(ref state.Lens);
			Bottom.CopyCameraMode(ref state.Lens);
			if (modifierValue >= 0f)
			{
				state.Lens.Lerp(in Top, modifierValue);
			}
			else
			{
				state.Lens.Lerp(in Bottom, 0f - modifierValue);
			}
		}
	}

	public class PositionDampingModifier : ComponentModifier<IModifiablePositionDamping>
	{
		[HideFoldout]
		public TopBottomRigs<Vector3> Damping;

		private Vector3 m_CenterDamping;

		public override void Validate(CinemachineVirtualCameraBase vcam)
		{
			Damping.Top = new Vector3(Mathf.Max(0f, Damping.Top.x), Mathf.Max(0f, Damping.Top.y), Mathf.Max(0f, Damping.Top.z));
			Damping.Bottom = new Vector3(Mathf.Max(0f, Damping.Bottom.x), Mathf.Max(0f, Damping.Bottom.y), Mathf.Max(0f, Damping.Bottom.z));
		}

		public override void Reset(CinemachineVirtualCameraBase vcam)
		{
			if (CachedComponent != null)
			{
				Damping.Top = (Damping.Bottom = CachedComponent.PositionDamping);
			}
		}

		public override void BeforePipeline(CinemachineVirtualCameraBase vcam, ref CameraState state, float deltaTime, float modifierValue)
		{
			if (CachedComponent != null)
			{
				m_CenterDamping = CachedComponent.PositionDamping;
				CachedComponent.PositionDamping = ((modifierValue >= 0f) ? Vector3.Lerp(m_CenterDamping, Damping.Top, modifierValue) : Vector3.Lerp(Damping.Bottom, m_CenterDamping, modifierValue + 1f));
			}
		}

		public override void AfterPipeline(CinemachineVirtualCameraBase vcam, ref CameraState state, float deltaTime, float modifierValue)
		{
			if (CachedComponent != null)
			{
				CachedComponent.PositionDamping = m_CenterDamping;
			}
		}
	}

	public class CompositionModifier : ComponentModifier<IModifiableComposition>
	{
		[HideFoldout]
		public TopBottomRigs<ScreenComposerSettings> Composition;

		private ScreenComposerSettings m_SavedComposition;

		public override void Validate(CinemachineVirtualCameraBase vcam)
		{
			Composition.Top.Validate();
			Composition.Bottom.Validate();
		}

		public override void Reset(CinemachineVirtualCameraBase vcam)
		{
			if (CachedComponent != null)
			{
				Composition.Top = (Composition.Bottom = CachedComponent.Composition);
			}
		}

		public override void BeforePipeline(CinemachineVirtualCameraBase vcam, ref CameraState state, float deltaTime, float modifierValue)
		{
			if (CachedComponent != null)
			{
				m_SavedComposition = CachedComponent.Composition;
				CachedComponent.Composition = ((modifierValue >= 0f) ? ScreenComposerSettings.Lerp(in m_SavedComposition, in Composition.Top, modifierValue) : ScreenComposerSettings.Lerp(in Composition.Bottom, in m_SavedComposition, modifierValue + 1f));
			}
		}

		public override void AfterPipeline(CinemachineVirtualCameraBase vcam, ref CameraState state, float deltaTime, float modifierValue)
		{
			if (CachedComponent != null)
			{
				CachedComponent.Composition = m_SavedComposition;
			}
		}
	}

	public class DistanceModifier : ComponentModifier<IModifiableDistance>
	{
		[HideFoldout]
		public TopBottomRigs<float> Distance;

		private float m_CenterDistance;

		public override void Validate(CinemachineVirtualCameraBase vcam)
		{
			Distance.Top = Mathf.Max(0f, Distance.Top);
			Distance.Bottom = Mathf.Max(0f, Distance.Bottom);
		}

		public override void Reset(CinemachineVirtualCameraBase vcam)
		{
			if (CachedComponent != null)
			{
				Distance.Top = (Distance.Bottom = CachedComponent.Distance);
			}
		}

		public override void BeforePipeline(CinemachineVirtualCameraBase vcam, ref CameraState state, float deltaTime, float modifierValue)
		{
			if (CachedComponent != null)
			{
				m_CenterDistance = CachedComponent.Distance;
				CachedComponent.Distance = ((modifierValue >= 0f) ? Mathf.Lerp(m_CenterDistance, Distance.Top, modifierValue) : Mathf.Lerp(Distance.Bottom, m_CenterDistance, modifierValue + 1f));
			}
		}

		public override void AfterPipeline(CinemachineVirtualCameraBase vcam, ref CameraState state, float deltaTime, float modifierValue)
		{
			if (CachedComponent != null)
			{
				CachedComponent.Distance = m_CenterDistance;
			}
		}
	}

	public class NoiseModifier : ComponentModifier<IModifiableNoise>
	{
		[Serializable]
		public struct NoiseSettings
		{
			[Tooltip("Multiplier for the noise amplitude")]
			public float Amplitude;

			[Tooltip("Multiplier for the noise frequency")]
			public float Frequency;
		}

		[HideFoldout]
		public TopBottomRigs<NoiseSettings> Noise;

		private (float, float) m_CenterNoise;

		public override void Reset(CinemachineVirtualCameraBase vcam)
		{
			if (CachedComponent != null)
			{
				(float, float) noiseAmplitudeFrequency = CachedComponent.NoiseAmplitudeFrequency;
				Noise.Top = (Noise.Bottom = new NoiseSettings
				{
					Amplitude = noiseAmplitudeFrequency.Item1,
					Frequency = noiseAmplitudeFrequency.Item2
				});
			}
		}

		public override void BeforePipeline(CinemachineVirtualCameraBase vcam, ref CameraState state, float deltaTime, float modifierValue)
		{
			if (CachedComponent != null)
			{
				m_CenterNoise = CachedComponent.NoiseAmplitudeFrequency;
				if (modifierValue >= 0f)
				{
					CachedComponent.NoiseAmplitudeFrequency = (Mathf.Lerp(m_CenterNoise.Item1, Noise.Top.Amplitude, modifierValue), Mathf.Lerp(m_CenterNoise.Item2, Noise.Top.Frequency, modifierValue));
				}
				else
				{
					CachedComponent.NoiseAmplitudeFrequency = (Mathf.Lerp(Noise.Bottom.Amplitude, m_CenterNoise.Item1, modifierValue + 1f), Mathf.Lerp(Noise.Bottom.Frequency, m_CenterNoise.Item2, modifierValue + 1f));
				}
			}
		}

		public override void AfterPipeline(CinemachineVirtualCameraBase vcam, ref CameraState state, float deltaTime, float modifierValue)
		{
			if (CachedComponent != null)
			{
				CachedComponent.NoiseAmplitudeFrequency = m_CenterNoise;
			}
		}
	}

	[Serializable]
	public struct TopBottomRigs<T>
	{
		[Tooltip("Value to take at the top of the axis range")]
		public T Top;

		[Tooltip("Value to take at the bottom of the axis range")]
		public T Bottom;
	}

	[Tooltip("The amount of easing to apply towards the center value. Zero easing blends linearly through the center value, while an easing of 1 smooths the result as it passes over the center value.")]
	[Range(0f, 1f)]
	public float Easing;

	[Tooltip("These will modify settings as a function of the FreeLook's Vertical axis value")]
	[SerializeReference]
	public List<Modifier> Modifiers = new List<Modifier>();

	private IModifierValueSource m_ValueSource;

	private float m_CurrentValue;

	private AnimationCurve m_EasingCurve;

	private float m_CachedEasingValue;

	private void OnValidate()
	{
		CinemachineVirtualCameraBase componentOwner = base.ComponentOwner;
		for (int i = 0; i < Modifiers.Count; i++)
		{
			Modifiers[i]?.Validate(componentOwner);
		}
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		RefreshComponentCache();
	}

	private static void TryGetVcamComponent<T>(CinemachineVirtualCameraBase vcam, out T component)
	{
		if (vcam == null || !vcam.TryGetComponent<T>(out component))
		{
			component = default(T);
		}
	}

	private void RefreshComponentCache()
	{
		CinemachineVirtualCameraBase componentOwner = base.ComponentOwner;
		TryGetVcamComponent<IModifierValueSource>(componentOwner, out m_ValueSource);
		for (int i = 0; i < Modifiers.Count; i++)
		{
			Modifiers[i]?.RefreshCache(componentOwner);
		}
	}

	internal bool HasValueSource()
	{
		RefreshComponentCache();
		return m_ValueSource != null;
	}

	public override void PrePipelineMutateCameraStateCallback(CinemachineVirtualCameraBase vcam, ref CameraState curState, float deltaTime)
	{
		if (m_ValueSource == null || !(vcam == base.ComponentOwner))
		{
			return;
		}
		if (m_EasingCurve == null || m_CachedEasingValue != Easing)
		{
			if (m_EasingCurve == null)
			{
				m_EasingCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
			}
			Keyframe[] keys = m_EasingCurve.keys;
			keys[0].outTangent = 1f - Easing;
			keys[1].inTangent = 1f + 2f * Easing;
			m_EasingCurve.keys = keys;
			m_CachedEasingValue = Easing;
		}
		float normalizedModifierValue = m_ValueSource.NormalizedModifierValue;
		float num = Mathf.Sign(normalizedModifierValue);
		m_CurrentValue = num * m_EasingCurve.Evaluate(Mathf.Abs(normalizedModifierValue));
		for (int i = 0; i < Modifiers.Count; i++)
		{
			Modifiers[i]?.BeforePipeline(vcam, ref curState, deltaTime, m_CurrentValue);
		}
	}

	protected override void PostPipelineStageCallback(CinemachineVirtualCameraBase vcam, CinemachineCore.Stage stage, ref CameraState state, float deltaTime)
	{
		if (m_ValueSource != null && stage == CinemachineCore.Stage.Finalize && vcam == base.ComponentOwner)
		{
			for (int i = 0; i < Modifiers.Count; i++)
			{
				Modifiers[i]?.AfterPipeline(vcam, ref state, deltaTime, m_CurrentValue);
			}
		}
	}
}
