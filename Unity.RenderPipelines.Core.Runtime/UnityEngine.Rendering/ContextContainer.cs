using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Mathematics;

namespace UnityEngine.Rendering;

public class ContextContainer : IDisposable
{
	private static class TypeId<T>
	{
		public static uint value = s_TypeCount++;
	}

	private struct Item
	{
		public ContextItem storage;

		public bool isSet;
	}

	private Item[] m_Items = new Item[64];

	private List<uint> m_ActiveItemIndices = new List<uint>();

	private static uint s_TypeCount;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public T Get<T>() where T : ContextItem, new()
	{
		uint value = TypeId<T>.value;
		if (!Contains(value))
		{
			throw new InvalidOperationException("Type " + typeof(T).FullName + " has not been created yet.");
		}
		return (T)m_Items[value].storage;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public T Create<T>() where T : ContextItem, new()
	{
		uint value = TypeId<T>.value;
		if (Contains(value))
		{
			throw new InvalidOperationException("Type " + typeof(T).FullName + " has already been created.");
		}
		return CreateAndGetData<T>(value);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public T GetOrCreate<T>() where T : ContextItem, new()
	{
		uint value = TypeId<T>.value;
		if (Contains(value))
		{
			return (T)m_Items[value].storage;
		}
		return CreateAndGetData<T>(value);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool Contains<T>() where T : ContextItem, new()
	{
		uint value = TypeId<T>.value;
		return Contains(value);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private bool Contains(uint typeId)
	{
		if (typeId < m_Items.Length)
		{
			return m_Items[typeId].isSet;
		}
		return false;
	}

	private T CreateAndGetData<T>(uint typeId) where T : ContextItem, new()
	{
		if (m_Items.Length <= typeId)
		{
			Item[] array = new Item[math.max(math.ceilpow2(s_TypeCount), m_Items.Length * 2)];
			for (int i = 0; i < m_Items.Length; i++)
			{
				array[i] = m_Items[i];
			}
			m_Items = array;
		}
		m_ActiveItemIndices.Add(typeId);
		ref Item reference = ref m_Items[typeId];
		ref ContextItem storage = ref reference.storage;
		if (storage == null)
		{
			storage = new T();
		}
		reference.isSet = true;
		return (T)reference.storage;
	}

	public void Dispose()
	{
		foreach (uint activeItemIndex in m_ActiveItemIndices)
		{
			ref Item reference = ref m_Items[activeItemIndex];
			reference.storage.Reset();
			reference.isSet = false;
		}
		m_ActiveItemIndices.Clear();
	}
}
