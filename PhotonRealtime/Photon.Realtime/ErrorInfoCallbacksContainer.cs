using System.Collections.Generic;

namespace Photon.Realtime;

internal class ErrorInfoCallbacksContainer : List<IErrorInfoCallback>, IErrorInfoCallback
{
	private LoadBalancingClient client;

	public ErrorInfoCallbacksContainer(LoadBalancingClient client)
	{
		this.client = client;
	}

	public void OnErrorInfo(ErrorInfo errorInfo)
	{
		client.UpdateCallbackTargets();
		using Enumerator enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			enumerator.Current.OnErrorInfo(errorInfo);
		}
	}
}
