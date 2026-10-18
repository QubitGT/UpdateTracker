using System;
using System.Threading;
using System.Threading.Tasks;

namespace Unity.XR.CoreUtils.Bindings.Variables;

internal struct BindableVariableTaskPredicate<T>
{
	private readonly TaskCompletionSource<T> m_Tcs;

	private readonly Func<T, bool> m_AwaitPredicate;

	private readonly IReadOnlyBindableVariable<T> m_BindableVariable;

	public Task<T> Task => m_Tcs.Task;

	public BindableVariableTaskPredicate(IReadOnlyBindableVariable<T> bindableVariable, Func<T, bool> awaitPredicate, CancellationToken cancellationToken = default(CancellationToken))
	{
		m_Tcs = new TaskCompletionSource<T>();
		m_AwaitPredicate = awaitPredicate;
		m_BindableVariable = bindableVariable;
		if (m_AwaitPredicate != null && m_AwaitPredicate(m_BindableVariable.Value))
		{
			m_Tcs.SetResult(m_BindableVariable.Value);
			return;
		}
		cancellationToken.Register(Cancelled);
		m_BindableVariable.Subscribe(Await);
	}

	private void Cancelled()
	{
		m_BindableVariable.Unsubscribe(Await);
		m_Tcs.SetResult(m_BindableVariable.Value);
	}

	private void Await(T state)
	{
		if (m_AwaitPredicate != null)
		{
			if (m_AwaitPredicate(state))
			{
				m_BindableVariable.Unsubscribe(Await);
				m_Tcs.SetResult(state);
			}
		}
		else
		{
			m_BindableVariable.Unsubscribe(Await);
			m_Tcs.SetResult(state);
		}
	}
}
