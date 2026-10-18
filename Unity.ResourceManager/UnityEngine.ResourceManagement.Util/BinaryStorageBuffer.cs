using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Unity.Collections.LowLevel.Unsafe;

namespace UnityEngine.ResourceManagement.Util;

internal class BinaryStorageBuffer
{
	private class BuiltinTypesSerializer : ISerializationAdapter<int>, ISerializationAdapter, ISerializationAdapter<bool>, ISerializationAdapter<long>, ISerializationAdapter<string>, ISerializationAdapter<Hash128>
	{
		private struct ObjectToStringRemap
		{
			public uint stringId;

			public char separator;
		}

		public IEnumerable<ISerializationAdapter> Dependencies => null;

		public object Deserialize(Reader reader, Type t, uint offset, out uint size)
		{
			size = 0u;
			if (offset == uint.MaxValue)
			{
				return null;
			}
			if (t == typeof(int))
			{
				return reader.ReadValue<int>(offset, out size);
			}
			if (t == typeof(bool))
			{
				return reader.ReadValue<bool>(offset, out size);
			}
			if (t == typeof(long))
			{
				return reader.ReadValue<long>(offset, out size);
			}
			if (t == typeof(Hash128))
			{
				return reader.ReadValue<Hash128>(offset, out size);
			}
			if (t == typeof(string))
			{
				ObjectToStringRemap objectToStringRemap = reader.ReadValue<ObjectToStringRemap>(offset, out size);
				uint size2;
				string result = reader.ReadString(objectToStringRemap.stringId, out size2, objectToStringRemap.separator, cacheValue: false);
				size += size2;
				return result;
			}
			return null;
		}

		private char FindBestSeparator(string str, params char[] seps)
		{
			int num = 0;
			char c2 = '\0';
			foreach (char s in seps)
			{
				int num2 = str.Count((char c) => c == s);
				if (num2 > num)
				{
					num = num2;
					c2 = s;
				}
			}
			if (num == 0)
			{
				return '\0';
			}
			string[] array = str.Split(c2);
			int num3 = 0;
			string[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				if (array2[i].Length > 4)
				{
					num3++;
				}
			}
			if (num3 <= 1)
			{
				return '\0';
			}
			return c2;
		}

		public uint Serialize(Writer writer, object val)
		{
			if (val == null)
			{
				return uint.MaxValue;
			}
			Type type = val.GetType();
			if (type == typeof(int))
			{
				return writer.Write((int)val);
			}
			if (type == typeof(bool))
			{
				return writer.Write((bool)val);
			}
			if (type == typeof(long))
			{
				return writer.Write((long)val);
			}
			if (type == typeof(Hash128))
			{
				return writer.Write((Hash128)val);
			}
			if (type == typeof(string))
			{
				string text = val as string;
				if (string.IsNullOrEmpty(text))
				{
					return uint.MaxValue;
				}
				char c = FindBestSeparator(text, '/', '\\', '.', '-', '_', ',');
				return writer.Write(new ObjectToStringRemap
				{
					stringId = writer.WriteString((string)val, c),
					separator = c
				});
			}
			return uint.MaxValue;
		}
	}

	private class TypeSerializer : ISerializationAdapter<Type>, ISerializationAdapter
	{
		private struct Data
		{
			public uint assemblyId;

			public uint classId;
		}

		public IEnumerable<ISerializationAdapter> Dependencies => null;

		public object Deserialize(Reader reader, Type type, uint offset, out uint size)
		{
			try
			{
				uint size2;
				Data data = reader.ReadValue<Data>(offset, out size2);
				uint size3;
				string assemblyString = reader.ReadString(data.assemblyId, out size3, '.');
				uint size4;
				string name = reader.ReadString(data.classId, out size4, '.');
				size = size2 + size3 + size4;
				Assembly assembly = Assembly.Load(assemblyString);
				return (assembly == null) ? null : assembly.GetType(name);
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
				size = 0u;
				return null;
			}
		}

		public uint Serialize(Writer writer, object val)
		{
			if (val == null)
			{
				return uint.MaxValue;
			}
			Type type = val as Type;
			return writer.Write(new Data
			{
				assemblyId = writer.WriteString(type.Assembly.FullName, '.'),
				classId = writer.WriteString(type.FullName, '.')
			});
		}
	}

	private struct DynamicString
	{
		public uint stringId;

		public uint nextId;
	}

	private struct ObjectTypeData
	{
		public uint typeId;

		public uint objectId;
	}

	public interface ISerializationAdapter
	{
		IEnumerable<ISerializationAdapter> Dependencies { get; }

		uint Serialize(Writer writer, object val);

		object Deserialize(Reader reader, Type t, uint offset, out uint size);
	}

	public interface ISerializationAdapter<T> : ISerializationAdapter
	{
	}

	public class Reader
	{
		private class StringCreationState
		{
			public uint id;

			public char sep;

			public int length;

			public uint size;
		}

		private byte[] m_Buffer;

		private Dictionary<Type, ISerializationAdapter> m_Adapters;

		private LRUCache<uint, object> m_Cache;

		private uint m_MinCachedObjectSize;

		private StringBuilder stringBuilder;

		private static StringCreationState stringCreationState = new StringCreationState();

		public void GetCacheStats(out int reqCount, out int reqHits)
		{
			reqCount = m_Cache.requestCount;
			reqHits = m_Cache.requestHits;
		}

		public void ResetCache(int maxCachedObjects, uint minCachedObjSize)
		{
			m_MinCachedObjectSize = minCachedObjSize;
			m_Cache = new LRUCache<uint, object>(maxCachedObjects);
		}

		private void Init(byte[] data, int maxCachedObjects, uint minCachedObjSize, params ISerializationAdapter[] adapters)
		{
			m_Buffer = data;
			m_MinCachedObjectSize = minCachedObjSize;
			stringBuilder = new StringBuilder(1024);
			m_Cache = new LRUCache<uint, object>(maxCachedObjects);
			m_Adapters = new Dictionary<Type, ISerializationAdapter>();
			foreach (ISerializationAdapter adapter in adapters)
			{
				BinaryStorageBuffer.AddSerializationAdapter(m_Adapters, adapter, forceOverride: false);
			}
			BinaryStorageBuffer.AddSerializationAdapter(m_Adapters, (ISerializationAdapter)new TypeSerializer(), forceOverride: false);
			BinaryStorageBuffer.AddSerializationAdapter(m_Adapters, (ISerializationAdapter)new BuiltinTypesSerializer(), forceOverride: false);
		}

		public void AddSerializationAdapter(ISerializationAdapter a)
		{
			BinaryStorageBuffer.AddSerializationAdapter(m_Adapters, a, forceOverride: false);
		}

		public Reader(byte[] data, int maxCachedObjects = 1024, uint minCachedObjSize = 64u, params ISerializationAdapter[] adapters)
		{
			Init(data, maxCachedObjects, minCachedObjSize, adapters);
		}

		internal byte[] GetBuffer()
		{
			return m_Buffer;
		}

		public Reader(Stream inputStream, uint bufferSize, int maxCachedObjects, uint minCachedObjSize, params ISerializationAdapter[] adapters)
		{
			byte[] array = new byte[(bufferSize == 0) ? inputStream.Length : bufferSize];
			inputStream.Read(array, 0, array.Length);
			Init(array, maxCachedObjects, minCachedObjSize, adapters);
		}

		private bool TryGetCachedValue(Type t, uint offset, out object val)
		{
			if (m_Cache.TryGet(t, offset, out var val2))
			{
				val = val2;
				return true;
			}
			val = null;
			return false;
		}

		private bool TryGetCachedValue<T>(uint offset, out T val)
		{
			if (m_Cache.TryGet(typeof(T), offset, out var val2))
			{
				val = (T)val2;
				return true;
			}
			val = default(T);
			return false;
		}

		public unsafe T[] ReadValueArray<T>(uint id, out uint readSize, bool cacheValue = true) where T : unmanaged
		{
			readSize = 0u;
			if (id == uint.MaxValue)
			{
				return null;
			}
			if (id - 4 >= m_Buffer.Length)
			{
				throw new Exception($"Data offset {id} is out of bounds of buffer with length of {m_Buffer.Length}.");
			}
			fixed (byte* ptr = &m_Buffer[id - 4])
			{
				if (TryGetCachedValue<T[]>(id, out var val))
				{
					return val;
				}
				uint num = 0u;
				UnsafeUtility.MemCpy(&num, ptr, 4L);
				if (id + num > m_Buffer.Length)
				{
					throw new Exception($"Data size {num} is out of bounds of buffer with length of {m_Buffer.Length}.");
				}
				T[] array = new T[num / sizeof(T)];
				fixed (T* destination = array)
				{
					UnsafeUtility.MemCpy(destination, ptr + 4, num);
				}
				if (cacheValue && num >= m_MinCachedObjectSize)
				{
					m_Cache.TryAdd(id, array);
				}
				readSize = num;
				return array;
			}
		}

		public unsafe uint ProcessObjectArray<T, C>(uint id, out uint size, C context, Action<T, C, int, int> procFunc, bool cacheValues = true)
		{
			if (id == uint.MaxValue)
			{
				size = 0u;
				return 0u;
			}
			fixed (byte* ptr = &m_Buffer[id - 4])
			{
				uint num = 0u;
				uint num2 = *(uint*)ptr / 4;
				for (int i = 0; i < num2; i++)
				{
					procFunc(ReadObject<T>(*(uint*)(ptr + 4 * (1 + i)), out var size2, cacheValues), context, i, (int)num2);
					num += size2;
				}
				size = num;
				return num2;
			}
		}

		public unsafe uint ReadObjectArray<T>(ref List<T> results, uint id, out uint size, bool cacheValues = true)
		{
			if (id == uint.MaxValue)
			{
				size = 0u;
				return 0u;
			}
			fixed (byte* ptr = &m_Buffer[id - 4])
			{
				uint num = 0u;
				uint num2 = *(uint*)ptr / 4;
				for (int i = 0; i < num2; i++)
				{
					results.Add(ReadObject<T>(*(uint*)(ptr + 4 * (1 + i)), out var size2, cacheValues));
					num += size2;
				}
				size = num;
				return num2;
			}
		}

		public unsafe object[] ReadObjectArray(uint id, out uint size, bool cacheValues = true, bool cacheFullArray = false)
		{
			if (id == uint.MaxValue)
			{
				size = 0u;
				return null;
			}
			if (TryGetCachedValue<object[]>(id, out var val))
			{
				size = 0u;
				return val;
			}
			fixed (byte* ptr = &m_Buffer[id - 4])
			{
				uint num = 0u;
				uint num2 = *(uint*)ptr / 4;
				object[] array = new object[num2];
				for (int i = 0; i < num2; i++)
				{
					array[i] = ReadObject(*(uint*)(ptr + 4 * (1 + i)), out var size2, cacheValues);
					num += size2;
				}
				size = num;
				if (cacheFullArray && size >= m_MinCachedObjectSize)
				{
					m_Cache.TryAdd(id, array);
				}
				return array;
			}
		}

		public unsafe object[] ReadObjectArray(Type t, uint id, out uint size, bool cacheValues = true, bool cacheFullArray = false)
		{
			if (id == uint.MaxValue)
			{
				size = 0u;
				return null;
			}
			if (TryGetCachedValue<object[]>(id, out var val))
			{
				size = 0u;
				return val;
			}
			fixed (byte* ptr = &m_Buffer[id - 4])
			{
				uint num = 0u;
				uint num2 = *(uint*)ptr / 4;
				object[] array = new object[num2];
				for (int i = 0; i < num2; i++)
				{
					array[i] = ReadObject(t, *(uint*)(ptr + 4 * (1 + i)), out var size2, cacheValues);
					num += size2;
				}
				size = num;
				if (cacheFullArray && size >= m_MinCachedObjectSize)
				{
					m_Cache.TryAdd(id, array);
				}
				return array;
			}
		}

		public unsafe T[] ReadObjectArray<T>(uint id, out uint size, bool cacheValues = true, bool cacheFullArray = false)
		{
			if (id == uint.MaxValue)
			{
				size = 0u;
				return null;
			}
			if (TryGetCachedValue<T[]>(id, out var val))
			{
				size = 0u;
				return val;
			}
			fixed (byte* ptr = &m_Buffer[id - 4])
			{
				uint num = 0u;
				uint num2 = *(uint*)ptr / 4;
				T[] array = new T[num2];
				for (int i = 0; i < num2; i++)
				{
					array[i] = ReadObject<T>(*(uint*)(ptr + 4 * (1 + i)), out var size2, cacheValues);
					num += size2;
				}
				size = num;
				if (cacheFullArray && size >= m_MinCachedObjectSize)
				{
					m_Cache.TryAdd(id, array);
				}
				return array;
			}
		}

		public object ReadObject(uint id, out uint size, bool cacheValue = true)
		{
			if (id == uint.MaxValue)
			{
				size = 0u;
				return null;
			}
			uint size2;
			ObjectTypeData objectTypeData = ReadValue<ObjectTypeData>(id, out size2);
			uint size3;
			Type t = ReadObject<Type>(objectTypeData.typeId, out size3);
			uint size4;
			object result = ReadObject(t, objectTypeData.objectId, out size4, cacheValue);
			size = size2 + size3 + size4;
			return result;
		}

		public T ReadObject<T>(uint id, out uint size, bool cacheValue = true)
		{
			size = 0u;
			if (id == uint.MaxValue)
			{
				return default(T);
			}
			if (TryGetCachedValue<T>(id, out var val))
			{
				return val;
			}
			if (!GetSerializationAdapter(m_Adapters, typeof(T), out var adapter))
			{
				return default(T);
			}
			T val2 = default(T);
			try
			{
				val2 = (T)adapter.Deserialize(this, typeof(T), id, out size);
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
				return default(T);
			}
			if (cacheValue && val2 != null && size >= m_MinCachedObjectSize)
			{
				m_Cache.TryAdd(id, val2);
			}
			return val2;
		}

		public object ReadObject(Type t, uint id, out uint size, bool cacheValue = true)
		{
			size = 0u;
			if (id == uint.MaxValue)
			{
				return null;
			}
			if (TryGetCachedValue(t, id, out var val))
			{
				return val;
			}
			if (!GetSerializationAdapter(m_Adapters, t, out var adapter))
			{
				return null;
			}
			object obj = null;
			try
			{
				obj = adapter.Deserialize(this, t, id, out size);
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
				return null;
			}
			if (cacheValue && obj != null && size >= m_MinCachedObjectSize)
			{
				m_Cache.TryAdd(id, obj);
			}
			return obj;
		}

		public unsafe T ReadValue<T>(uint id, out uint size) where T : unmanaged
		{
			size = (uint)sizeof(T);
			if (id == uint.MaxValue)
			{
				return default(T);
			}
			if (id >= m_Buffer.Length)
			{
				throw new Exception($"Data offset {id} is out of bounds of buffer with length of {m_Buffer.Length}.");
			}
			fixed (byte* ptr = m_Buffer)
			{
				T result = default(T);
				UnsafeUtility.MemCpy(&result, ptr + id, size);
				return result;
			}
		}

		public string ReadString(uint id, out uint size, char sep = '\0', bool cacheValue = true)
		{
			if (id == uint.MaxValue)
			{
				size = 0u;
				return null;
			}
			if (TryGetCachedValue<string>(id, out var val))
			{
				size = 0u;
				return val;
			}
			if (sep == '\0')
			{
				return ReadAutoEncodedString(id, out size, cacheValue);
			}
			return ReadDynamicString(id, out size, sep, cacheValue);
		}

		private unsafe string ReadStringInternal(uint offset, out uint size, Encoding enc, bool cacheValue = true)
		{
			if (offset - 4 >= m_Buffer.Length)
			{
				throw new Exception($"Data offset {offset} is out of bounds of buffer with length of {m_Buffer.Length}.");
			}
			if (TryGetCachedValue<string>(offset, out var val))
			{
				size = 0u;
				return val;
			}
			fixed (byte* ptr = m_Buffer)
			{
				uint num = *(uint*)(ptr + (offset - 4));
				if (offset + num > m_Buffer.Length)
				{
					throw new Exception($"Data offset {offset}, len {num} is out of bounds of buffer with length of {m_Buffer.Length}.");
				}
				string @string = enc.GetString(ptr + offset, (int)num);
				size = num;
				if (cacheValue && size >= m_MinCachedObjectSize)
				{
					m_Cache.TryAdd(offset, @string);
				}
				return @string;
			}
		}

		private string ReadAutoEncodedString(uint id, out uint size, bool cacheValue)
		{
			if ((id & 0x80000000u) == 2147483648u)
			{
				return ReadStringInternal(id & 0x3FFFFFFFu, out size, Encoding.Unicode, cacheValue);
			}
			return ReadStringInternal(id, out size, Encoding.ASCII, cacheValue);
		}

		public int ComputeStringLength(uint id, char sep = '\0')
		{
			if (id == uint.MaxValue)
			{
				return 0;
			}
			if (sep == '\0')
			{
				return GetAutoEncodedStringLength(id);
			}
			return GetDynamicStringLength(id, sep);
		}

		private int GetDynamicStringLength(uint id, char sep)
		{
			if ((id & 0x40000000) == 1073741824)
			{
				int num = 0;
				uint num2 = id;
				while (num2 != uint.MaxValue)
				{
					uint size;
					DynamicString dynamicString = ReadValue<DynamicString>(num2 & 0x3FFFFFFFu, out size);
					num += GetAutoEncodedStringLength(dynamicString.stringId);
					num2 = dynamicString.nextId;
					if (num2 != uint.MaxValue)
					{
						num++;
					}
				}
				return num;
			}
			return GetAutoEncodedStringLength(id);
		}

		private int GetAutoEncodedStringLength(uint id)
		{
			if ((id & 0x80000000u) == 2147483648u)
			{
				return GetStringLengthInternal(id & 0x3FFFFFFFu, Encoding.Unicode);
			}
			return GetStringLengthInternal(id, Encoding.ASCII);
		}

		private unsafe int GetStringLengthInternal(uint offset, Encoding enc)
		{
			if (offset - 4 >= m_Buffer.Length)
			{
				throw new Exception($"Data offset {offset} is out of bounds of buffer with length of {m_Buffer.Length}.");
			}
			fixed (byte* ptr = m_Buffer)
			{
				uint num = *(uint*)(ptr + (offset - 4));
				if (offset + num > m_Buffer.Length)
				{
					throw new Exception($"Data offset {offset}, len {num} is out of bounds of buffer with length of {m_Buffer.Length}.");
				}
				return enc.GetCharCount(ptr + offset, (int)num);
			}
		}

		private string ReadDynamicString(uint id, out uint size, char sep, bool cacheValue)
		{
			if ((id & 0x40000000) == 1073741824)
			{
				size = 0u;
				if (TryGetCachedValue<string>(id, out var val))
				{
					return val;
				}
				int dynamicStringLength = GetDynamicStringLength(id, sep);
				stringCreationState.id = id;
				stringCreationState.sep = sep;
				stringCreationState.length = dynamicStringLength;
				stringCreationState.size = 0u;
				val = string.Create(dynamicStringLength, stringCreationState, delegate(Span<char> chars, StringCreationState state)
				{
					int num = state.length;
					uint num2 = state.id;
					while (num2 != uint.MaxValue)
					{
						uint size2;
						DynamicString dynamicString = ReadValue<DynamicString>(num2 & 0x3FFFFFFFu, out size2);
						uint size3;
						string text = ReadAutoEncodedString(dynamicString.stringId, out size3, cacheValue: true);
						text.AsSpan().CopyTo(chars.Slice(num - text.Length, text.Length));
						state.size += size2 + size3;
						num -= text.Length;
						num2 = dynamicString.nextId;
						if (num2 != uint.MaxValue)
						{
							chars[--num] = state.sep;
						}
					}
				});
				size = stringCreationState.size;
				if (cacheValue && size >= m_MinCachedObjectSize)
				{
					m_Cache.TryAdd(id, val);
				}
				return val;
			}
			return ReadAutoEncodedString(id, out size, cacheValue);
		}
	}

	public class Writer
	{
		private class Chunk
		{
			public uint position;

			public byte[] data;
		}

		private struct StringParts
		{
			public string str;

			public uint dataSize;

			public bool isUnicode;
		}

		private uint totalBytes;

		private uint defaulChunkSize;

		private List<Chunk> chunks;

		private Dictionary<Hash128, uint> existingValues;

		private Dictionary<Type, ISerializationAdapter> serializationAdapters;

		public uint Length => totalBytes;

		public Writer(int chunkSize = 1048576, params ISerializationAdapter[] adapters)
		{
			defaulChunkSize = ((chunkSize > 0) ? ((uint)chunkSize) : 1048576u);
			existingValues = new Dictionary<Hash128, uint>();
			chunks = new List<Chunk>(10);
			chunks.Add(new Chunk
			{
				position = 0u
			});
			serializationAdapters = new Dictionary<Type, ISerializationAdapter>();
			AddSerializationAdapter(serializationAdapters, new TypeSerializer());
			AddSerializationAdapter(serializationAdapters, new BuiltinTypesSerializer());
			foreach (ISerializationAdapter adapter in adapters)
			{
				AddSerializationAdapter(serializationAdapters, adapter, forceOverride: true);
			}
		}

		private Chunk FindChunkWithSpace(uint length)
		{
			Chunk chunk = chunks[chunks.Count - 1];
			if (chunk.data == null)
			{
				chunk.data = new byte[(length > defaulChunkSize) ? length : defaulChunkSize];
			}
			if (length > chunk.data.Length - chunk.position)
			{
				chunk = new Chunk
				{
					position = 0u,
					data = new byte[(length > defaulChunkSize) ? length : defaulChunkSize]
				};
				chunks.Add(chunk);
			}
			return chunk;
		}

		private unsafe uint WriteInternal(void* pData, uint dataSize, bool prefixSize)
		{
			Hash128 key = default(Hash128);
			ComputeHash(pData, dataSize, &key);
			if (existingValues.TryGetValue(key, out var value))
			{
				return value;
			}
			uint num = (prefixSize ? (dataSize + 4) : dataSize);
			Chunk chunk = FindChunkWithSpace(num);
			fixed (byte* ptr = &chunk.data[chunk.position])
			{
				uint num2 = totalBytes;
				if (prefixSize)
				{
					UnsafeUtility.MemCpy(ptr, &dataSize, 4L);
					if (dataSize != 0)
					{
						UnsafeUtility.MemCpy(ptr + 4, pData, dataSize);
					}
					num2 += 4;
				}
				else
				{
					if (dataSize == 0)
					{
						return uint.MaxValue;
					}
					UnsafeUtility.MemCpy(ptr, pData, dataSize);
				}
				totalBytes += num;
				chunk.position += num;
				existingValues[key] = num2;
				return num2;
			}
		}

		private uint ReserveInternal(uint dataSize, bool prefixSize)
		{
			uint num = (prefixSize ? (dataSize + 4) : dataSize);
			Chunk chunk = FindChunkWithSpace(num);
			totalBytes += num;
			chunk.position += num;
			return totalBytes - dataSize;
		}

		private unsafe void WriteInternal(uint id, void* pData, uint dataSize, bool prefixSize)
		{
			Hash128 key = default(Hash128);
			ComputeHash(pData, dataSize, &key);
			existingValues[key] = id;
			uint num = id;
			foreach (Chunk chunk in chunks)
			{
				if (num < chunk.position)
				{
					fixed (byte* ptr = chunk.data)
					{
						if (prefixSize)
						{
							UnsafeUtility.MemCpy(ptr + (num - 4), &dataSize, 4L);
						}
						UnsafeUtility.MemCpy(ptr + num, pData, dataSize);
						break;
					}
				}
				num -= chunk.position;
			}
		}

		public unsafe uint Reserve<T>() where T : unmanaged
		{
			return ReserveInternal((uint)sizeof(T), prefixSize: false);
		}

		public unsafe uint Write<T>(in T val) where T : unmanaged
		{
			fixed (T* pData = &val)
			{
				return WriteInternal(pData, (uint)sizeof(T), prefixSize: false);
			}
		}

		public unsafe uint Write<T>(T val) where T : unmanaged
		{
			return WriteInternal(&val, (uint)sizeof(T), prefixSize: false);
		}

		public unsafe uint Write<T>(uint offset, in T val) where T : unmanaged
		{
			fixed (T* pData = &val)
			{
				WriteInternal(offset, pData, (uint)sizeof(T), prefixSize: false);
			}
			return offset;
		}

		public unsafe uint Write<T>(uint offset, T val) where T : unmanaged
		{
			WriteInternal(offset, &val, (uint)sizeof(T), prefixSize: false);
			return offset;
		}

		public unsafe uint Reserve<T>(uint count) where T : unmanaged
		{
			return ReserveInternal((uint)sizeof(T) * count, prefixSize: true);
		}

		public unsafe uint Write<T>(T[] values, bool hashElements = true) where T : unmanaged
		{
			fixed (T* ptr = values)
			{
				uint num = (uint)(values.Length * sizeof(T));
				Hash128 key = default(Hash128);
				ComputeHash(ptr, num, &key);
				if (existingValues.TryGetValue(key, out var value))
				{
					return value;
				}
				Chunk chunk = FindChunkWithSpace(num + 4);
				fixed (byte* ptr2 = &chunk.data[chunk.position])
				{
					uint num2 = totalBytes + 4;
					UnsafeUtility.MemCpy(ptr2, &num, 4L);
					UnsafeUtility.MemCpy(ptr2 + 4, ptr, num);
					uint num3 = num + 4;
					totalBytes += num3;
					chunk.position += num3;
					existingValues[key] = num2;
					if (hashElements && sizeof(T) > 4)
					{
						for (int i = 0; i < values.Length; i++)
						{
							key = default(Hash128);
							ComputeHash(ptr + i, (ulong)sizeof(T), &key);
							existingValues[key] = num2 + (uint)(i * sizeof(T));
						}
					}
					return num2;
				}
			}
		}

		public unsafe uint Write<T>(uint offset, T[] values, bool hashElements = true) where T : unmanaged
		{
			uint num = (uint)(values.Length * sizeof(T));
			uint num2 = offset;
			fixed (T* source = values)
			{
				Hash128 key = default(Hash128);
				foreach (Chunk chunk in chunks)
				{
					if (num2 < chunk.position)
					{
						fixed (byte* ptr = chunk.data)
						{
							UnsafeUtility.MemCpy(ptr + (num2 - 4), &num, 4L);
							UnsafeUtility.MemCpy(ptr + num2, source, num);
							if (hashElements && sizeof(T) > 4)
							{
								for (int i = 0; i < values.Length; i++)
								{
									T val = values[i];
									ComputeHash(&val, (ulong)sizeof(T), &key);
									existingValues[key] = offset + (uint)(i * sizeof(T));
								}
							}
							return offset;
						}
					}
					num2 -= chunk.position;
				}
			}
			return uint.MaxValue;
		}

		public uint WriteObjects<T>(IEnumerable<T> objs, bool serizalizeTypeData)
		{
			if (objs == null)
			{
				return uint.MaxValue;
			}
			uint[] array = new uint[objs.Count()];
			int num = 0;
			foreach (T obj in objs)
			{
				array[num++] = WriteObject(obj, serizalizeTypeData);
			}
			return Write(array, hashElements: true);
		}

		public uint WriteObject(object obj, bool serializeTypeData)
		{
			if (obj == null)
			{
				return uint.MaxValue;
			}
			Type type = obj.GetType();
			if (!GetSerializationAdapter(serializationAdapters, type, out var adapter))
			{
				return uint.MaxValue;
			}
			uint num = adapter.Serialize(this, obj);
			if (serializeTypeData)
			{
				num = Write(new ObjectTypeData
				{
					typeId = WriteObject(type, serializeTypeData: false),
					objectId = num
				});
			}
			return num;
		}

		public uint WriteString(string str, char sep = '\0')
		{
			if (str == null)
			{
				return uint.MaxValue;
			}
			if (sep != 0)
			{
				return WriteDynamicString(str, sep);
			}
			return WriteAutoEncodedString(str);
		}

		private unsafe uint WriteStringInternal(string val, Encoding enc)
		{
			if (val == null)
			{
				return uint.MaxValue;
			}
			byte[] bytes = enc.GetBytes(val);
			fixed (byte* pData = bytes)
			{
				return WriteInternal(pData, (uint)bytes.Length, prefixSize: true);
			}
		}

		public unsafe byte[] SerializeToByteArray()
		{
			byte[] array = new byte[totalBytes];
			fixed (byte* ptr = array)
			{
				uint num = 0u;
				foreach (Chunk chunk in chunks)
				{
					fixed (byte* source = chunk.data)
					{
						UnsafeUtility.MemCpy(ptr + num, source, chunk.position);
					}
					num += chunk.position;
				}
			}
			return array;
		}

		public uint SerializeToStream(Stream str)
		{
			foreach (Chunk chunk in chunks)
			{
				str.Write(chunk.data, 0, (int)chunk.position);
			}
			return totalBytes;
		}

		private static bool IsUnicode(string str)
		{
			for (int i = 0; i < str.Length; i++)
			{
				if (str[i] > 'ÿ')
				{
					return true;
				}
			}
			return false;
		}

		private uint WriteAutoEncodedString(string str)
		{
			if (str == null)
			{
				return uint.MaxValue;
			}
			if (IsUnicode(str))
			{
				return WriteUnicodeString(str);
			}
			return WriteStringInternal(str, Encoding.ASCII);
		}

		private uint WriteUnicodeString(string str)
		{
			uint num = WriteStringInternal(str, Encoding.Unicode);
			return 0x80000000u | num;
		}

		private static uint ComputeStringSize(string str, out bool isUnicode)
		{
			if (isUnicode = IsUnicode(str))
			{
				return (uint)Encoding.Unicode.GetByteCount(str);
			}
			return (uint)Encoding.ASCII.GetByteCount(str);
		}

		private unsafe uint WriteDynamicString(string str, char sep)
		{
			if (str == null)
			{
				return uint.MaxValue;
			}
			string[] array = str.Split(sep);
			uint num = (uint)sizeof(DynamicString);
			StringParts[] array2 = new StringParts[array.Length];
			for (int i = 0; i < array2.Length; i++)
			{
				bool isUnicode;
				uint dataSize = ComputeStringSize(array[i], out isUnicode);
				array2[i] = new StringParts
				{
					str = array[i],
					dataSize = dataSize,
					isUnicode = isUnicode
				};
			}
			if (array2.Length < 2 || (array2.Length == 2 && array2[0].dataSize + array2[1].dataSize < num))
			{
				return WriteAutoEncodedString(str);
			}
			return 0x40000000u | RecurseDynamicStringParts(array2, array2.Length - 1, sep, num);
		}

		private uint RecurseDynamicStringParts(StringParts[] parts, int index, char sep, uint minSize)
		{
			while (index > 0)
			{
				uint dataSize = parts[index].dataSize;
				if (dataSize >= minSize)
				{
					break;
				}
				parts[index - 1].str = $"{parts[index - 1].str}{sep}{parts[index].str}";
				parts[index - 1].dataSize += dataSize + 1;
				parts[index - 1].isUnicode |= parts[index].isUnicode;
				index--;
			}
			uint stringId = (parts[index].isUnicode ? WriteUnicodeString(parts[index].str) : WriteStringInternal(parts[index].str, Encoding.ASCII));
			uint nextId = ((index > 0) ? RecurseDynamicStringParts(parts, index - 1, sep, minSize) : uint.MaxValue);
			return Write(new DynamicString
			{
				stringId = stringId,
				nextId = nextId
			});
		}
	}

	private const uint kUnicodeStringFlag = 2147483648u;

	private const uint kDynamicStringFlag = 1073741824u;

	private const uint kClearFlagsMask = 1073741823u;

	private unsafe static void ComputeHash(void* pData, ulong size, Hash128* hash)
	{
		if (pData == null || size == 0L)
		{
			*hash = default(Hash128);
		}
		else
		{
			HashUnsafeUtilities.ComputeHash128(pData, size, hash);
		}
	}

	private static void AddSerializationAdapter(Dictionary<Type, ISerializationAdapter> serializationAdapters, ISerializationAdapter adapter, bool forceOverride = false)
	{
		bool flag = false;
		Type[] interfaces = adapter.GetType().GetInterfaces();
		foreach (Type type in interfaces)
		{
			if (!type.IsGenericType || !typeof(ISerializationAdapter).IsAssignableFrom(type))
			{
				continue;
			}
			Type type2 = type.GenericTypeArguments[0];
			if (serializationAdapters.ContainsKey(type2))
			{
				if (forceOverride)
				{
					ISerializationAdapter arg = serializationAdapters[type2];
					serializationAdapters.Remove(type2);
					serializationAdapters[type2] = adapter;
					flag = true;
					Debug.Log($"Replacing adapter for type {type2}: {arg} -> {adapter}");
				}
				else
				{
					Debug.Log($"Failed to register adapter for type {type2}: {adapter}, {serializationAdapters[type2]} is already registered.");
				}
			}
			else
			{
				serializationAdapters[type2] = adapter;
				flag = true;
			}
		}
		if (!flag)
		{
			return;
		}
		IEnumerable<ISerializationAdapter> dependencies = adapter.Dependencies;
		if (dependencies == null)
		{
			return;
		}
		foreach (ISerializationAdapter item in dependencies)
		{
			AddSerializationAdapter(serializationAdapters, item);
		}
	}

	private static bool GetSerializationAdapter(Dictionary<Type, ISerializationAdapter> serializationAdapters, Type t, out ISerializationAdapter adapter)
	{
		if (!serializationAdapters.TryGetValue(t, out adapter))
		{
			foreach (KeyValuePair<Type, ISerializationAdapter> serializationAdapter in serializationAdapters)
			{
				if (serializationAdapter.Key.IsAssignableFrom(t))
				{
					return (adapter = serializationAdapter.Value) != null;
				}
			}
			Debug.LogError($"Unable to find serialization adapter for type {t}.");
		}
		return adapter != null;
	}
}
