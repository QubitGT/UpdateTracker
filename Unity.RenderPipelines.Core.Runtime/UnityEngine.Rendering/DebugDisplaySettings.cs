using System;
using System.Collections.Generic;

namespace UnityEngine.Rendering;

public abstract class DebugDisplaySettings<T> : IDebugDisplaySettings where T : IDebugDisplaySettings, new()
{
	private class IDebugDisplaySettingsDataComparer : IEqualityComparer<IDebugDisplaySettingsData>
	{
		public bool Equals(IDebugDisplaySettingsData x, IDebugDisplaySettingsData y)
		{
			if (x == y)
			{
				return true;
			}
			if (x == null || y == null)
			{
				return false;
			}
			return x.GetType() == y.GetType();
		}

		public int GetHashCode(IDebugDisplaySettingsData obj)
		{
			return 17 * 23 + obj.GetType().GetHashCode();
		}
	}

	protected readonly HashSet<IDebugDisplaySettingsData> m_Settings = new HashSet<IDebugDisplaySettingsData>(new IDebugDisplaySettingsDataComparer());

	private static readonly Lazy<T> s_Instance = new Lazy<T>(delegate
	{
		T result = new T();
		result.Reset();
		return result;
	});

	public static T Instance => s_Instance.Value;

	public virtual bool AreAnySettingsActive
	{
		get
		{
			foreach (IDebugDisplaySettingsData setting in m_Settings)
			{
				if (setting.AreAnySettingsActive)
				{
					return true;
				}
			}
			return false;
		}
	}

	public virtual bool IsPostProcessingAllowed
	{
		get
		{
			bool flag = true;
			foreach (IDebugDisplaySettingsData setting in m_Settings)
			{
				flag &= setting.IsPostProcessingAllowed;
			}
			return flag;
		}
	}

	public virtual bool IsLightingActive
	{
		get
		{
			bool flag = true;
			foreach (IDebugDisplaySettingsData setting in m_Settings)
			{
				flag &= setting.IsLightingActive;
			}
			return flag;
		}
	}

	protected TData Add<TData>(TData newData) where TData : IDebugDisplaySettingsData
	{
		m_Settings.Add(newData);
		return newData;
	}

	IDebugDisplaySettingsData IDebugDisplaySettings.Add(IDebugDisplaySettingsData newData)
	{
		m_Settings.Add(newData);
		return newData;
	}

	public void ForEach(Action<IDebugDisplaySettingsData> onExecute)
	{
		foreach (IDebugDisplaySettingsData setting in m_Settings)
		{
			onExecute(setting);
		}
	}

	public virtual void Reset()
	{
		foreach (IDebugDisplaySettingsData setting in m_Settings)
		{
			setting.Reset();
		}
		m_Settings.Clear();
	}

	public virtual bool TryGetScreenClearColor(ref Color color)
	{
		foreach (IDebugDisplaySettingsData setting in m_Settings)
		{
			if (setting.TryGetScreenClearColor(ref color))
			{
				return true;
			}
		}
		return false;
	}
}
