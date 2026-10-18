using UnityEngine.UI;

namespace UnityEngine.Rendering.UI;

public abstract class DebugUIHandlerField<T> : DebugUIHandlerWidget where T : DebugUI.Widget
{
	public Text nextButtonText;

	public Text previousButtonText;

	public Text nameLabel;

	public Text valueLabel;

	protected internal T m_Field;

	internal override void SetWidget(DebugUI.Widget widget)
	{
		base.SetWidget(widget);
		m_Field = CastWidget<T>();
		nameLabel.text = m_Field.displayName;
		UpdateValueLabel();
	}

	public override bool OnSelection(bool fromNext, DebugUIHandlerWidget previous)
	{
		if (nextButtonText != null)
		{
			nextButtonText.color = colorSelected;
		}
		if (previousButtonText != null)
		{
			previousButtonText.color = colorSelected;
		}
		nameLabel.color = colorSelected;
		valueLabel.color = colorSelected;
		return true;
	}

	public override void OnDeselection()
	{
		if (nextButtonText != null)
		{
			nextButtonText.color = colorDefault;
		}
		if (previousButtonText != null)
		{
			previousButtonText.color = colorDefault;
		}
		nameLabel.color = colorDefault;
		valueLabel.color = colorDefault;
	}

	public override void OnAction()
	{
		OnIncrement(fast: false);
	}

	public abstract void UpdateValueLabel();

	protected void SetLabelText(string text)
	{
		if (text.Length > 26)
		{
			text = text.Substring(0, 23) + "...";
		}
		valueLabel.text = text;
	}
}
