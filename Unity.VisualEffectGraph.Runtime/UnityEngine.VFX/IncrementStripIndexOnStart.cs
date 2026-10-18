using System;

namespace UnityEngine.VFX;

internal class IncrementStripIndexOnStart : VFXSpawnerCallbacks
{
	public class InputProperties
	{
		[Tooltip("Maximum Strip Count (Used to cycle indices)")]
		public uint StripMaxCount = 8u;
	}

	private static readonly int stripMaxCountID = Shader.PropertyToID("StripMaxCount");

	private static readonly int stripIndexID = Shader.PropertyToID("stripIndex");

	private uint m_Index;

	public override void OnPlay(VFXSpawnerState state, VFXExpressionValues vfxValues, VisualEffect vfxComponent)
	{
		m_Index = (m_Index + 1) % Math.Max(1u, vfxValues.GetUInt(stripMaxCountID));
		state.vfxEventAttribute.SetUint(stripIndexID, m_Index);
	}

	public override void OnStop(VFXSpawnerState state, VFXExpressionValues vfxValues, VisualEffect vfxComponent)
	{
		m_Index = 0u;
	}

	public override void OnUpdate(VFXSpawnerState state, VFXExpressionValues vfxValues, VisualEffect vfxComponent)
	{
	}
}
