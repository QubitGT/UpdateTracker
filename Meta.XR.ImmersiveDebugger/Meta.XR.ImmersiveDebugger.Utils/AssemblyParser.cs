using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;

namespace Meta.XR.ImmersiveDebugger.Utils;

internal static class AssemblyParser
{
	private static List<Type> _types = new List<Type>();

	private static bool _assembliesParsed = false;

	private static Func<bool> _enabledDelegate = GetImmersiveDebuggerEnabled;

	private static Func<Assembly[]> _assembliesDelegate = GetAllAssemblies;

	private static RuntimeSettings _prebakedRuntimeSettings;

	public static bool Ready => _assembliesParsed;

	public static bool Enabled => _enabledDelegate();

	private static event Action<List<Type>> OnAssemblyParsed;

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	private static void Init()
	{
		_types?.Clear();
		_assembliesParsed = false;
		_prebakedRuntimeSettings = null;
	}

	private static bool GetImmersiveDebuggerEnabled()
	{
		return RuntimeSettings.Instance.ImmersiveDebuggerEnabled;
	}

	private static Assembly[] GetAllAssemblies()
	{
		return AppDomain.CurrentDomain.GetAssemblies();
	}

	[RuntimeInitializeOnLoadMethod]
	private static void OnLoad()
	{
		Refresh();
	}

	private static void RefreshWhenPlaying()
	{
		Refresh();
	}

	public static void Refresh(bool ignorePrebakedAsset = false)
	{
		if (Enabled)
		{
			LoadAssembliesMainThread(ignorePrebakedAsset);
		}
	}

	private static async Task LoadAssembliesMainThread(bool ignorePrebakedAsset)
	{
		_assembliesParsed = false;
		_types.Clear();
		_prebakedRuntimeSettings = ((!ignorePrebakedAsset) ? RuntimeSettings.Instance : null);
		await Task.Run((Func<Task>)LoadAssembliesAsync);
		AssemblyParser.OnAssemblyParsed?.Invoke(_types);
		_assembliesParsed = true;
	}

	private static Task LoadAssembliesAsync()
	{
		Assembly[] array = _assembliesDelegate();
		foreach (Assembly assembly in array)
		{
			if (_prebakedRuntimeSettings != null)
			{
				if (!_prebakedRuntimeSettings.debugTypesDict.ContainsKey(assembly.GetName().Name))
				{
					continue;
				}
				foreach (string item in _prebakedRuntimeSettings.debugTypesDict[assembly.GetName().Name])
				{
					try
					{
						_types.Add(assembly.GetType(item, throwOnError: true));
					}
					catch (Exception)
					{
						Debug.LogWarning("Immersive Debugger cannot get " + item + " type from assembly " + assembly.GetName().Name + ", skipping");
					}
				}
				continue;
			}
			foreach (Type item2 in from t in assembly.GetTypes()
				where t.GetMembers(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).Any((MemberInfo m) => m.GetCustomAttribute<DebugMember>() != null)
				select t)
			{
				_types.Add(item2);
			}
		}
		return Task.CompletedTask;
	}

	public static void RegisterAssemblyTypes(Action<List<Type>> del)
	{
		if (Ready)
		{
			del?.Invoke(_types);
		}
		OnAssemblyParsed -= del;
		OnAssemblyParsed += del;
	}

	public static void Unregister(Action<List<Type>> del)
	{
		OnAssemblyParsed -= del;
	}
}
