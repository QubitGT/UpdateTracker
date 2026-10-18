using System;
using Unity.XR.CoreUtils;

namespace UnityEngine.XR.Interaction.Toolkit;

[Serializable]
public struct InteractionLayerMask : ISerializationCallbackReceiver
{
	[SerializeField]
	private uint m_Bits;

	private int m_Mask;

	public int value
	{
		get
		{
			return m_Mask;
		}
		set
		{
			m_Mask = value;
			m_Bits = (uint)value;
		}
	}

	public static implicit operator int(InteractionLayerMask mask)
	{
		return mask.m_Mask;
	}

	public static implicit operator InteractionLayerMask(int intVal)
	{
		InteractionLayerMask result = default(InteractionLayerMask);
		result.m_Mask = intVal;
		result.m_Bits = (uint)intVal;
		return result;
	}

	public static string LayerToName(int layer)
	{
		if (layer < 0 || layer >= 32)
		{
			return string.Empty;
		}
		return ScriptableSettings<InteractionLayerSettings>.Instance.GetLayerNameAt(layer);
	}

	public static int NameToLayer(string layerName)
	{
		return ScriptableSettings<InteractionLayerSettings>.Instance.GetLayer(layerName);
	}

	public static int GetMask(params string[] layerNames)
	{
		if (layerNames == null)
		{
			throw new ArgumentNullException("layerNames");
		}
		int num = 0;
		for (int i = 0; i < layerNames.Length; i++)
		{
			int num2 = NameToLayer(layerNames[i]);
			if (num2 != -1)
			{
				num |= 1 << num2;
			}
		}
		return num;
	}

	public void OnAfterDeserialize()
	{
		m_Mask = (int)m_Bits;
	}

	public void OnBeforeSerialize()
	{
	}
}
