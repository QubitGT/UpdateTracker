using System;

namespace UnityEngine.Rendering.UI;

public class DebugUIHandlerObjectList : DebugUIHandlerField<DebugUI.ObjectListField>
{
	private int m_Index;

	internal override void SetWidget(DebugUI.Widget widget)
	{
		base.SetWidget(widget);
		m_Index = 0;
	}

	public override void OnIncrement(bool fast)
	{
		m_Index++;
		UpdateValueLabel();
	}

	public override void OnDecrement(bool fast)
	{
		m_Index--;
		UpdateValueLabel();
	}

	public override void UpdateValueLabel()
	{
		string labelText = "Empty";
		Object[] value = m_Field.GetValue();
		if (value != null)
		{
			m_Index = Math.Clamp(m_Index, 0, value.Length - 1);
			labelText = value[m_Index].name;
		}
		SetLabelText(labelText);
	}
}
