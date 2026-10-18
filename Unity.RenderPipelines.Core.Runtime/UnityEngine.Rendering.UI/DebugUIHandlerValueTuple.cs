using UnityEngine.UI;

namespace UnityEngine.Rendering.UI;

public class DebugUIHandlerValueTuple : DebugUIHandlerWidget
{
	public Text nameLabel;

	public Text valueLabel;

	protected internal DebugUI.ValueTuple m_Field;

	protected internal Text[] valueElements;

	private const float k_XOffset = 230f;

	private float m_Timer;

	private static readonly Color k_ZeroColor = Color.gray;

	protected override void OnEnable()
	{
		m_Timer = 0f;
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

	internal override void SetWidget(DebugUI.Widget widget)
	{
		m_Widget = widget;
		m_Field = CastWidget<DebugUI.ValueTuple>();
		nameLabel.text = m_Field.displayName;
		int numElements = m_Field.numElements;
		valueElements = new Text[numElements];
		valueElements[0] = valueLabel;
		float num = 230f / (float)numElements;
		for (int i = 1; i < numElements; i++)
		{
			GameObject gameObject = Object.Instantiate(valueLabel.gameObject, base.transform);
			gameObject.AddComponent<LayoutElement>().ignoreLayout = true;
			RectTransform obj = gameObject.transform as RectTransform;
			RectTransform rectTransform = nameLabel.transform as RectTransform;
			Vector2 anchorMax = (obj.anchorMin = new Vector2(0f, 1f));
			obj.anchorMax = anchorMax;
			obj.sizeDelta = new Vector2(100f, 26f);
			Vector3 vector2 = rectTransform.anchoredPosition;
			vector2.x += (float)(i + 1) * num + 200f;
			obj.anchoredPosition = vector2;
			obj.pivot = new Vector2(0f, 1f);
			valueElements[i] = gameObject.GetComponent<Text>();
		}
	}

	internal virtual void UpdateValueLabels()
	{
		for (int i = 0; i < m_Field.numElements; i++)
		{
			if (i < valueElements.Length && valueElements[i] != null)
			{
				object value = m_Field.values[i].GetValue();
				valueElements[i].text = m_Field.values[i].FormatString(value);
				if (value is float)
				{
					valueElements[i].color = (((float)value == 0f) ? k_ZeroColor : colorDefault);
				}
			}
		}
	}

	private void Update()
	{
		if (m_Field != null && m_Timer >= m_Field.refreshRate)
		{
			UpdateValueLabels();
			m_Timer -= m_Field.refreshRate;
		}
		m_Timer += Time.deltaTime;
	}
}
