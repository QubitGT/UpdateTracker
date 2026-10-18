using System;
using System.Collections.Generic;
using System.Reflection;

namespace UnityEngine.Rendering;

public abstract class DebugDisplaySettingsPanel : IDebugDisplaySettingsPanelDisposable, IDebugDisplaySettingsPanel, IDisposable
{
	private readonly List<DebugUI.Widget> m_Widgets = new List<DebugUI.Widget>();

	private readonly DisplayInfoAttribute m_DisplayInfo;

	public virtual string PanelName => m_DisplayInfo?.name ?? string.Empty;

	public virtual int Order => m_DisplayInfo?.order ?? 0;

	public DebugUI.Widget[] Widgets => m_Widgets.ToArray();

	public virtual DebugUI.Flags Flags => DebugUI.Flags.None;

	protected void AddWidget(DebugUI.Widget widget)
	{
		if (widget == null)
		{
			throw new ArgumentNullException("widget");
		}
		m_Widgets.Add(widget);
	}

	protected void Clear()
	{
		m_Widgets.Clear();
	}

	public virtual void Dispose()
	{
		Clear();
	}

	protected DebugDisplaySettingsPanel()
	{
		m_DisplayInfo = GetType().GetCustomAttribute<DisplayInfoAttribute>();
		if (m_DisplayInfo == null)
		{
			Debug.Log(string.Format("Type {0} should specify the attribute {1}", GetType(), "DisplayInfoAttribute"));
		}
	}
}
public abstract class DebugDisplaySettingsPanel<T> : DebugDisplaySettingsPanel where T : IDebugDisplaySettingsData
{
	internal T m_Data;

	public T data
	{
		get
		{
			return m_Data;
		}
		internal set
		{
			m_Data = value;
		}
	}

	protected DebugDisplaySettingsPanel(T data)
	{
		m_Data = data;
	}
}
