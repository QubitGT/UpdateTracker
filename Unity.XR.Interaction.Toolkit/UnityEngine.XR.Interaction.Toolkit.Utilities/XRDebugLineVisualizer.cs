using System;
using System.Collections.Generic;

namespace UnityEngine.XR.Interaction.Toolkit.Utilities;

[AddComponentMenu("")]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.Utilities.XRDebugLineVisualizer.html")]
internal class XRDebugLineVisualizer : MonoBehaviour
{
	[Serializable]
	private class DebugLine
	{
		public string name;

		public Color color;

		public LineRenderer lineRenderer;

		public float decayTime;
	}

	private List<DebugLine> m_DebugLines = new List<DebugLine>();

	private void Update()
	{
		for (int num = m_DebugLines.Count - 1; num >= 0; num--)
		{
			m_DebugLines[num].decayTime -= Time.deltaTime;
			if (m_DebugLines[num].decayTime <= 0f)
			{
				Object.Destroy(m_DebugLines[num].lineRenderer.gameObject);
				m_DebugLines.RemoveAt(num);
			}
		}
	}

	private void OnDestroy()
	{
		ClearLines();
	}

	public void UpdateOrCreateLine(string lineName, Vector3 start, Vector3 end, Color color, float decayTime = 0.2f)
	{
		DebugLine debugLine = m_DebugLines.Find((DebugLine l) => l.name == lineName);
		if (debugLine == null)
		{
			GameObject obj = new GameObject(lineName + "Line");
			obj.transform.SetParent(base.transform, worldPositionStays: false);
			LineRenderer lineRenderer = obj.AddComponent<LineRenderer>();
			lineRenderer.startWidth = 0.01f;
			lineRenderer.endWidth = 0.01f;
			lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
			lineRenderer.startColor = color;
			lineRenderer.endColor = color;
			debugLine = new DebugLine
			{
				name = lineName,
				color = color,
				lineRenderer = lineRenderer
			};
			m_DebugLines.Add(debugLine);
		}
		debugLine.lineRenderer.SetPosition(0, start);
		debugLine.lineRenderer.SetPosition(1, end);
		debugLine.decayTime = decayTime;
	}

	public void ClearLines()
	{
		foreach (DebugLine debugLine in m_DebugLines)
		{
			Object.Destroy(debugLine.lineRenderer.gameObject);
		}
		m_DebugLines.Clear();
	}
}
