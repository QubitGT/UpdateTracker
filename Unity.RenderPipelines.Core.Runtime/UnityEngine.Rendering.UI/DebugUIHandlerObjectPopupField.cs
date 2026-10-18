using System.Collections.Generic;
using System.Linq;

namespace UnityEngine.Rendering.UI;

public class DebugUIHandlerObjectPopupField : DebugUIHandlerField<DebugUI.ObjectPopupField>
{
	private int m_Index;

	internal override void SetWidget(DebugUI.Widget widget)
	{
		base.SetWidget(widget);
		m_Index = 0;
	}

	private void ChangeSelectedObject()
	{
		if (m_Field == null)
		{
			return;
		}
		IEnumerable<Object> enumerable = m_Field.getObjects();
		if (enumerable != null)
		{
			Object[] array = enumerable.ToArray();
			int num = array.Length;
			if (m_Index >= num)
			{
				m_Index = 0;
			}
			else if (m_Index < 0)
			{
				m_Index = num - 1;
			}
			Object value = array[m_Index];
			m_Field.SetValue(value);
			UpdateValueLabel();
		}
	}

	public override void OnIncrement(bool fast)
	{
		m_Index++;
		ChangeSelectedObject();
	}

	public override void OnDecrement(bool fast)
	{
		m_Index--;
		ChangeSelectedObject();
	}

	public override void UpdateValueLabel()
	{
		Object value = m_Field.GetValue();
		string labelText = ((value != null) ? value.name : "Empty");
		SetLabelText(labelText);
	}
}
