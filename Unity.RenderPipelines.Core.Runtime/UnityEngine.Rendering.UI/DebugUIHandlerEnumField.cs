namespace UnityEngine.Rendering.UI;

public class DebugUIHandlerEnumField : DebugUIHandlerField<DebugUI.EnumField>
{
	public override void OnIncrement(bool fast)
	{
		if (m_Field.enumValues.Length == 0)
		{
			return;
		}
		int[] enumValues = m_Field.enumValues;
		int currentIndex = m_Field.currentIndex;
		if (currentIndex == enumValues.Length - 1)
		{
			currentIndex = 0;
		}
		else if (fast)
		{
			int[] quickSeparators = m_Field.quickSeparators;
			if (quickSeparators == null)
			{
				m_Field.InitQuickSeparators();
				quickSeparators = m_Field.quickSeparators;
			}
			int i;
			for (i = 0; i < quickSeparators.Length && currentIndex + 1 > quickSeparators[i]; i++)
			{
			}
			currentIndex = ((i != quickSeparators.Length) ? quickSeparators[i] : 0);
		}
		else
		{
			currentIndex++;
		}
		m_Field.SetValue(enumValues[currentIndex]);
		m_Field.currentIndex = currentIndex;
		UpdateValueLabel();
	}

	public override void OnDecrement(bool fast)
	{
		if (m_Field.enumValues.Length == 0)
		{
			return;
		}
		int[] enumValues = m_Field.enumValues;
		int currentIndex = m_Field.currentIndex;
		if (currentIndex == 0)
		{
			if (fast)
			{
				int[] quickSeparators = m_Field.quickSeparators;
				if (quickSeparators == null)
				{
					m_Field.InitQuickSeparators();
					quickSeparators = m_Field.quickSeparators;
				}
				currentIndex = quickSeparators[^1];
			}
			else
			{
				currentIndex = enumValues.Length - 1;
			}
		}
		else if (fast)
		{
			int[] quickSeparators2 = m_Field.quickSeparators;
			if (quickSeparators2 == null)
			{
				m_Field.InitQuickSeparators();
				quickSeparators2 = m_Field.quickSeparators;
			}
			int num = quickSeparators2.Length - 1;
			while (num > 0 && currentIndex <= quickSeparators2[num])
			{
				num--;
			}
			currentIndex = quickSeparators2[num];
		}
		else
		{
			currentIndex--;
		}
		m_Field.SetValue(enumValues[currentIndex]);
		m_Field.currentIndex = currentIndex;
		UpdateValueLabel();
	}

	public override void UpdateValueLabel()
	{
		int num = m_Field.currentIndex;
		if (num < 0)
		{
			num = 0;
		}
		SetLabelText(m_Field.enumNames[num].text);
	}
}
