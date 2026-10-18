using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;

namespace UnityEngine.Rendering;

[Serializable]
public class VolumeComponent : ScriptableObject
{
	public sealed class Indent : PropertyAttribute
	{
		public readonly int relativeAmount;

		public Indent(int relativeAmount = 1)
		{
			this.relativeAmount = relativeAmount;
		}
	}

	public bool active = true;

	internal VolumeParameter[] parameterList;

	private ReadOnlyCollection<VolumeParameter> m_ParameterReadOnlyCollection;

	public string displayName { get; protected set; } = "";


	public ReadOnlyCollection<VolumeParameter> parameters => m_ParameterReadOnlyCollection ?? (m_ParameterReadOnlyCollection = new ReadOnlyCollection<VolumeParameter>(parameterList));

	internal static void FindParameters(object o, List<VolumeParameter> parameters, Func<FieldInfo, bool> filter = null)
	{
		if (o == null)
		{
			return;
		}
		foreach (FieldInfo item2 in from t in o.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
			orderby t.MetadataToken
			select t)
		{
			Type fieldType = item2.FieldType;
			if (fieldType.IsSubclassOf(typeof(VolumeParameter)))
			{
				if (filter == null || filter(item2))
				{
					VolumeParameter item = (VolumeParameter)item2.GetValue(o);
					parameters.Add(item);
				}
			}
			else if (!fieldType.IsArray && fieldType.IsClass)
			{
				FindParameters(item2.GetValue(o), parameters, filter);
			}
		}
	}

	protected virtual void OnEnable()
	{
		ListPool<VolumeParameter>.Get(out var value);
		FindParameters(this, value);
		parameterList = value.ToArray();
		ListPool<VolumeParameter>.Release(value);
		VolumeParameter[] array = parameterList;
		foreach (VolumeParameter volumeParameter in array)
		{
			if (volumeParameter != null)
			{
				volumeParameter.OnEnable();
			}
			else
			{
				Debug.LogWarning("Volume Component " + GetType().Name + " contains a null parameter; please make sure all parameters are initialized to a default value. Until this is fixed the null parameters will not be considered by the system.");
			}
		}
	}

	protected virtual void OnDisable()
	{
		VolumeParameter[] array = parameterList;
		for (int i = 0; i < array.Length; i++)
		{
			array[i]?.OnDisable();
		}
	}

	public virtual void Override(VolumeComponent state, float interpFactor)
	{
		int num = parameterList.Length;
		for (int i = 0; i < num; i++)
		{
			VolumeParameter volumeParameter = state.parameterList[i];
			VolumeParameter volumeParameter2 = parameterList[i];
			if (volumeParameter2.overrideState)
			{
				volumeParameter.overrideState = volumeParameter2.overrideState;
				volumeParameter.Interp(volumeParameter, volumeParameter2, interpFactor);
			}
		}
	}

	public void SetAllOverridesTo(bool state)
	{
		SetOverridesTo(parameterList, state);
	}

	internal void SetOverridesTo(IEnumerable<VolumeParameter> enumerable, bool state)
	{
		foreach (VolumeParameter item in enumerable)
		{
			item.overrideState = state;
			Type type = item.GetType();
			if (VolumeParameter.IsObjectParameter(type))
			{
				ReadOnlyCollection<VolumeParameter> readOnlyCollection = (ReadOnlyCollection<VolumeParameter>)type.GetProperty("parameters", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(item, null);
				if (readOnlyCollection != null)
				{
					SetOverridesTo(readOnlyCollection, state);
				}
			}
		}
	}

	public override int GetHashCode()
	{
		int num = 17;
		for (int i = 0; i < parameterList.Length; i++)
		{
			num = num * 23 + parameterList[i].GetHashCode();
		}
		return num;
	}

	public bool AnyPropertiesIsOverridden()
	{
		for (int i = 0; i < parameterList.Length; i++)
		{
			if (parameterList[i].overrideState)
			{
				return true;
			}
		}
		return false;
	}

	protected virtual void OnDestroy()
	{
		Release();
	}

	public void Release()
	{
		if (parameterList == null)
		{
			return;
		}
		for (int i = 0; i < parameterList.Length; i++)
		{
			if (parameterList[i] != null)
			{
				parameterList[i].Release();
			}
		}
	}
}
