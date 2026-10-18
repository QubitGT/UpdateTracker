using System;
using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Locomotion;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Turning;
using UnityEngine.XR.Interaction.Toolkit.Utilities.Pooling;

namespace UnityEngine.XR.Interaction.Toolkit.Utilities;

internal class TeleportationMonitor
{
	private class PoseContainer
	{
		public Pose beforePose;

		public Pose afterPose;

		public Pose deltaPose;

		private int m_BeforeFrame = -1;

		private int m_AfterFrame = -1;

		private int m_DeltaFrame = -1;

		public void CaptureBeforePose(XRBodyTransformer bodyTransformer)
		{
			int frameCount = Time.frameCount;
			if (m_BeforeFrame != frameCount && LocomotionUtility.TryGetOriginTransform(bodyTransformer, out var originTransform))
			{
				m_BeforeFrame = frameCount;
				beforePose = originTransform.GetWorldPose();
			}
		}

		public void CaptureAfterPose(XRBodyTransformer bodyTransformer)
		{
			int frameCount = Time.frameCount;
			if (m_AfterFrame != frameCount && LocomotionUtility.TryGetOriginTransform(bodyTransformer, out var originTransform))
			{
				m_AfterFrame = frameCount;
				afterPose = originTransform.GetWorldPose();
			}
		}

		public void CalculateDeltaPose()
		{
			int frameCount = Time.frameCount;
			if (m_DeltaFrame != frameCount)
			{
				Vector3 position = afterPose.position - beforePose.position;
				Quaternion rotation = afterPose.rotation * Quaternion.Inverse(beforePose.rotation);
				m_DeltaFrame = frameCount;
				deltaPose = new Pose(position, rotation);
			}
		}
	}

	private abstract class ProviderMonitor
	{
		protected static Dictionary<XRBodyTransformer, PoseContainer> s_OriginPoses;

		public abstract void AddInteractor(IXRInteractor interactor);

		public abstract void RemoveInteractor(IXRInteractor interactor);
	}

	private class ProviderMonitor<T> : ProviderMonitor where T : LocomotionProvider
	{
		private Dictionary<T, List<IXRInteractor>> m_ProviderInteractors;

		private static List<T> s_Providers;

		private static readonly LinkedPool<Dictionary<T, List<IXRInteractor>>> s_ProviderInteractorsPool = new LinkedPool<Dictionary<T, List<IXRInteractor>>>(() => new Dictionary<T, List<IXRInteractor>>());

		public event Action<PoseContainer> providerStepped;

		public static void InitializeProvidersList()
		{
			if (s_Providers != null)
			{
				return;
			}
			s_Providers = new List<T>();
			foreach (LocomotionProvider locomotionProvider in LocomotionProvider.locomotionProviders)
			{
				if (!(locomotionProvider == null) && locomotionProvider is T item)
				{
					s_Providers.Add(item);
				}
			}
			LocomotionProvider.locomotionProvidersChanged += OnLocomotionProvidersChanged;
			static void OnLocomotionProvidersChanged(LocomotionProvider provider)
			{
				if (provider is T item2)
				{
					s_Providers.Add(item2);
				}
				s_Providers.RemoveAll((T p) => p == null);
			}
		}

		public override void AddInteractor(IXRInteractor interactor)
		{
			if (interactor == null)
			{
				throw new ArgumentNullException("interactor");
			}
			Transform transform = interactor.transform;
			if (transform == null)
			{
				return;
			}
			if (s_Providers == null)
			{
				InitializeProvidersList();
			}
			foreach (T s_Provider in s_Providers)
			{
				if (!(s_Provider == null) && LocomotionUtility.TryGetOriginTransform(s_Provider, out var originTransform) && transform.IsChildOf(originTransform))
				{
					if (m_ProviderInteractors == null)
					{
						m_ProviderInteractors = s_ProviderInteractorsPool.Get();
					}
					if (!m_ProviderInteractors.TryGetValue(s_Provider, out var value))
					{
						value = new List<IXRInteractor>();
						m_ProviderInteractors.Add(s_Provider, value);
					}
					value.Add(interactor);
					if (value.Count == 1)
					{
						s_Provider.beforeStepLocomotion += OnBeforeStepLocomotion;
						s_Provider.afterStepLocomotion += OnAfterStepLocomotion;
					}
				}
			}
		}

		public override void RemoveInteractor(IXRInteractor interactor)
		{
			if (interactor == null)
			{
				throw new ArgumentNullException("interactor");
			}
			int num = 0;
			if (m_ProviderInteractors != null)
			{
				foreach (KeyValuePair<T, List<IXRInteractor>> providerInteractor in m_ProviderInteractors)
				{
					T key = providerInteractor.Key;
					List<IXRInteractor> value = providerInteractor.Value;
					if (!(key == null))
					{
						if (value.Remove(interactor) && value.Count == 0)
						{
							key.beforeStepLocomotion -= OnBeforeStepLocomotion;
							key.afterStepLocomotion -= OnAfterStepLocomotion;
						}
						num += value.Count;
					}
				}
			}
			if (num == 0 && m_ProviderInteractors != null)
			{
				s_ProviderInteractorsPool.Release(m_ProviderInteractors);
				m_ProviderInteractors = null;
			}
		}

		private static void CaptureOriginPoseBefore(XRBodyTransformer bodyTransformer)
		{
			if (ProviderMonitor.s_OriginPoses == null)
			{
				ProviderMonitor.s_OriginPoses = new Dictionary<XRBodyTransformer, PoseContainer>();
			}
			if (!ProviderMonitor.s_OriginPoses.TryGetValue(bodyTransformer, out var value))
			{
				value = new PoseContainer();
				ProviderMonitor.s_OriginPoses[bodyTransformer] = value;
			}
			value.CaptureBeforePose(bodyTransformer);
		}

		private static PoseContainer CaptureOriginPoseAfter(XRBodyTransformer bodyTransformer)
		{
			if (ProviderMonitor.s_OriginPoses == null)
			{
				ProviderMonitor.s_OriginPoses = new Dictionary<XRBodyTransformer, PoseContainer>();
			}
			if (!ProviderMonitor.s_OriginPoses.TryGetValue(bodyTransformer, out var value))
			{
				value = new PoseContainer();
				ProviderMonitor.s_OriginPoses[bodyTransformer] = value;
			}
			value.CaptureAfterPose(bodyTransformer);
			return value;
		}

		private static void OnBeforeStepLocomotion(LocomotionProvider provider)
		{
			if (!(provider.mediator == null))
			{
				CaptureOriginPoseBefore(provider.mediator.bodyTransformer);
			}
		}

		private void OnAfterStepLocomotion(LocomotionProvider provider)
		{
			if (!(provider.mediator == null))
			{
				PoseContainer obj = CaptureOriginPoseAfter(provider.mediator.bodyTransformer);
				this.providerStepped?.Invoke(obj);
			}
		}
	}

	private int m_TeleportedFrame = -1;

	private ProviderMonitor[] m_Monitors;

	public event Action<Pose, Pose, Pose> teleported;

	private void Initialize()
	{
		ProviderMonitor<TeleportationProvider> providerMonitor = new ProviderMonitor<TeleportationProvider>();
		providerMonitor.providerStepped += OnTeleportedAlways;
		ProviderMonitor<SnapTurnProvider> providerMonitor2 = new ProviderMonitor<SnapTurnProvider>();
		providerMonitor2.providerStepped += OnTeleportedAlways;
		ProviderMonitor<ContinuousTurnProvider> providerMonitor3 = new ProviderMonitor<ContinuousTurnProvider>();
		providerMonitor3.providerStepped += OnTeleportedTurnAround;
		ProviderMonitor<TeleportationProvider>.InitializeProvidersList();
		ProviderMonitor<SnapTurnProvider>.InitializeProvidersList();
		ProviderMonitor<ContinuousTurnProvider>.InitializeProvidersList();
		m_Monitors = new ProviderMonitor[3] { providerMonitor, providerMonitor2, providerMonitor3 };
	}

	public void AddInteractor(IXRInteractor interactor)
	{
		if (m_Monitors == null)
		{
			Initialize();
		}
		ProviderMonitor[] monitors = m_Monitors;
		for (int i = 0; i < monitors.Length; i++)
		{
			monitors[i].AddInteractor(interactor);
		}
	}

	public void RemoveInteractor(IXRInteractor interactor)
	{
		ProviderMonitor[] monitors = m_Monitors;
		for (int i = 0; i < monitors.Length; i++)
		{
			monitors[i].RemoveInteractor(interactor);
		}
	}

	private void OnTeleportedAlways(PoseContainer poseContainer)
	{
		int frameCount = Time.frameCount;
		if (m_TeleportedFrame != frameCount)
		{
			m_TeleportedFrame = frameCount;
			poseContainer.CalculateDeltaPose();
			this.teleported?.Invoke(poseContainer.beforePose, poseContainer.afterPose, poseContainer.deltaPose);
		}
	}

	private void OnTeleportedTurnAround(PoseContainer poseContainer)
	{
		int frameCount = Time.frameCount;
		if (m_TeleportedFrame != frameCount && !(Vector3.Dot(poseContainer.beforePose.forward, poseContainer.afterPose.forward) >= 0f))
		{
			m_TeleportedFrame = frameCount;
			poseContainer.CalculateDeltaPose();
			this.teleported?.Invoke(poseContainer.beforePose, poseContainer.afterPose, poseContainer.deltaPose);
		}
	}
}
