using System.Collections.Generic;

namespace UnityEngine.Rendering.UI;

internal class DebugUIHandlerPersistentCanvas : MonoBehaviour
{
	public RectTransform panel;

	public RectTransform valuePrefab;

	private List<DebugUIHandlerValue> m_Items = new List<DebugUIHandlerValue>();

	private List<DebugUI.ValueTuple> m_ValueTupleWidgets = new List<DebugUI.ValueTuple>();

	internal void Toggle(DebugUI.Value widget, string displayName = null)
	{
		int num = m_Items.FindIndex((DebugUIHandlerValue x) => x.GetWidget() == widget);
		if (num > -1)
		{
			CoreUtils.Destroy(m_Items[num].gameObject);
			m_Items.RemoveAt(num);
			return;
		}
		DebugUIHandlerValue component = Object.Instantiate(valuePrefab, panel, worldPositionStays: false).gameObject.GetComponent<DebugUIHandlerValue>();
		component.SetWidget(widget);
		component.nameLabel.text = (string.IsNullOrEmpty(displayName) ? widget.displayName : displayName);
		m_Items.Add(component);
	}

	internal void Toggle(DebugUI.ValueTuple widget, int? forceTupleIndex = null)
	{
		DebugUI.ValueTuple valueTuple = m_ValueTupleWidgets.Find((DebugUI.ValueTuple x) => x == widget);
		int num = valueTuple?.pinnedElementIndex ?? (-1);
		if (valueTuple != null)
		{
			m_ValueTupleWidgets.Remove(valueTuple);
			Toggle(widget.values[num]);
		}
		if (forceTupleIndex.HasValue)
		{
			num = forceTupleIndex.Value;
		}
		if (num + 1 < widget.numElements)
		{
			widget.pinnedElementIndex = num + 1;
			string text = widget.displayName;
			if (widget.parent is DebugUI.Foldout)
			{
				string[] columnLabels = (widget.parent as DebugUI.Foldout).columnLabels;
				if (columnLabels != null && widget.pinnedElementIndex < columnLabels.Length)
				{
					text = text + " (" + columnLabels[widget.pinnedElementIndex] + ")";
				}
			}
			Toggle(widget.values[widget.pinnedElementIndex], text);
			m_ValueTupleWidgets.Add(widget);
		}
		else
		{
			widget.pinnedElementIndex = -1;
		}
	}

	internal bool IsEmpty()
	{
		return m_Items.Count == 0;
	}

	internal void Clear()
	{
		foreach (DebugUIHandlerValue item in m_Items)
		{
			CoreUtils.Destroy(item.gameObject);
		}
		m_Items.Clear();
	}
}
