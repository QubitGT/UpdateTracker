using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Meta.Voice.Logging;
using Meta.WitAi.Json;
using UnityEngine.Events;

namespace Meta.WitAi.Composer.Data;

public class ComposerContextMap : PluggableBase<IContextMapReservedPathExtension>
{
	internal static HashSet<string> ReservedPaths = new HashSet<string>();

	public WitResponseClass Data { get; private set; }

	public IVLogger Logger { get; } = LoggerRegistry.Instance.GetLogger(LogCategory.Composer);


	public UnityEvent OnContextMapChanged { get; } = new UnityEvent();


	public UnityEvent<string, string, string> OnContextMapValueChanged { get; } = new UnityEvent<string, string, string>();


	public UnityEvent<string> OnContextMapValueRemoved { get; } = new UnityEvent<string>();


	public ComposerContextMap()
	{
		PluggableBase<IContextMapReservedPathExtension>.CheckForPlugins();
		Data = new WitResponseClass();
	}

	public bool HasData(string key)
	{
		if (Data != null)
		{
			return Data.HasChild(key);
		}
		return false;
	}

	private WitResponseClass GetParentAndNodeName(string key, out string childNodeName)
	{
		string[] array = key.Split(".");
		WitResponseClass witResponseClass = Data;
		for (int i = 0; i < array.Length - 1; i++)
		{
			string text = array[i];
			if (!witResponseClass.HasChild(text))
			{
				GetArrayNameAndIndex(text, out var arrayName, out var arrayIndex);
				if (!string.IsNullOrEmpty(arrayName) && witResponseClass.HasChild(arrayName))
				{
					WitResponseArray asArray = witResponseClass[arrayName].AsArray;
					if (arrayIndex >= 0 && arrayIndex < asArray.Count)
					{
						witResponseClass = asArray[arrayIndex].AsObject;
						continue;
					}
					if (asArray.Count > 0)
					{
						witResponseClass = asArray[0].AsObject;
						continue;
					}
				}
			}
			witResponseClass = witResponseClass[text].AsObject;
		}
		childNodeName = array.Last();
		return witResponseClass;
	}

	private void GetArrayNameAndIndex(string childName, out string arrayName, out int arrayIndex)
	{
		int num = childName.IndexOf('[');
		if (num == -1)
		{
			arrayName = string.Empty;
			arrayIndex = -1;
			return;
		}
		arrayName = childName.Substring(0, num);
		string text = childName.Substring(num + 1);
		int num2 = text.IndexOf(']');
		if (num2 != -1 && int.TryParse(text.Substring(0, num2), out var result))
		{
			arrayIndex = result;
			return;
		}
		VLog.W(GetType().Name, "Could not determine array index for child: " + childName);
		arrayIndex = -1;
	}

	public T GetData<T>(string key, T defaultValue = default(T))
	{
		if (string.IsNullOrEmpty(key))
		{
			throw new ArgumentException("Invalid key");
		}
		string childNodeName;
		WitResponseClass parentAndNodeName = GetParentAndNodeName(key, out childNodeName);
		if (parentAndNodeName == null)
		{
			return defaultValue;
		}
		return parentAndNodeName.GetChild(childNodeName, defaultValue);
	}

	public void SetData<T>(string key, T newValue)
	{
		if (string.IsNullOrEmpty(key))
		{
			throw new ArgumentException("Invalid key");
		}
		string childNodeName;
		WitResponseClass parentAndNodeName = GetParentAndNodeName(key, out childNodeName);
		if (newValue is WitResponseNode value)
		{
			parentAndNodeName[childNodeName] = value;
		}
		else
		{
			parentAndNodeName[childNodeName] = JsonConvert.SerializeToken(newValue);
		}
		OnContextMapValueChanged?.Invoke(key, "", Data[key]);
		OnContextMapChanged?.Invoke();
	}

	public void ClearData(string key, bool notifyContextMapChanged = true)
	{
		if (!string.IsNullOrEmpty(key))
		{
			Data?.Remove(key);
			OnContextMapValueRemoved?.Invoke(key);
			if (notifyContextMapChanged)
			{
				OnContextMapChanged?.Invoke();
			}
		}
	}

	public void ClearAllNonReservedData()
	{
		string[] childNodeNames = Data.ChildNodeNames;
		foreach (string text in childNodeNames)
		{
			if (!ReservedPaths.Contains(text))
			{
				ClearData(text, notifyContextMapChanged: false);
			}
		}
		OnContextMapChanged?.Invoke();
	}

	public List<string> GetReservedPaths()
	{
		return ReservedPaths.ToList();
	}

	public string GetJson(bool ignoreEmptyFields = false)
	{
		if (Data == null)
		{
			return "{}";
		}
		try
		{
			return Data.ToString(ignoreEmptyFields);
		}
		catch (Exception arg)
		{
			VLog.E($"Composer Context Map - Decode Failed\n{arg}");
		}
		return "{}";
	}

	public override string ToString()
	{
		if (Data == null || Data.ChildNodeNames.Length == 0)
		{
			return "No Data";
		}
		StringBuilder stringBuilder = new StringBuilder();
		string[] childNodeNames = Data.ChildNodeNames;
		foreach (string text in childNodeNames)
		{
			stringBuilder.AppendLine("\t" + text + ": " + GetData(text, "-"));
		}
		return stringBuilder.ToString();
	}

	public bool UpdateData(WitResponseNode responseNode)
	{
		WitResponseClass witResponseClass = responseNode?["context_map"]?.AsObject;
		if (witResponseClass == null || witResponseClass.Count == 0)
		{
			return false;
		}
		bool num = UpdateDataObject(Data, witResponseClass);
		if (num)
		{
			UnityEvent onContextMapChanged = OnContextMapChanged;
			if (onContextMapChanged == null)
			{
				return num;
			}
			onContextMapChanged.Invoke();
		}
		return num;
	}

	private bool UpdateDataObject(WitResponseClass oldClass, WitResponseClass newClass)
	{
		bool result = false;
		string[] childNodeNames = newClass.ChildNodeNames;
		foreach (string text in childNodeNames)
		{
			WitResponseNode witResponseNode = oldClass[text];
			WitResponseNode witResponseNode2 = newClass[text];
			if (!ReservedPaths.Contains(text) && !WitResponseNode.Equals(witResponseNode, witResponseNode2))
			{
				result = true;
				Data[text] = witResponseNode2;
				Logger.Verbose("Update Context Map Key: '{0}'\nFrom: {1}\nTo: {2}", text, witResponseNode, witResponseNode2, null, "UpdateDataObject", ".\\Library\\PackageCache\\com.meta.xr.sdk.voice@d3f6f37b8e1c\\Features\\Composer\\Composer\\Lib\\Wit.ai\\Features\\composer\\Scripts\\Runtime\\Data\\ComposerContextMap.cs", 310);
				OnContextMapValueChanged?.Invoke(text, witResponseNode, witResponseNode2);
			}
		}
		return result;
	}

	public void CopyPersistentData(ComposerContextMap otherMap, ComposerService composerTarget)
	{
		if (otherMap.LoadedPlugins == null)
		{
			return;
		}
		LoadedPlugins = otherMap.LoadedPlugins;
		foreach (BaseReservedContextPath loadedPlugin in otherMap.LoadedPlugins)
		{
			loadedPlugin.AssignTo(composerTarget);
			loadedPlugin.UpdateContextMap();
		}
	}
}
