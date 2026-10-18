using System.Collections.Generic;
using UnityEngine;

namespace Unity.Cinemachine;

internal class CameraBlendStack : ICameraOverrideStack
{
	private class StackFrame : NestedBlendSource
	{
		public int Id;

		public int Priority;

		public readonly CinemachineBlend Source = new CinemachineBlend();

		public float DeltaTimeOverride;

		private readonly SnapshotBlendSource m_Snapshot = new SnapshotBlendSource();

		private ICinemachineCamera m_SnapshotSource;

		private float m_SnapshotBlendWeight;

		public bool Active => Source.IsValid;

		public StackFrame()
			: base(new CinemachineBlend())
		{
		}

		public ICinemachineCamera GetSnapshotIfAppropriate(ICinemachineCamera cam, float weight)
		{
			if (cam == null || (cam.State.BlendHint & CameraState.BlendHints.FreezeWhenBlendingOut) == 0)
			{
				m_Snapshot.TakeSnapshot(null);
				m_SnapshotSource = null;
				m_SnapshotBlendWeight = 0f;
				return cam;
			}
			if (m_SnapshotSource != cam || m_SnapshotBlendWeight > weight)
			{
				m_Snapshot.TakeSnapshot(cam);
				m_SnapshotSource = cam;
				m_SnapshotBlendWeight = weight;
			}
			return m_Snapshot;
		}
	}

	private class SnapshotBlendSource : ICinemachineCamera
	{
		private CameraState m_State;

		private string m_Name;

		public float RemainingTimeInBlend { get; set; }

		public string Name => m_Name;

		public string Description => Name;

		public CameraState State => m_State;

		public bool IsValid => true;

		public ICinemachineMixer ParentCamera => null;

		public SnapshotBlendSource(ICinemachineCamera source = null, float remainingTimeInBlend = 0f)
		{
			TakeSnapshot(source);
			RemainingTimeInBlend = remainingTimeInBlend;
		}

		public void UpdateCameraState(Vector3 worldUp, float deltaTime)
		{
		}

		public void OnCameraActivated(ICinemachineCamera.ActivationEventParams evt)
		{
		}

		public void TakeSnapshot(ICinemachineCamera source)
		{
			m_State = source?.State ?? CameraState.Default;
			m_State.BlendHint &= (CameraState.BlendHints)(-33);
			if (m_Name == null)
			{
				m_Name = ((source == null) ? "(null)" : ("*" + source.Name));
			}
		}
	}

	private const float kEpsilon = 0.0001f;

	private readonly List<StackFrame> m_FrameStack = new List<StackFrame>();

	private int m_NextFrameId;

	private static readonly AnimationCurve s_DefaultLinearAnimationCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	public Vector3 DefaultWorldUp => Vector3.up;

	public bool IsInitialized => m_FrameStack.Count > 0;

	public CinemachineBlendDefinition.LookupBlendDelegate LookupBlendDelegate { get; set; }

	public int SetCameraOverride(int overrideId, int priority, ICinemachineCamera camA, ICinemachineCamera camB, float weightB, float deltaTime)
	{
		if (overrideId < 0)
		{
			overrideId = ++m_NextFrameId;
		}
		if (m_FrameStack.Count == 0)
		{
			m_FrameStack.Add(new StackFrame());
		}
		StackFrame stackFrame = m_FrameStack[FindFrame(overrideId, priority)];
		stackFrame.DeltaTimeOverride = deltaTime;
		stackFrame.Source.TimeInBlend = weightB;
		if (stackFrame.Source.CamA != camA || stackFrame.Source.CamB != camB)
		{
			stackFrame.Source.CustomBlender = CinemachineCore.GetCustomBlender?.Invoke(camA, camB);
			stackFrame.Source.CamA = camA;
			stackFrame.Source.CamB = camB;
			if (camA is CinemachineVirtualCameraBase cinemachineVirtualCameraBase)
			{
				cinemachineVirtualCameraBase.EnsureStarted();
			}
			if (camB is CinemachineVirtualCameraBase cinemachineVirtualCameraBase2)
			{
				cinemachineVirtualCameraBase2.EnsureStarted();
			}
		}
		return overrideId;
		int FindFrame(int withId, int priority)
		{
			int count = m_FrameStack.Count;
			int num;
			for (num = count - 1; num > 0; num--)
			{
				if (m_FrameStack[num].Id == withId)
				{
					return num;
				}
			}
			for (num = 1; num < count && m_FrameStack[num].Priority <= priority; num++)
			{
			}
			StackFrame stackFrame2 = new StackFrame
			{
				Id = withId,
				Priority = priority
			};
			stackFrame2.Source.Duration = 1f;
			stackFrame2.Source.BlendCurve = s_DefaultLinearAnimationCurve;
			m_FrameStack.Insert(num, stackFrame2);
			return num;
		}
	}

	public void ReleaseCameraOverride(int overrideId)
	{
		for (int num = m_FrameStack.Count - 1; num > 0; num--)
		{
			if (m_FrameStack[num].Id == overrideId)
			{
				m_FrameStack.RemoveAt(num);
				break;
			}
		}
	}

	public virtual void OnEnable()
	{
		m_FrameStack.Clear();
		m_FrameStack.Add(new StackFrame());
	}

	public virtual void OnDisable()
	{
		m_FrameStack.Clear();
		m_NextFrameId = 0;
	}

	public void ResetRootFrame()
	{
		if (m_FrameStack.Count == 0)
		{
			m_FrameStack.Add(new StackFrame());
			return;
		}
		StackFrame stackFrame = m_FrameStack[0];
		stackFrame.Blend.ClearBlend();
		stackFrame.Blend.CamB = null;
		stackFrame.Source.ClearBlend();
		stackFrame.Source.CamB = null;
	}

	public void UpdateRootFrame(ICinemachineMixer context, ICinemachineCamera activeCamera, Vector3 up, float deltaTime)
	{
		if (m_FrameStack.Count == 0)
		{
			m_FrameStack.Add(new StackFrame());
		}
		StackFrame stackFrame = m_FrameStack[0];
		ICinemachineCamera camB = stackFrame.Source.CamB;
		if (activeCamera != camB)
		{
			bool flag = false;
			float num = 0f;
			float num2 = 0f;
			if (LookupBlendDelegate != null && activeCamera != null && activeCamera.IsValid && camB != null && camB.IsValid && deltaTime >= 0f)
			{
				CinemachineBlendDefinition cinemachineBlendDefinition = LookupBlendDelegate(camB, activeCamera);
				if (cinemachineBlendDefinition.BlendCurve != null && cinemachineBlendDefinition.BlendTime > 0.0001f)
				{
					flag = stackFrame.Source.CamA == activeCamera && stackFrame.Source.CamB == camB;
					if (flag && stackFrame.Blend.Duration > 0.0001f)
					{
						num = stackFrame.Blend.TimeInBlend / stackFrame.Blend.Duration;
					}
					stackFrame.Source.CamA = camB;
					stackFrame.Source.BlendCurve = cinemachineBlendDefinition.BlendCurve;
					num2 = cinemachineBlendDefinition.BlendTime;
				}
				stackFrame.Source.Duration = num2;
				stackFrame.Source.TimeInBlend = 0f;
				stackFrame.Source.CustomBlender = null;
			}
			stackFrame.Source.CamB = activeCamera;
			if (num2 > 0f)
			{
				stackFrame.Source.CustomBlender = CinemachineCore.GetCustomBlender?.Invoke(camB, activeCamera);
			}
			ICinemachineCamera camA;
			if (stackFrame.Blend.IsComplete)
			{
				camA = stackFrame.GetSnapshotIfAppropriate(camB, 0f);
			}
			else
			{
				bool flag2 = (stackFrame.Blend.State.BlendHint & CameraState.BlendHints.FreezeWhenBlendingOut) != 0;
				if (!flag2 && activeCamera != null && (activeCamera.State.BlendHint & CameraState.BlendHints.InheritPosition) != 0 && stackFrame.Blend.Uses(activeCamera))
				{
					flag2 = true;
				}
				NestedBlendSource nestedBlendSource = stackFrame.Blend.CamA as NestedBlendSource;
				if (!flag2 && nestedBlendSource != null && nestedBlendSource.Blend.CamA is NestedBlendSource nestedBlendSource2)
				{
					nestedBlendSource2.Blend.CamA = new SnapshotBlendSource(nestedBlendSource2.Blend.CamA);
				}
				if (flag)
				{
					flag2 = true;
					float num3 = stackFrame.Blend.TimeInBlend;
					if (nestedBlendSource != null)
					{
						num3 += nestedBlendSource.Blend.Duration - nestedBlendSource.Blend.TimeInBlend;
					}
					else if (stackFrame.Blend.CamA is SnapshotBlendSource snapshotBlendSource)
					{
						num3 += snapshotBlendSource.RemainingTimeInBlend;
					}
					num2 = Mathf.Min(num2 * num, num3);
				}
				if (flag2)
				{
					camA = new SnapshotBlendSource(stackFrame, stackFrame.Blend.Duration - stackFrame.Blend.TimeInBlend);
				}
				else
				{
					CinemachineBlend cinemachineBlend = new CinemachineBlend();
					cinemachineBlend.CopyFrom(stackFrame.Blend);
					camA = new NestedBlendSource(cinemachineBlend);
				}
			}
			stackFrame.Blend.CamA = camB;
			stackFrame.Blend.CamB = activeCamera;
			stackFrame.Blend.BlendCurve = stackFrame.Source.BlendCurve;
			stackFrame.Blend.Duration = num2;
			stackFrame.Blend.TimeInBlend = 0f;
			stackFrame.Blend.CustomBlender = stackFrame.Source.CustomBlender;
			if (num2 > 0f)
			{
				CinemachineCore.BlendCreatedEvent.Invoke(new CinemachineCore.BlendEventParams
				{
					Origin = context,
					Blend = stackFrame.Blend
				});
			}
			stackFrame.Blend.CamA = camA;
			stackFrame.Blend.CamB = activeCamera;
		}
		if (AdvanceBlend(stackFrame.Blend, deltaTime))
		{
			stackFrame.Source.ClearBlend();
		}
		stackFrame.UpdateCameraState(up, deltaTime);
		static bool AdvanceBlend(CinemachineBlend blend, float deltaTime)
		{
			bool result = false;
			if (blend.CamA != null)
			{
				blend.TimeInBlend += ((deltaTime >= 0f) ? deltaTime : blend.Duration);
				if (blend.IsComplete)
				{
					blend.ClearBlend();
					result = true;
				}
				else if (blend.CamA is NestedBlendSource nestedBlendSource3)
				{
					AdvanceBlend(nestedBlendSource3.Blend, deltaTime);
				}
			}
			return result;
		}
	}

	public void ProcessOverrideFrames(ref CinemachineBlend outputBlend, int numTopLayersToExclude)
	{
		if (m_FrameStack.Count == 0)
		{
			m_FrameStack.Add(new StackFrame());
		}
		int index = 0;
		int num = Mathf.Max(0, m_FrameStack.Count - numTopLayersToExclude);
		for (int i = 1; i < num; i++)
		{
			StackFrame stackFrame = m_FrameStack[i];
			if (!stackFrame.Active)
			{
				continue;
			}
			stackFrame.Blend.CopyFrom(stackFrame.Source);
			if (stackFrame.Source.CamA != null)
			{
				stackFrame.Blend.CamA = stackFrame.GetSnapshotIfAppropriate(stackFrame.Source.CamA, stackFrame.Source.TimeInBlend);
			}
			if (!stackFrame.Blend.IsComplete)
			{
				if (stackFrame.Blend.CamA == null)
				{
					stackFrame.Blend.CamA = stackFrame.GetSnapshotIfAppropriate(m_FrameStack[index], stackFrame.Blend.TimeInBlend);
					stackFrame.Blend.CustomBlender = CinemachineCore.GetCustomBlender?.Invoke(stackFrame.Blend.CamA, stackFrame.Blend.CamB);
				}
				if (stackFrame.Blend.CamB == null)
				{
					stackFrame.Blend.CamB = m_FrameStack[index];
					stackFrame.Blend.CustomBlender = CinemachineCore.GetCustomBlender?.Invoke(stackFrame.Blend.CamA, stackFrame.Blend.CamB);
				}
			}
			index = i;
		}
		outputBlend.CopyFrom(m_FrameStack[index].Blend);
	}

	public void SetRootBlend(CinemachineBlend blend)
	{
		if (IsInitialized)
		{
			if (blend == null)
			{
				m_FrameStack[0].Blend.Duration = 0f;
				return;
			}
			m_FrameStack[0].Blend.BlendCurve = blend.BlendCurve;
			m_FrameStack[0].Blend.TimeInBlend = blend.TimeInBlend;
			m_FrameStack[0].Blend.Duration = blend.Duration;
			m_FrameStack[0].Blend.CustomBlender = blend.CustomBlender;
		}
	}

	public float GetDeltaTimeOverride()
	{
		for (int num = m_FrameStack.Count - 1; num > 0; num--)
		{
			StackFrame stackFrame = m_FrameStack[num];
			if (stackFrame.Active)
			{
				return stackFrame.DeltaTimeOverride;
			}
		}
		return -1f;
	}
}
