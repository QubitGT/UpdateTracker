using UnityEngine.UI;

namespace UnityEngine.Rendering.UI;

public class DebugUIHandlerValue : DebugUIHandlerWidget
{
	public Text nameLabel;

	public Text valueLabel;

	private DebugUI.Value m_Field;

	protected internal float m_Timer;

	private static readonly Color k_ZeroColor = Color.gray;

	protected override void OnEnable()
	{
		m_Timer = 0f;
	}

	internal override void SetWidget(DebugUI.Widget widget)
	{
		base.SetWidget(widget);
		m_Field = CastWidget<DebugUI.Value>();
		nameLabel.text = m_Field.displayName;
	}

	public override bool OnSelection(bool fromNext, DebugUIHandlerWidget previous)
	{
		nameLabel.color = colorSelected;
		valueLabel.color = colorSelected;
		return true;
	}

	public override void OnDeselection()
	{
		nameLabel.color = colorDefault;
		valueLabel.color = colorDefault;
	}

	private void Update()
	{
		if (m_Timer >= m_Field.refreshRate)
		{
			object value = m_Field.GetValue();
			valueLabel.text = m_Field.FormatString(value);
			if (value is float)
			{
				valueLabel.color = (((float)value == 0f) ? k_ZeroColor : colorDefault);
			}
			m_Timer -= m_Field.refreshRate;
		}
		m_Timer += Time.deltaTime;
	}
}
