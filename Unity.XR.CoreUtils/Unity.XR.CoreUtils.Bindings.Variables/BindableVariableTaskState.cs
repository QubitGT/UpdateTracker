using System.Threading;
using System.Threading.Tasks;

namespace Unity.XR.CoreUtils.Bindings.Variables;

internal struct BindableVariableTaskState<T>
{
	private readonly TaskCompletionSource<T> m_Tcs;

	private readonly T m_AwaitState;

	private readonly IReadOnlyBindableVariable<T> m_BindableVariable;

	public Task<T> task => m_Tcs.Task;

	public BindableVariableTaskState(IReadOnlyBindableVariable<T> bindableVariable, T awaitState, CancellationToken cancellationToken = default(CancellationToken))
	{
		m_Tcs = new TaskCompletionSource<T>();
		m_AwaitState = awaitState;
		m_BindableVariable = bindableVariable;
		if (m_BindableVariable.ValueEquals(awaitState))
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
		if (m_BindableVariable.ValueEquals(m_AwaitState))
		{
			m_BindableVariable.Unsubscribe(Await);
			m_Tcs.SetResult(state);
		}
	}
}
