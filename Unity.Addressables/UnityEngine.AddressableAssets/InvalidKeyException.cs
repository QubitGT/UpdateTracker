using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace UnityEngine.AddressableAssets;

public class InvalidKeyException : Exception
{
	internal enum Format
	{
		StandardMessage,
		NoMergeMode,
		MultipleTypesRequested,
		NoLocation,
		TypeMismatch,
		MultipleTypeMismatch,
		MergeModeBase,
		UnionAvailableForKeys,
		UnionAvailableForKeysWithoutOther,
		IntersectionAvailable,
		KeyAvailableAsType
	}

	private AddressablesImpl m_Addressables;

	internal const string BaseInvalidKeyMessageFormat = "{0}, Key={1}, Type={2}";

	internal const string NoLocationMessageFormat = "{0} No Location found for Key={1}";

	internal const string MultipleTypeMismatchMessageFormat = "{0} No Asset found for Key={1} with Type={2}. Key exists as multiple Types={3}, which is not assignable from the requested Type={2}";

	internal const string TypeMismatchMessageFormat = "{0} No Asset found for Key={1} with Type={2}. Key exists as Type={3}, which is not assignable from the requested Type={2}";

	internal const string MultipleTypesMessageFormat = "{0} Enumerable key contains multiple Types. {1}, all Keys are expected to be strings";

	internal const string MergeModeNoLocationMessageFormat = "\nNo Location found for Key={0}";

	internal const string NoMergeModeMessageFormat = "{0} No MergeMode is set to merge the multiple keys requested. {1}, Type={2}";

	internal const string MergeModeBaseMessageFormat = "{0} No {1} of Assets between {2} with Type={3}";

	internal const string UnionAvailableForKeysMessageFormat = "\nUnion of Type={0} found with {1}";

	internal const string UnionAvailableForKeysWithoutOtherMessageFormat = "\nUnion of Type={0} found with {1}. Without {2}";

	internal const string IntersectionAvailableMessageFormat = "\nAn Intersection exists for Type={0}";

	internal const string KeyAvailableAsTypeMessageFormat = "\nType={0} exists for {1}";

	public object Key { get; private set; }

	public Type Type { get; private set; }

	public Addressables.MergeMode? MergeMode { get; }

	public override string Message
	{
		get
		{
			string text = Key as string;
			if (!string.IsNullOrEmpty(text))
			{
				if (m_Addressables == null)
				{
					return FormatMessage(Format.StandardMessage);
				}
				return GetMessageForSingleKey(text);
			}
			if (Key is IEnumerable enumerable)
			{
				int num = 0;
				List<string> list = new List<string>();
				HashSet<string> hashSet = new HashSet<string>();
				foreach (object item in enumerable)
				{
					num++;
					hashSet.Add(item.GetType().ToString());
					if (item is string)
					{
						list.Add(item as string);
					}
				}
				if (!MergeMode.HasValue)
				{
					string cSVString = GetCSVString(list, "Key=", "Keys=");
					FormatMergeModeMessage(Format.NoMergeMode);
					return $"{base.Message} No MergeMode is set to merge the multiple keys requested. {cSVString}, Type={Type}";
				}
				if (num != list.Count)
				{
					string cSVString2 = GetCSVString(hashSet, "Type=", "Types=");
					return FormatMessage(Format.MultipleTypesRequested, cSVString2);
				}
				if (num == 1)
				{
					return GetMessageForSingleKey(list[0]);
				}
				return GetMessageforMergeKeys(list);
			}
			return FormatMessage(Format.StandardMessage);
		}
	}

	public InvalidKeyException(object key)
		: this(key, typeof(object))
	{
	}

	public InvalidKeyException(object key, Type type)
	{
		Key = key;
		Type = type;
	}

	internal InvalidKeyException(object key, Type type, AddressablesImpl addr)
	{
		Key = key;
		Type = type;
		m_Addressables = addr;
	}

	public InvalidKeyException(object key, Type type, Addressables.MergeMode mergeMode)
	{
		Key = key;
		Type = type;
		MergeMode = mergeMode;
	}

	internal InvalidKeyException(object key, Type type, Addressables.MergeMode mergeMode, AddressablesImpl addr)
	{
		Key = key;
		Type = type;
		MergeMode = mergeMode;
		m_Addressables = addr;
	}

	public InvalidKeyException()
	{
	}

	public InvalidKeyException(string message)
		: base(message)
	{
	}

	public InvalidKeyException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected InvalidKeyException(SerializationInfo message, StreamingContext context)
		: base(message, context)
	{
	}

	internal string FormatMessage(Format format, string foundWithTypeString = null)
	{
		switch (format)
		{
		case Format.StandardMessage:
			return $"{base.Message}, Key={Key.ToString()}, Type={Type.FullName}";
		case Format.MultipleTypesRequested:
		{
			IEnumerable obj = Key as IEnumerable;
			string text = null;
			foreach (object item in obj)
			{
				text = ((text != null) ? (text + ", " + item.ToString()) : item.ToString());
			}
			return $"{base.Message} Enumerable key contains multiple Types. {text}, all Keys are expected to be strings";
		}
		case Format.NoLocation:
			return $"{base.Message} No Location found for Key={Key.ToString()}";
		case Format.TypeMismatch:
			return string.Format("{0} No Asset found for Key={1} with Type={2}. Key exists as Type={3}, which is not assignable from the requested Type={2}", base.Message, Key.ToString(), Type.FullName, foundWithTypeString);
		case Format.MultipleTypeMismatch:
			return string.Format("{0} No Asset found for Key={1} with Type={2}. Key exists as multiple Types={3}, which is not assignable from the requested Type={2}", base.Message, Key.ToString(), Type.FullName, foundWithTypeString);
		default:
			throw new ArgumentOutOfRangeException("format", format, null);
		}
	}

	internal string FormatMergeModeMessage(Format format, string keysAvailable = null, string keysUnavailable = null, string typeString = null)
	{
		return format switch
		{
			Format.NoLocation => $"\nNo Location found for Key={((keysUnavailable == null) ? GetKeyString() : keysUnavailable)}", 
			Format.NoMergeMode => $"{base.Message} No MergeMode is set to merge the multiple keys requested. {GetKeyString()}, Type={Type.FullName}", 
			Format.MergeModeBase => $"{base.Message} No {(MergeMode.HasValue ? MergeMode.Value : Addressables.MergeMode.None)} of Assets between {GetKeyString()} with Type={Type.FullName}", 
			Format.UnionAvailableForKeys => $"\nUnion of Type={typeString} found with {keysAvailable}", 
			Format.UnionAvailableForKeysWithoutOther => $"\nUnion of Type={typeString} found with {keysAvailable}. Without {keysUnavailable}", 
			Format.IntersectionAvailable => $"\nAn Intersection exists for Type={typeString}", 
			Format.KeyAvailableAsType => $"\nType={typeString} exists for {keysAvailable}", 
			_ => throw new ArgumentOutOfRangeException("format", format, null), 
		};
	}

	private string GetMessageForSingleKey(string keyString)
	{
		HashSet<Type> typesForKey = GetTypesForKey(keyString);
		if (typesForKey.Count == 0)
		{
			return FormatNotFoundMessage(keyString);
		}
		if (typesForKey.Count == 1)
		{
			return FormatTypeNotAssignableMessage(keyString, typesForKey);
		}
		return FormatMultipleAssignableTypesMessage(keyString, typesForKey);
	}

	private string FormatNotFoundMessage(string keyString)
	{
		return FormatMessage(Format.NoLocation);
	}

	private string FormatTypeNotAssignableMessage(string keyString, HashSet<Type> typesAvailableForKey)
	{
		Type type = null;
		foreach (Type item in typesAvailableForKey)
		{
			type = item;
		}
		if (type == null)
		{
			return FormatMessage(Format.StandardMessage);
		}
		return FormatMessage(Format.TypeMismatch, type.ToString());
	}

	private string FormatMultipleAssignableTypesMessage(string keyString, HashSet<Type> typesAvailableForKey)
	{
		StringBuilder stringBuilder = new StringBuilder(512);
		int num = 0;
		foreach (Type item in typesAvailableForKey)
		{
			num++;
			stringBuilder.Append((num > 1) ? $", {item}" : item.ToString());
		}
		return FormatMessage(Format.MultipleTypeMismatch, stringBuilder.ToString());
	}

	private string GetMessageforMergeKeys(List<string> keys)
	{
		StringBuilder stringBuilder = new StringBuilder(FormatMergeModeMessage(Format.MergeModeBase));
		switch (MergeMode)
		{
		case Addressables.MergeMode.Union:
		{
			Dictionary<Type, List<string>> dictionary2 = new Dictionary<Type, List<string>>();
			foreach (string key in keys)
			{
				if (!GetTypeToKeys(key, dictionary2))
				{
					stringBuilder.Append(FormatMergeModeMessage(Format.NoLocation, null, key));
				}
			}
			foreach (KeyValuePair<Type, List<string>> item in dictionary2)
			{
				string cSVString = GetCSVString(item.Value, "Key=", "Keys=");
				List<string> list = new List<string>();
				foreach (string key2 in keys)
				{
					if (!item.Value.Contains(key2))
					{
						list.Add(key2);
					}
				}
				if (list.Count == 0)
				{
					stringBuilder.Append(FormatMergeModeMessage(Format.UnionAvailableForKeys, cSVString, null, item.Key.ToString()));
					continue;
				}
				string cSVString2 = GetCSVString(list, "Key=", "Keys=");
				stringBuilder.Append(FormatMergeModeMessage(Format.UnionAvailableForKeysWithoutOther, cSVString, cSVString2, item.Key.ToString()));
			}
			break;
		}
		case Addressables.MergeMode.Intersection:
		{
			bool flag = false;
			Dictionary<Type, List<string>> dictionary3 = new Dictionary<Type, List<string>>();
			foreach (string key3 in keys)
			{
				if (!GetTypeToKeys(key3, dictionary3))
				{
					flag = true;
					stringBuilder.Append(FormatMergeModeMessage(Format.NoLocation, null, key3));
				}
			}
			if (flag)
			{
				break;
			}
			foreach (KeyValuePair<Type, List<string>> item2 in dictionary3)
			{
				if (item2.Value.Count == keys.Count)
				{
					stringBuilder.Append(FormatMergeModeMessage(Format.IntersectionAvailable, null, null, item2.Key.ToString()));
				}
			}
			break;
		}
		case Addressables.MergeMode.None:
		{
			Dictionary<Type, List<string>> dictionary = new Dictionary<Type, List<string>>();
			foreach (string key4 in keys)
			{
				if (!GetTypeToKeys(key4, dictionary))
				{
					stringBuilder.Append(FormatMergeModeMessage(Format.NoLocation, null, key4));
				}
			}
			foreach (KeyValuePair<Type, List<string>> item3 in dictionary)
			{
				foreach (string item4 in item3.Value)
				{
					stringBuilder.Append(FormatMergeModeMessage(Format.KeyAvailableAsType, item4, null, item3.Key.ToString()));
				}
			}
			break;
		}
		}
		return stringBuilder.ToString();
	}

	private HashSet<Type> GetTypesForKey(string keyString)
	{
		HashSet<Type> hashSet = new HashSet<Type>();
		foreach (IResourceLocator resourceLocator in m_Addressables.ResourceLocators)
		{
			if (!resourceLocator.Locate(keyString, null, out var locations))
			{
				continue;
			}
			foreach (IResourceLocation item in locations)
			{
				hashSet.Add(item.ResourceType);
			}
		}
		return hashSet;
	}

	private bool GetTypeToKeys(string key, Dictionary<Type, List<string>> typeToKeys)
	{
		HashSet<Type> typesForKey = GetTypesForKey(key);
		if (typesForKey.Count == 0)
		{
			return false;
		}
		foreach (Type item in typesForKey)
		{
			if (!typeToKeys.TryGetValue(item, out var value))
			{
				typeToKeys.Add(item, new List<string> { key });
			}
			else
			{
				value.Add(key);
			}
		}
		return true;
	}

	internal string GetKeyString()
	{
		if (Key is string)
		{
			return "Key=" + Key;
		}
		if (Key is IEnumerable enumerator)
		{
			return GetCSVString(enumerator, "Key=", "Keys=");
		}
		return Key.ToString();
	}

	internal static string GetCSVString(IEnumerable enumerator, string prefixSingle, string prefixPlural)
	{
		StringBuilder stringBuilder = new StringBuilder(prefixPlural);
		int num = 0;
		foreach (object item in enumerator)
		{
			num++;
			stringBuilder.Append((num > 1) ? $", {item}" : item);
		}
		if (num == 1 && !string.IsNullOrEmpty(prefixPlural) && !string.IsNullOrEmpty(prefixSingle))
		{
			stringBuilder.Replace(prefixPlural, prefixSingle);
		}
		return stringBuilder.ToString();
	}
}
