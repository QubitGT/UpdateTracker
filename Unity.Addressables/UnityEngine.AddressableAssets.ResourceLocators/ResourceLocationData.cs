using System;
using System.Collections.Generic;
using UnityEngine.AddressableAssets.Utility;
using UnityEngine.ResourceManagement.Util;
using UnityEngine.Serialization;

namespace UnityEngine.AddressableAssets.ResourceLocators;

[Serializable]
public class ResourceLocationData
{
	[FormerlySerializedAs("m_keys")]
	[SerializeField]
	private string[] m_Keys;

	[FormerlySerializedAs("m_internalId")]
	[SerializeField]
	private string m_InternalId;

	[FormerlySerializedAs("m_provider")]
	[SerializeField]
	private string m_Provider;

	[FormerlySerializedAs("m_dependencies")]
	[SerializeField]
	private string[] m_Dependencies;

	[SerializeField]
	private SerializedType m_ResourceType;

	[SerializeField]
	private byte[] SerializedData;

	private object _Data;

	public string[] Keys => m_Keys;

	public string InternalId => m_InternalId;

	public string Provider => m_Provider;

	public string[] Dependencies => m_Dependencies;

	public Type ResourceType => m_ResourceType.Value;

	public object Data
	{
		get
		{
			if (_Data == null)
			{
				if (SerializedData == null || SerializedData.Length == 0)
				{
					return null;
				}
				_Data = SerializationUtilities.ReadObjectFromByteArray(SerializedData, 0);
			}
			return _Data;
		}
		set
		{
			List<byte> list = new List<byte>();
			SerializationUtilities.WriteObjectToByteList(value, list);
			SerializedData = list.ToArray();
		}
	}

	public ResourceLocationData(string[] keys, string id, Type provider, Type t, string[] dependencies = null)
	{
		m_Keys = keys;
		m_InternalId = id;
		m_Provider = ((provider == null) ? "" : provider.FullName);
		m_Dependencies = ((dependencies == null) ? new string[0] : dependencies);
		m_ResourceType = new SerializedType
		{
			Value = t
		};
	}
}
