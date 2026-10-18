#define DEBUG
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Fusion;

[HelpURL("https://doc.photonengine.com/fusion/current/manual/prebuilt-components#networkmechanimanimator")]
[DisallowMultipleComponent]
[AddComponentMenu("Fusion/Network Mecanim Animator")]
[NetworkBehaviourWeaved(-1)]
public sealed class NetworkMecanimAnimator : NetworkBehaviour, IAfterAllTicks, IPublicFacingInterface
{
	internal readonly struct AnimatorData
	{
		public readonly int Param32Count;

		public readonly int ParamBoolCount;

		public readonly AnimatorControllerParameter[] Parameters;

		public readonly int ParameterCount;

		public readonly int[] ParameterHashes;

		public readonly int LayerCount;

		public readonly int SyncedLayerCount;

		public readonly int ParamBoolsWordCount;

		public readonly int ParamBoolsPtrOffset;

		public readonly int WordCount;

		public readonly int[] PrevBoolsBitmask;

		public AnimatorData(Animator animator, AnimatorSyncSettings syncSettings)
		{
			Assert.Check(animator);
			Parameters = animator.parameters;
			ParameterCount = animator.parameterCount;
			ParameterHashes = new int[ParameterCount];
			LayerCount = animator.layerCount;
			WordCount = GetWordCount(syncSettings, Parameters, ParameterHashes, LayerCount, out Param32Count, out ParamBoolCount, out SyncedLayerCount, out ParamBoolsWordCount);
			ParamBoolsPtrOffset = Param32Count;
			PrevBoolsBitmask = new int[ParamBoolsWordCount];
		}

		public static int GetWordCount(AnimatorSyncSettings syncSettings, AnimatorControllerParameter[] parameters, int[] parameterHashes, int layerCount, out int param32Count, out int paramBoolCount, out int syncedLayerCount, out int wordsUsedForBools)
		{
			bool flag = (syncSettings & AnimatorSyncSettings.ParameterFloats) == AnimatorSyncSettings.ParameterFloats;
			bool flag2 = (syncSettings & AnimatorSyncSettings.ParameterInts) == AnimatorSyncSettings.ParameterInts;
			bool flag3 = (syncSettings & AnimatorSyncSettings.ParameterBools) == AnimatorSyncSettings.ParameterBools;
			bool flag4 = (syncSettings & AnimatorSyncSettings.ParameterTriggers) == AnimatorSyncSettings.ParameterTriggers;
			bool flag5 = (syncSettings & AnimatorSyncSettings.StateRoot) == AnimatorSyncSettings.StateRoot;
			bool flag6 = (syncSettings & AnimatorSyncSettings.LayerWeights) == AnimatorSyncSettings.LayerWeights;
			bool flag7 = (syncSettings & AnimatorSyncSettings.StateLayers) == AnimatorSyncSettings.StateLayers;
			param32Count = 0;
			paramBoolCount = 0;
			int i = 0;
			for (int num = parameters.Length; i < num; i++)
			{
				AnimatorControllerParameter animatorControllerParameter = parameters[i];
				parameterHashes[i] = animatorControllerParameter.nameHash;
				switch (animatorControllerParameter.type)
				{
				case AnimatorControllerParameterType.Float:
					if (flag)
					{
						param32Count++;
					}
					break;
				case AnimatorControllerParameterType.Int:
					if (flag2)
					{
						param32Count++;
					}
					break;
				case AnimatorControllerParameterType.Bool:
					if (flag3)
					{
						paramBoolCount++;
					}
					break;
				case AnimatorControllerParameterType.Trigger:
					if (flag4)
					{
						paramBoolCount++;
					}
					break;
				}
			}
			syncedLayerCount = ((!flag7) ? 1 : layerCount);
			int num2 = param32Count;
			int num3 = (flag5 ? (2 * syncedLayerCount) : 0);
			int num4 = ((flag6 && layerCount > 0) ? (layerCount - 1) : 0);
			wordsUsedForBools = paramBoolCount * 4 + 31 >> 5;
			return num2 + wordsUsedForBools + num3 + num4;
		}
	}

	[InlineHelp]
	public Animator Animator;

	[InlineHelp]
	[SerializeField]
	public RenderSource ApplyTiming = RenderSource.To;

	[InlineHelp]
	[SerializeField]
	[ExpandableEnum]
	internal AnimatorSyncSettings SyncSettings = AnimatorSyncSettings.ParameterInts | AnimatorSyncSettings.ParameterFloats | AnimatorSyncSettings.ParameterBools | AnimatorSyncSettings.ParameterTriggers | AnimatorSyncSettings.LayerWeights;

	[InlineHelp]
	[SerializeField]
	internal int[] StateHashes;

	[InlineHelp]
	[SerializeField]
	internal int[] TriggerHashes;

	[InlineHelp]
	[ReadOnly]
	[SerializeField]
	internal int TotalWords;

	private readonly HashSet<int> _pendingTriggers = new HashSet<int>();

	private AnimatorData _animatorData;

	private bool _isInitialized;

	private const int BITS_PER_BOOL = 4;

	private int _lastAppliedTick;

	public override int? DynamicWordCount
	{
		get
		{
			EnsureInitialized();
			return TotalWords;
		}
	}

	public override void Spawned()
	{
		EnsureInitialized();
		if (Animator == null)
		{
			base.enabled = false;
		}
	}

	void IAfterAllTicks.AfterAllTicks(bool resimulation, int tickCount)
	{
		EnsureInitialized();
		if (base.Object.HasStateAuthority)
		{
			CaptureAnimatorData();
		}
	}

	public override void Render()
	{
		EnsureInitialized();
		if (!base.Object.IsProxy)
		{
			return;
		}
		RenderSource applyTiming = ApplyTiming;
		NetworkBehaviourBuffer from;
		NetworkBehaviourBuffer to;
		float alpha;
		if (applyTiming == RenderSource.Latest && base.Runner.Tick > _lastAppliedTick)
		{
			ApplyAnimatorData(base.StateBuffer);
			_lastAppliedTick = base.Runner.Tick;
		}
		else if (TryGetSnapshotsBuffers(out from, out to, out alpha))
		{
			NetworkBehaviourBuffer buffer = ((ApplyTiming == RenderSource.To) ? to : from);
			Tick tick = buffer.Tick;
			if (tick > _lastAppliedTick)
			{
				ApplyAnimatorData(buffer);
				_lastAppliedTick = tick;
			}
		}
	}

	public void SetTrigger(int triggerHash, bool passThroughOnInputAuthority = false)
	{
		EnsureInitialized();
		if (base.Object.HasStateAuthority)
		{
			_pendingTriggers.Add(triggerHash);
		}
		else if (passThroughOnInputAuthority && base.Object.HasInputAuthority)
		{
			Animator.SetTrigger(triggerHash);
		}
	}

	public void SetTrigger(string trigger, bool passThroughOnInputAuthority = false)
	{
		EnsureInitialized();
		if (base.Object.HasStateAuthority)
		{
			int item = Animator.StringToHash(trigger);
			_pendingTriggers.Add(item);
		}
		else if (passThroughOnInputAuthority && base.Object.HasInputAuthority)
		{
			Animator.SetTrigger(trigger);
		}
	}

	private void CaptureAnimatorData()
	{
		int wordOffset = 0;
		CaptureParameters(ref wordOffset);
		if ((SyncSettings & AnimatorSyncSettings.StateRoot) == AnimatorSyncSettings.StateRoot)
		{
			CaptureStates(ref wordOffset);
		}
		if ((SyncSettings & AnimatorSyncSettings.LayerWeights) == AnimatorSyncSettings.LayerWeights)
		{
			CaptureLayerWeights(ref wordOffset);
		}
	}

	private void ApplyAnimatorData(NetworkBehaviourBuffer buffer)
	{
		int wordOffset = 0;
		ApplyParameters(buffer, ref wordOffset);
		if ((SyncSettings & AnimatorSyncSettings.StateRoot) == AnimatorSyncSettings.StateRoot)
		{
			ApplyStates(buffer, ref wordOffset);
		}
		if ((SyncSettings & AnimatorSyncSettings.LayerWeights) == AnimatorSyncSettings.LayerWeights)
		{
			ApplyLayerWeights(buffer, ref wordOffset);
		}
	}

	private void CaptureStates(ref int wordOffset)
	{
		for (int i = 0; i < _animatorData.SyncedLayerCount; i++)
		{
			if (Animator.IsInTransition(i))
			{
				ReinterpretState<int>(wordOffset++) = 0;
				ReinterpretState<FloatCompressed>(wordOffset++) = 0f;
				continue;
			}
			AnimatorStateInfo currentAnimatorStateInfo = Animator.GetCurrentAnimatorStateInfo(i);
			int num = currentAnimatorStateInfo.fullPathHash;
			int num2 = Array.IndexOf(StateHashes, num);
			if (num2 >= 0)
			{
				num = num2;
			}
			else
			{
				InternalLogStreams.LogDebug?.Warn(base.name + ":" + GetType().Name + " cannot find hash in indexes. Inspect the component to refresh the controller hash lookup. Sending full hash instead of index as fallback.");
			}
			ReinterpretState<int>(wordOffset++) = num;
			ReinterpretState<FloatCompressed>(wordOffset++) = currentAnimatorStateInfo.normalizedTime;
		}
	}

	private void ApplyStates(NetworkBehaviourBuffer buffer, ref int wordOffset)
	{
		for (int i = 0; i < _animatorData.SyncedLayerCount; i++)
		{
			int num = buffer.ReinterpretState<int>(wordOffset++);
			FloatCompressed floatCompressed = buffer.ReinterpretState<FloatCompressed>(wordOffset++);
			if (num == 0)
			{
				break;
			}
			if (num > 0 && num < StateHashes.Length)
			{
				num = StateHashes[num];
			}
			else
			{
				InternalLogStreams.LogDebug?.Warn(base.name + ":" + GetType().Name + " cannot find hash in indexes. Inspect the component to refresh the controller hash lookup. Sending full hash instead of index as fallback.");
			}
			Animator.Play(num, i, floatCompressed);
		}
	}

	private void CaptureParameters(ref int wordOffset)
	{
		bool flag = (SyncSettings & AnimatorSyncSettings.ParameterFloats) == AnimatorSyncSettings.ParameterFloats;
		bool flag2 = (SyncSettings & AnimatorSyncSettings.ParameterInts) == AnimatorSyncSettings.ParameterInts;
		bool flag3 = (SyncSettings & AnimatorSyncSettings.ParameterBools) == AnimatorSyncSettings.ParameterBools;
		bool flag4 = (SyncSettings & AnimatorSyncSettings.ParameterTriggers) == AnimatorSyncSettings.ParameterTriggers;
		bool flag5 = true;
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		AnimatorControllerParameter[] parameters = _animatorData.Parameters;
		int[] parameterHashes = _animatorData.ParameterHashes;
		int i = 0;
		for (int parameterCount = _animatorData.ParameterCount; i < parameterCount; i++)
		{
			int num4 = parameterHashes[i];
			switch (parameters[i].type)
			{
			case AnimatorControllerParameterType.Float:
				if (flag)
				{
					float num11 = (Animator.IsParameterControlledByCurve(num4) ? 0f : Animator.GetFloat(num4));
					ReinterpretState<FloatCompressed>(wordOffset++) = num11;
				}
				break;
			case AnimatorControllerParameterType.Int:
				if (flag2)
				{
					ReinterpretState<int>(wordOffset++) = Animator.GetInteger(num4);
				}
				break;
			case AnimatorControllerParameterType.Bool:
			{
				if (!flag3)
				{
					break;
				}
				if (flag5)
				{
					num3 = _animatorData.ParamBoolsPtrOffset;
					flag5 = false;
				}
				int num9 = 4 * num;
				int num10 = 15 << num9;
				num3 &= ~num10;
				if (Animator.GetBool(num4))
				{
					num3 |= 1 << num9;
				}
				num++;
				if (num == 8)
				{
					num = 0;
					ReinterpretState<int>(_animatorData.ParamBoolsPtrOffset + num2++) = num3;
					if (num2 < _animatorData.ParamBoolsWordCount)
					{
						num3 = ReinterpretState<int>(_animatorData.ParamBoolsPtrOffset + num2);
					}
				}
				break;
			}
			case AnimatorControllerParameterType.Trigger:
			{
				bool flag6 = _pendingTriggers.Contains(num4);
				if (flag6)
				{
					Animator.SetTrigger(num4);
				}
				if (!flag4)
				{
					break;
				}
				if (flag5)
				{
					num3 = ReinterpretState<int>(_animatorData.ParamBoolsPtrOffset);
					flag5 = false;
				}
				int num5 = 4 * num;
				int num6 = 15 << num5;
				int num7 = (num3 & num6) >> num5;
				int num8 = num7 >> 1;
				bool flag7 = (num7 & 1) != 0;
				if (flag6 || flag7)
				{
					num7 = num8 + 1 << 1;
					if (flag6)
					{
						num7 |= 1;
					}
					num7 <<= num5;
					num3 &= ~num6;
					num3 |= num7 & num6;
				}
				num++;
				if (num == 8)
				{
					num = 0;
					ReinterpretState<int>(_animatorData.ParamBoolsPtrOffset + num2++) = num3;
					if (num2 < _animatorData.ParamBoolsWordCount)
					{
						num3 = ReinterpretState<int>(_animatorData.ParamBoolsPtrOffset + num2);
					}
				}
				break;
			}
			}
		}
		if (num > 0)
		{
			ReinterpretState<int>(_animatorData.ParamBoolsPtrOffset + num2) = num3;
		}
		wordOffset += _animatorData.ParamBoolsWordCount;
		_pendingTriggers.Clear();
	}

	private void ApplyParameters(NetworkBehaviourBuffer buffer, ref int wordOffset)
	{
		bool flag = (SyncSettings & AnimatorSyncSettings.ParameterFloats) == AnimatorSyncSettings.ParameterFloats;
		bool flag2 = (SyncSettings & AnimatorSyncSettings.ParameterInts) == AnimatorSyncSettings.ParameterInts;
		bool flag3 = (SyncSettings & AnimatorSyncSettings.ParameterBools) == AnimatorSyncSettings.ParameterBools;
		bool flag4 = (SyncSettings & AnimatorSyncSettings.ParameterTriggers) == AnimatorSyncSettings.ParameterTriggers;
		bool flag5 = true;
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		AnimatorControllerParameter[] parameters = _animatorData.Parameters;
		int[] parameterHashes = _animatorData.ParameterHashes;
		int i = 0;
		for (int parameterCount = _animatorData.ParameterCount; i < parameterCount; i++)
		{
			int num5 = parameterHashes[i];
			switch (parameters[i].type)
			{
			case AnimatorControllerParameterType.Float:
				if (flag)
				{
					if (Animator.IsParameterControlledByCurve(num5))
					{
						wordOffset++;
						break;
					}
					float value2 = buffer.ReinterpretState<FloatCompressed>(wordOffset++);
					Animator.SetFloat(num5, value2);
				}
				break;
			case AnimatorControllerParameterType.Int:
				if (flag2)
				{
					int value = buffer.ReinterpretState<int>(wordOffset++);
					Animator.SetInteger(num5, value);
				}
				break;
			case AnimatorControllerParameterType.Bool:
			{
				if (!flag3)
				{
					break;
				}
				if (flag5)
				{
					num3 = _animatorData.PrevBoolsBitmask[0];
					num4 = buffer.ReinterpretState<int>(_animatorData.ParamBoolsPtrOffset);
					flag5 = false;
				}
				int num10 = 4 * num;
				bool value3 = (num4 & (1 << num10)) != 0;
				Animator.SetBool(num5, value3);
				num++;
				if (num == 8)
				{
					_animatorData.PrevBoolsBitmask[num2] = num4;
					num = 0;
					num2++;
					num3 = _animatorData.PrevBoolsBitmask[num2];
					if (num2 < _animatorData.ParamBoolsWordCount)
					{
						num3 = _animatorData.PrevBoolsBitmask[num2];
						num4 = buffer.ReinterpretState<int>(_animatorData.ParamBoolsPtrOffset + num2);
					}
				}
				break;
			}
			case AnimatorControllerParameterType.Trigger:
			{
				if (!flag4)
				{
					break;
				}
				if (flag5)
				{
					num3 = _animatorData.PrevBoolsBitmask[0];
					num4 = buffer.ReinterpretState<int>(_animatorData.ParamBoolsPtrOffset);
					flag5 = false;
				}
				int num6 = 4 * num;
				int num7 = 15 << num6;
				int num8 = (num3 & num7) >> num6;
				int num9 = (num4 & num7) >> num6;
				if (num8 != num9)
				{
					bool flag6 = (num8 & 1) != 0;
					if (((uint)num9 & (true ? 1u : 0u)) != 0 || !flag6)
					{
						Animator.SetTrigger(num5);
					}
				}
				num++;
				if (num == 8)
				{
					_animatorData.PrevBoolsBitmask[num2] = num4;
					num = 0;
					num2++;
					if (num2 < _animatorData.ParamBoolsWordCount)
					{
						num3 = _animatorData.PrevBoolsBitmask[num2];
						num4 = buffer.ReinterpretState<int>(_animatorData.ParamBoolsPtrOffset + num2);
					}
				}
				break;
			}
			}
		}
		if (num > 0)
		{
			_animatorData.PrevBoolsBitmask[num2] = num4;
		}
		wordOffset += _animatorData.ParamBoolsWordCount;
	}

	private void CaptureLayerWeights(ref int wordOffset)
	{
		int num = 1;
		int layerCount = _animatorData.LayerCount;
		while (num < layerCount)
		{
			ReinterpretState<FloatCompressed>(wordOffset) = Animator.GetLayerWeight(num);
			num++;
			wordOffset++;
		}
	}

	private void ApplyLayerWeights(NetworkBehaviourBuffer buffer, ref int wordOffset)
	{
		int num = 1;
		int layerCount = _animatorData.LayerCount;
		while (num < layerCount)
		{
			Animator.SetLayerWeight(num, buffer.ReinterpretState<FloatCompressed>(wordOffset));
			num++;
			wordOffset++;
		}
	}

	private void EnsureInitialized()
	{
		if (_isInitialized)
		{
			return;
		}
		if (!Animator)
		{
			Animator = GetComponent<Animator>();
		}
		if (!Animator || !Animator.gameObject.activeSelf)
		{
			_animatorData = default(AnimatorData);
			return;
		}
		_animatorData = new AnimatorData(Animator, SyncSettings);
		_isInitialized = true;
		if (TotalWords != _animatorData.WordCount)
		{
			InternalLogStreams.LogWarn?.Log("Baked and runtime word counts don't match! Does the prefab need to be reimported?");
		}
	}
}
