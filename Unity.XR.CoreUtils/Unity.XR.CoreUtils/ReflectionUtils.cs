using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Unity.XR.CoreUtils;

public static class ReflectionUtils
{
	private static Assembly[] s_Assemblies;

	private static List<Type[]> s_TypesPerAssembly;

	private static List<Dictionary<string, Type>> s_AssemblyTypeMaps;

	private static Assembly[] GetCachedAssemblies()
	{
		return s_Assemblies ?? (s_Assemblies = AppDomain.CurrentDomain.GetAssemblies());
	}

	private static List<Type[]> GetCachedTypesPerAssembly()
	{
		if (s_TypesPerAssembly == null)
		{
			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
			s_TypesPerAssembly = new List<Type[]>(assemblies.Length);
			Assembly[] array = assemblies;
			foreach (Assembly assembly in array)
			{
				try
				{
					s_TypesPerAssembly.Add(assembly.GetTypes());
				}
				catch (ReflectionTypeLoadException)
				{
				}
			}
		}
		return s_TypesPerAssembly;
	}

	private static List<Dictionary<string, Type>> GetCachedAssemblyTypeMaps()
	{
		if (s_AssemblyTypeMaps == null)
		{
			List<Type[]> cachedTypesPerAssembly = GetCachedTypesPerAssembly();
			s_AssemblyTypeMaps = new List<Dictionary<string, Type>>(cachedTypesPerAssembly.Count);
			foreach (Type[] item in cachedTypesPerAssembly)
			{
				try
				{
					Dictionary<string, Type> dictionary = new Dictionary<string, Type>();
					Type[] array = item;
					foreach (Type type in array)
					{
						dictionary[type.FullName] = type;
					}
					s_AssemblyTypeMaps.Add(dictionary);
				}
				catch (ReflectionTypeLoadException)
				{
				}
			}
		}
		return s_AssemblyTypeMaps;
	}

	public static void PreWarmTypeCache()
	{
		GetCachedAssemblyTypeMaps();
	}

	public static void ForEachAssembly(Action<Assembly> callback)
	{
		Assembly[] cachedAssemblies = GetCachedAssemblies();
		foreach (Assembly obj in cachedAssemblies)
		{
			try
			{
				callback(obj);
			}
			catch (ReflectionTypeLoadException)
			{
			}
		}
	}

	public static void ForEachType(Action<Type> callback)
	{
		foreach (Type[] item in GetCachedTypesPerAssembly())
		{
			foreach (Type obj in item)
			{
				callback(obj);
			}
		}
	}

	public static Type FindType(Func<Type, bool> predicate)
	{
		foreach (Type[] item in GetCachedTypesPerAssembly())
		{
			foreach (Type type in item)
			{
				if (predicate(type))
				{
					return type;
				}
			}
		}
		return null;
	}

	public static Type FindTypeByFullName(string fullName)
	{
		foreach (Dictionary<string, Type> cachedAssemblyTypeMap in GetCachedAssemblyTypeMaps())
		{
			if (cachedAssemblyTypeMap.TryGetValue(fullName, out var value))
			{
				return value;
			}
		}
		return null;
	}

	public static void FindTypesBatch(List<Func<Type, bool>> predicates, List<Type> resultList)
	{
		List<Type[]> cachedTypesPerAssembly = GetCachedTypesPerAssembly();
		for (int i = 0; i < predicates.Count; i++)
		{
			Func<Type, bool> func = predicates[i];
			foreach (Type[] item in cachedTypesPerAssembly)
			{
				foreach (Type type in item)
				{
					if (func(type))
					{
						resultList[i] = type;
					}
				}
			}
		}
	}

	public static void FindTypesByFullNameBatch(List<string> typeNames, List<Type> resultList)
	{
		List<Dictionary<string, Type>> cachedAssemblyTypeMaps = GetCachedAssemblyTypeMaps();
		foreach (string typeName in typeNames)
		{
			bool flag = false;
			foreach (Dictionary<string, Type> item in cachedAssemblyTypeMaps)
			{
				if (item.TryGetValue(typeName, out var value))
				{
					resultList.Add(value);
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				resultList.Add(null);
			}
		}
	}

	public static Type FindTypeInAssemblyByFullName(string assemblyName, string typeName)
	{
		Assembly[] cachedAssemblies = GetCachedAssemblies();
		List<Dictionary<string, Type>> cachedAssemblyTypeMaps = GetCachedAssemblyTypeMaps();
		for (int i = 0; i < cachedAssemblies.Length; i++)
		{
			if (!(cachedAssemblies[i].GetName().Name != assemblyName))
			{
				if (!cachedAssemblyTypeMaps[i].TryGetValue(typeName, out var value))
				{
					return null;
				}
				return value;
			}
		}
		return null;
	}

	public static string NicifyVariableName(string name)
	{
		if (name.StartsWith("m_"))
		{
			name = name.Substring(2, name.Length - 2);
		}
		else if (name.StartsWith("_"))
		{
			name = name.Substring(1, name.Length - 1);
		}
		if (name[0] == 'k' && name[1] >= 'A' && name[1] <= 'Z')
		{
			name = name.Substring(1, name.Length - 1);
		}
		name = Regex.Replace(name, "(\\B[A-Z]+?(?=[A-Z][^A-Z])|\\B[A-Z]+?(?=[^A-Z]))", " $1");
		name = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(name);
		return name;
	}
}
