using System;
using System.Collections.Generic;

namespace Fusion.Internal;

[Serializable]
public abstract class UnityDictionarySurrogate<TKeyType, TKeyReaderWriter, TValueType, TValueReaderWriter> : UnitySurrogateBase where TKeyType : unmanaged where TKeyReaderWriter : unmanaged, IElementReaderWriter<TKeyType> where TValueType : unmanaged where TValueReaderWriter : unmanaged, IElementReaderWriter<TValueType>
{
	private static IElementReaderWriter<TKeyType> _keyReaderWriter = new TKeyReaderWriter();

	private static IElementReaderWriter<TValueType> _valReaderWriter = new TValueReaderWriter();

	public abstract SerializableDictionary<TKeyType, TValueType> DataProperty { get; set; }

	public unsafe override void Read(int* data, int capacity)
	{
		bool flag = false;
		SerializableDictionary<TKeyType, TValueType> dataProperty = DataProperty;
		NetworkDictionary<TKeyType, TValueType> networkDictionary = new NetworkDictionary<TKeyType, TValueType>(data, capacity, _keyReaderWriter, _valReaderWriter);
		if (networkDictionary.Count != dataProperty.Count)
		{
			flag = true;
		}
		else
		{
			foreach (KeyValuePair<TKeyType, TValueType> item in networkDictionary)
			{
				if (!dataProperty.ContainsKey(item.Key))
				{
					flag = true;
					break;
				}
				dataProperty[item.Key] = item.Value;
			}
		}
		if (flag)
		{
			dataProperty.Clear();
			foreach (KeyValuePair<TKeyType, TValueType> item2 in networkDictionary)
			{
				dataProperty.Add(item2.Key, item2.Value);
			}
		}
		dataProperty.Store();
	}

	public unsafe override void Write(int* data, int capacity)
	{
		NetworkDictionary<TKeyType, TValueType> networkDictionary = new NetworkDictionary<TKeyType, TValueType>(data, capacity, _keyReaderWriter, _valReaderWriter);
		networkDictionary.Clear();
		foreach (KeyValuePair<TKeyType, TValueType> item in DataProperty)
		{
			networkDictionary.Add(item.Key, item.Value);
		}
	}

	public override void Init(int capacity)
	{
		DataProperty = new SerializableDictionary<TKeyType, TValueType>();
	}
}
