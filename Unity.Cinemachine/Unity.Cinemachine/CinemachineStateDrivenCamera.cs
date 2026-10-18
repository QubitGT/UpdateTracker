using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Unity.Cinemachine;

[DisallowMultipleComponent]
[ExecuteAlways]
[ExcludeFromPreset]
[SaveDuringPlay]
[AddComponentMenu("Cinemachine/Cinemachine State Driven Camera")]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.cinemachine@3.1/manual/CinemachineStateDrivenCamera.html")]
public class CinemachineStateDrivenCamera : CinemachineCameraManagerBase
{
	[Serializable]
	public struct Instruction
	{
		[Tooltip("The full hash of the animation state")]
		[FormerlySerializedAs("m_FullHash")]
		public int FullHash;

		[Tooltip("The virtual camera to activate when the animation state becomes active")]
		[FormerlySerializedAs("m_VirtualCamera")]
		[ChildCameraProperty]
		public CinemachineVirtualCameraBase Camera;

		[Tooltip("How long to wait (in seconds) before activating the camera. This filters out very short state durations")]
		[FormerlySerializedAs("m_ActivateAfter")]
		public float ActivateAfter;

		[Tooltip("The minimum length of time (in seconds) to keep a camera active")]
		[FormerlySerializedAs("m_MinDuration")]
		public float MinDuration;
	}

	[Serializable]
	internal struct ParentHash
	{
		public int Hash;

		public int HashOfParent;
	}

	private struct HashPair
	{
		public int parentHash;

		public int hash;
	}

	[Space]
	[Tooltip("The state machine whose state changes will drive this camera's choice of active child")]
	[NoSaveDuringPlay]
	[FormerlySerializedAs("m_AnimatedTarget")]
	public Animator AnimatedTarget;

	[Tooltip("Which layer in the target state machine to observe")]
	[NoSaveDuringPlay]
	[FormerlySerializedAs("m_LayerIndex")]
	public int LayerIndex;

	[Tooltip("The set of instructions associating cameras with states.  These instructions are used to choose the live child at any given moment")]
	[FormerlySerializedAs("m_Instructions")]
	public Instruction[] Instructions;

	[HideInInspector]
	[SerializeField]
	[NoSaveDuringPlay]
	private List<ParentHash> HashOfParent = new List<ParentHash>();

	[SerializeField]
	[HideInInspector]
	[NoSaveDuringPlay]
	[FormerlySerializedAs("m_LookAt")]
	private Transform m_LegacyLookAt;

	[SerializeField]
	[HideInInspector]
	[NoSaveDuringPlay]
	[FormerlySerializedAs("m_Follow")]
	private Transform m_LegacyFollow;

	private float m_ActivationTime;

	private int m_ActiveInstructionIndex;

	private float m_PendingActivationTime;

	private int m_PendingInstructionIndex;

	private Dictionary<int, List<int>> m_InstructionDictionary;

	private Dictionary<int, int> m_StateParentLookup;

	private readonly List<AnimatorClipInfo> m_ClipInfoList = new List<AnimatorClipInfo>();

	private Dictionary<AnimationClip, List<HashPair>> m_HashCache;

	internal void SetParentHash(List<ParentHash> list)
	{
		HashOfParent.Clear();
		HashOfParent.AddRange(list);
	}

	protected override void Reset()
	{
		base.Reset();
		Instructions = null;
		AnimatedTarget = null;
		LayerIndex = 0;
		Instructions = null;
		DefaultBlend = new CinemachineBlendDefinition(CinemachineBlendDefinition.Styles.EaseInOut, 0.5f);
		CustomBlends = null;
	}

	protected internal override void PerformLegacyUpgrade(int streamedVersion)
	{
		base.PerformLegacyUpgrade(streamedVersion);
		if (streamedVersion < 20220721 && (m_LegacyLookAt != null || m_LegacyFollow != null))
		{
			DefaultTarget = new DefaultTargetSettings
			{
				Enabled = true,
				Target = new CameraTarget
				{
					LookAtTarget = m_LegacyLookAt,
					TrackingTarget = m_LegacyFollow,
					CustomLookAtTarget = (m_LegacyLookAt != m_LegacyFollow)
				}
			};
			m_LegacyLookAt = (m_LegacyFollow = null);
		}
	}

	internal static int CreateFakeHash(int parentHash, AnimationClip clip)
	{
		return Animator.StringToHash(parentHash + "_" + clip.name);
	}

	private int LookupFakeHash(int parentHash, AnimationClip clip)
	{
		if (m_HashCache == null)
		{
			m_HashCache = new Dictionary<AnimationClip, List<HashPair>>();
		}
		if (!m_HashCache.TryGetValue(clip, out var value))
		{
			value = new List<HashPair>();
			m_HashCache[clip] = value;
		}
		for (int i = 0; i < value.Count; i++)
		{
			if (value[i].parentHash == parentHash)
			{
				return value[i].hash;
			}
		}
		int num = CreateFakeHash(parentHash, clip);
		value.Add(new HashPair
		{
			parentHash = parentHash,
			hash = num
		});
		m_StateParentLookup[num] = parentHash;
		return num;
	}

	internal void ValidateInstructions()
	{
		if (Instructions == null)
		{
			Instructions = Array.Empty<Instruction>();
		}
		m_InstructionDictionary = new Dictionary<int, List<int>>();
		for (int i = 0; i < Instructions.Length; i++)
		{
			if (!m_InstructionDictionary.TryGetValue(Instructions[i].FullHash, out var value))
			{
				value = new List<int>();
				m_InstructionDictionary[Instructions[i].FullHash] = value;
			}
			value.Add(i);
		}
		m_StateParentLookup = new Dictionary<int, int>();
		int num = 0;
		while (HashOfParent != null && num < HashOfParent.Count)
		{
			m_StateParentLookup[HashOfParent[num].Hash] = HashOfParent[num].HashOfParent;
			num++;
		}
		m_HashCache = null;
		m_ActivationTime = (m_PendingActivationTime = 0f);
		ResetLiveChild();
	}

	protected override CinemachineVirtualCameraBase ChooseCurrentCamera(Vector3 worldUp, float deltaTime)
	{
		if (!PreviousStateIsValid)
		{
			ValidateInstructions();
		}
		List<CinemachineVirtualCameraBase> childCameras = base.ChildCameras;
		if (childCameras == null || childCameras.Count == 0)
		{
			m_ActivationTime = 0f;
			return null;
		}
		CinemachineVirtualCameraBase result = childCameras[0];
		if (AnimatedTarget == null || !AnimatedTarget.gameObject.activeSelf || AnimatedTarget.runtimeAnimatorController == null || LayerIndex < 0 || !AnimatedTarget.hasBoundPlayables || LayerIndex >= AnimatedTarget.layerCount)
		{
			m_ActivationTime = 0f;
			return result;
		}
		if (m_ActiveInstructionIndex < 0 || m_ActiveInstructionIndex >= Instructions.Length)
		{
			m_ActiveInstructionIndex = 0;
			m_ActivationTime = 0f;
		}
		if (!PreviousStateIsValid || m_PendingInstructionIndex < 0 || m_PendingInstructionIndex >= Instructions.Length)
		{
			m_PendingInstructionIndex = 0;
			m_PendingActivationTime = 0f;
		}
		int num;
		if (AnimatedTarget.IsInTransition(LayerIndex))
		{
			AnimatorStateInfo nextAnimatorStateInfo = AnimatedTarget.GetNextAnimatorStateInfo(LayerIndex);
			AnimatedTarget.GetNextAnimatorClipInfo(LayerIndex, m_ClipInfoList);
			num = GetClipHash(nextAnimatorStateInfo.fullPathHash, m_ClipInfoList);
		}
		else
		{
			AnimatorStateInfo currentAnimatorStateInfo = AnimatedTarget.GetCurrentAnimatorStateInfo(LayerIndex);
			AnimatedTarget.GetCurrentAnimatorClipInfo(LayerIndex, m_ClipInfoList);
			num = GetClipHash(currentAnimatorStateInfo.fullPathHash, m_ClipInfoList);
		}
		while (num != 0 && !m_InstructionDictionary.ContainsKey(num))
		{
			num = (m_StateParentLookup.ContainsKey(num) ? m_StateParentLookup[num] : 0);
		}
		int num2 = -1;
		if (m_InstructionDictionary.ContainsKey(num))
		{
			List<int> list = m_InstructionDictionary[num];
			int num3 = int.MinValue;
			for (int i = 0; i < list.Count; i++)
			{
				int num4 = list[i];
				CinemachineVirtualCameraBase cinemachineVirtualCameraBase = ((num4 < Instructions.Length) ? Instructions[num4].Camera : null);
				if (cinemachineVirtualCameraBase != null && cinemachineVirtualCameraBase.isActiveAndEnabled && cinemachineVirtualCameraBase.Priority.Value > num3)
				{
					num2 = num4;
					num3 = cinemachineVirtualCameraBase.Priority.Value;
				}
			}
		}
		float currentTime = CinemachineCore.CurrentTime;
		if (num2 >= 0)
		{
			if (m_ActivationTime == 0f)
			{
				m_ActiveInstructionIndex = num2;
				m_ActivationTime = currentTime;
				m_PendingActivationTime = 0f;
			}
			else if (m_ActiveInstructionIndex != num2 && (m_PendingActivationTime == 0f || m_PendingInstructionIndex != num2))
			{
				m_PendingInstructionIndex = num2;
				m_PendingActivationTime = currentTime;
			}
		}
		if (m_PendingActivationTime != 0f && currentTime - m_PendingActivationTime > Instructions[m_PendingInstructionIndex].ActivateAfter && currentTime - m_ActivationTime > Instructions[m_ActiveInstructionIndex].MinDuration)
		{
			m_ActiveInstructionIndex = m_PendingInstructionIndex;
			m_ActivationTime = currentTime;
			m_PendingActivationTime = 0f;
		}
		if (m_ActivationTime != 0f)
		{
			return Instructions[m_ActiveInstructionIndex].Camera;
		}
		return result;
	}

	private int GetClipHash(int hash, List<AnimatorClipInfo> clips)
	{
		int num = -1;
		for (int i = 0; i < clips.Count; i++)
		{
			if (num < 0 || clips[i].weight > clips[num].weight)
			{
				num = i;
			}
		}
		if (num >= 0 && clips[num].weight > 0f)
		{
			hash = LookupFakeHash(hash, clips[num].clip);
		}
		return hash;
	}

	public void CancelWait()
	{
		if (m_PendingActivationTime != 0f && m_PendingInstructionIndex >= 0 && m_PendingInstructionIndex < Instructions.Length)
		{
			m_ActiveInstructionIndex = m_PendingInstructionIndex;
			m_ActivationTime = CinemachineCore.CurrentTime;
			m_PendingActivationTime = 0f;
		}
	}
}
