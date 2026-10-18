using UnityEngine.UI;

namespace UnityEngine.Rendering.UI;

public class DebugUIHandlerRow : DebugUIHandlerFoldout
{
	private float m_Timer;

	protected override void OnEnable()
	{
		m_Timer = 0f;
	}

	private GameObject GetChild(int index)
	{
		if (index < 0)
		{
			return null;
		}
		if (base.gameObject.transform != null)
		{
			Transform child = base.gameObject.transform.GetChild(1);
			if (child != null && child.childCount > index)
			{
				return child.GetChild(index).gameObject;
			}
		}
		return null;
	}

	private bool TryGetChild(int index, out GameObject child)
	{
		child = GetChild(index);
		return child != null;
	}

	private bool IsActive(DebugUI.Table table, int index, GameObject child)
	{
		if (table == null || !table.GetColumnVisibility(index))
		{
			return false;
		}
		Transform transform = child.transform.Find("Value");
		if (transform != null && transform.TryGetComponent<Text>(out var component))
		{
			return !string.IsNullOrEmpty(component.text);
		}
		return true;
	}

	protected void Update()
	{
		DebugUI.Table.Row row = CastWidget<DebugUI.Table.Row>();
		DebugUI.Table table = row.parent as DebugUI.Table;
		float num = 0.1f;
		bool flag = m_Timer >= num;
		if (flag)
		{
			m_Timer -= num;
		}
		m_Timer += Time.deltaTime;
		for (int i = 0; i < row.children.Count; i++)
		{
			if (!TryGetChild(i, out var child))
			{
				continue;
			}
			bool flag2 = IsActive(table, i, child);
			if (child != null)
			{
				child.SetActive(flag2);
			}
			if (flag2 && flag)
			{
				if (child.TryGetComponent<DebugUIHandlerColor>(out var component))
				{
					component.UpdateColor();
				}
				if (child.TryGetComponent<DebugUIHandlerToggle>(out var component2))
				{
					component2.UpdateValueLabel();
				}
				if (child.TryGetComponent<DebugUIHandlerObjectList>(out var component3))
				{
					component3.UpdateValueLabel();
				}
			}
		}
		DebugUIHandlerWidget debugUIHandlerWidget = GetChild(0).GetComponent<DebugUIHandlerWidget>();
		DebugUIHandlerWidget debugUIHandlerWidget2 = null;
		for (int j = 0; j < row.children.Count; j++)
		{
			debugUIHandlerWidget.previousUIHandler = debugUIHandlerWidget2;
			if (!TryGetChild(j, out var child2))
			{
				continue;
			}
			if (IsActive(table, j, child2))
			{
				debugUIHandlerWidget2 = debugUIHandlerWidget;
			}
			bool flag3 = false;
			for (int k = j + 1; k < row.children.Count; k++)
			{
				if (TryGetChild(k, out var child3) && IsActive(table, k, child3))
				{
					DebugUIHandlerWidget debugUIHandlerWidget3 = (debugUIHandlerWidget.nextUIHandler = child2.GetComponent<DebugUIHandlerWidget>());
					debugUIHandlerWidget = debugUIHandlerWidget3;
					j = k - 1;
					flag3 = true;
					break;
				}
			}
			if (!flag3)
			{
				debugUIHandlerWidget.nextUIHandler = null;
				break;
			}
		}
	}
}
