using UnityEngine.UI;

namespace UnityEngine.Rendering.UI;

public class DebugUIHandlerProgressBar : DebugUIHandlerWidget
{
	public Text nameLabel;

	public Text valueLabel;

	public RectTransform progressBarRect;

	private DebugUI.ProgressBarValue m_Value;

	private float m_Timer;

	protected override void OnEnable()
	{
		m_Timer = 0f;
	}

	internal override void SetWidget(DebugUI.Widget widget)
	{
		base.SetWidget(widget);
		m_Value = CastWidget<DebugUI.ProgressBarValue>();
		nameLabel.text = m_Value.displayName;
		UpdateValue();
	}

	public override bool OnSelection(bool fromNext, DebugUIHandlerWidget previous)
	{
		nameLabel.color = colorSelected;
		return true;
	}

	public override void OnDeselection()
	{
		nameLabel.color = colorDefault;
	}

	private void Update()
	{
		if (m_Timer >= m_Value.refreshRate)
		{
			UpdateValue();
			m_Timer -= m_Value.refreshRate;
		}
		m_Timer += Time.deltaTime;
	}

	private void UpdateValue()
	{
		float num = (float)m_Value.GetValue();
		valueLabel.text = m_Value.FormatString(num);
		Vector3 localScale = progressBarRect.localScale;
		localScale.x = num;
		progressBarRect.localScale = localScale;
	}
}
