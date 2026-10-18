using System;
using System.Collections.Generic;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.Serialization;

namespace UnityEngine.ResourceManagement.Util;

[Serializable]
public struct ObjectInitializationData
{
	internal class Serializer : BinaryStorageBuffer.ISerializationAdapter<ObjectInitializationData>, BinaryStorageBuffer.ISerializationAdapter
	{
		private struct Data
		{
			public uint id;

			public uint type;

			public uint data;
		}

		public IEnumerable<BinaryStorageBuffer.ISerializationAdapter> Dependencies => null;

		public object Deserialize(BinaryStorageBuffer.Reader reader, Type t, uint offset, out uint size)
		{
			uint size2;
			Data data = reader.ReadValue<Data>(offset, out size2);
			uint size3;
			string id = reader.ReadString(data.id, out size3);
			SerializedType serializedType = default(SerializedType);
			serializedType.Value = reader.ReadObject<Type>(data.type, out var size4);
			SerializedType objectType = serializedType;
			ObjectInitializationData objectInitializationData = default(ObjectInitializationData);
			objectInitializationData.m_Id = id;
			objectInitializationData.m_ObjectType = objectType;
			objectInitializationData.m_Data = reader.ReadString(data.data, out var size5);
			ObjectInitializationData objectInitializationData2 = objectInitializationData;
			size = size2 + size3 + size4 + size5;
			return objectInitializationData2;
		}

		public uint Serialize(BinaryStorageBuffer.Writer writer, object val)
		{
			ObjectInitializationData objectInitializationData = (ObjectInitializationData)val;
			Data data = default(Data);
			data.id = writer.WriteString(objectInitializationData.m_Id);
			data.type = writer.WriteObject(objectInitializationData.ObjectType.Value, serializeTypeData: false);
			data.data = writer.WriteString(objectInitializationData.m_Data);
			Data val2 = data;
			return writer.Write(val2);
		}
	}

	[FormerlySerializedAs("m_id")]
	[SerializeField]
	private string m_Id;

	[FormerlySerializedAs("m_objectType")]
	[SerializeField]
	private SerializedType m_ObjectType;

	[FormerlySerializedAs("m_data")]
	[SerializeField]
	private string m_Data;

	public string Id => m_Id;

	public SerializedType ObjectType => m_ObjectType;

	public string Data => m_Data;

	public override string ToString()
	{
		return $"ObjectInitializationData: id={m_Id}, type={m_ObjectType}";
	}

	public TObject CreateInstance<TObject>(string idOverride = null)
	{
		try
		{
			Type value = m_ObjectType.Value;
			if (value == null)
			{
				return default(TObject);
			}
			object obj = Activator.CreateInstance(value, nonPublic: true);
			if (obj is IInitializableObject initializableObject && !initializableObject.Initialize((idOverride == null) ? m_Id : idOverride, m_Data))
			{
				return default(TObject);
			}
			return (TObject)obj;
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
			return default(TObject);
		}
	}

	public AsyncOperationHandle GetAsyncInitHandle(ResourceManager rm, string idOverride = null)
	{
		try
		{
			Type value = m_ObjectType.Value;
			if (value == null)
			{
				return default(AsyncOperationHandle);
			}
			if (Activator.CreateInstance(value, nonPublic: true) is IInitializableObject initializableObject)
			{
				return initializableObject.InitializeAsync(rm, (idOverride == null) ? m_Id : idOverride, m_Data);
			}
			return default(AsyncOperationHandle);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
			return default(AsyncOperationHandle);
		}
	}
}
