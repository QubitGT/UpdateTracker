using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Unity.XR.CoreUtils.Bindings.Variables;

[Serializable]
public abstract class BindableVariableBase<T> : IReadOnlyBindableVariable<T>
{
	private T m_InternalValue;

	private readonly bool m_CheckEquality;

	private bool m_IsInitialized;

	private readonly Func<T, T, bool> m_EqualityMethod;

	private int m_BindingCount;

	public T Value
	{
		get
		{
			return m_InternalValue;
		}
		set
		{
			if (SetValueWithoutNotify(value))
			{
				BroadcastValue();
			}
		}
	}

	public int BindingCount => m_BindingCount;

	private event Action<T> valueUpdated;

	public bool SetValueWithoutNotify(T value)
	{
		if (m_BindingCount == 0)
		{
			m_IsInitialized = true;
			m_InternalValue = value;
			return false;
		}
		if (m_IsInitialized && m_CheckEquality && (m_EqualityMethod?.Invoke(m_InternalValue, value) ?? ValueEquals(value)))
		{
			return false;
		}
		m_IsInitialized = true;
		m_InternalValue = value;
		return true;
	}

	public IEventBinding Subscribe(Action<T> callback)
	{
		EventBinding eventBinding = default(EventBinding);
		if (callback != null)
		{
			Action<T> callbackReference = callback;
			eventBinding.BindAction = delegate
			{
				valueUpdated += callbackReference;
				IncrementReferenceCount();
			};
			eventBinding.UnbindAction = delegate
			{
				valueUpdated -= callbackReference;
				DecrementReferenceCount();
			};
			eventBinding.Bind();
		}
		return eventBinding;
	}

	public IEventBinding SubscribeAndUpdate(Action<T> callback)
	{
		callback?.Invoke(m_InternalValue);
		return Subscribe(callback);
	}

	public void Unsubscribe(Action<T> callback)
	{
		if (callback != null)
		{
			valueUpdated -= callback;
			DecrementReferenceCount();
		}
	}

	private void IncrementReferenceCount()
	{
		m_BindingCount++;
	}

	private void DecrementReferenceCount()
	{
		m_BindingCount = Mathf.Max(0, m_BindingCount - 1);
	}

	protected BindableVariableBase(T initialValue = default(T), bool checkEquality = true, Func<T, T, bool> equalityMethod = null, bool startInitialized = false)
	{
		m_IsInitialized = startInitialized;
		m_InternalValue = initialValue;
		m_CheckEquality = checkEquality;
		m_EqualityMethod = equalityMethod;
		m_BindingCount = 0;
	}

	public void BroadcastValue()
	{
		this.valueUpdated?.Invoke(m_InternalValue);
	}

	public Task<T> Task(Func<T, bool> awaitPredicate, CancellationToken token = default(CancellationToken))
	{
		if (awaitPredicate != null && awaitPredicate(m_InternalValue))
		{
			return System.Threading.Tasks.Task.FromResult(m_InternalValue);
		}
		return new BindableVariableTaskPredicate<T>(this, awaitPredicate, token).Task;
	}

	public Task<T> Task(T awaitState, CancellationToken token = default(CancellationToken))
	{
		if (ValueEquals(awaitState))
		{
			return System.Threading.Tasks.Task.FromResult(m_InternalValue);
		}
		return new BindableVariableTaskState<T>(this, awaitState, token).task;
	}

	public virtual bool ValueEquals(T other)
	{
		return m_InternalValue.Equals(other);
	}
}
