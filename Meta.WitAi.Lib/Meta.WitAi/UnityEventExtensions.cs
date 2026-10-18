using UnityEngine.Events;

namespace Meta.WitAi;

public static class UnityEventExtensions
{
	public static void SetListener(this UnityEvent baseEvent, UnityAction call, bool add)
	{
		if (baseEvent != null && call != null)
		{
			if (add)
			{
				baseEvent.AddListener(call);
			}
			else
			{
				baseEvent.RemoveListener(call);
			}
		}
	}

	public static void SetListener<T>(this UnityEvent<T> baseEvent, UnityAction<T> call, bool add)
	{
		if (baseEvent != null && call != null)
		{
			if (add)
			{
				baseEvent.AddListener(call);
			}
			else
			{
				baseEvent.RemoveListener(call);
			}
		}
	}

	public static void SetListener<T0, T1>(this UnityEvent<T0, T1> baseEvent, UnityAction<T0, T1> call, bool add)
	{
		if (baseEvent != null && call != null)
		{
			if (add)
			{
				baseEvent.AddListener(call);
			}
			else
			{
				baseEvent.RemoveListener(call);
			}
		}
	}

	public static void SetListener<T0, T1, T2>(this UnityEvent<T0, T1, T2> baseEvent, UnityAction<T0, T1, T2> call, bool add)
	{
		if (baseEvent != null && call != null)
		{
			if (add)
			{
				baseEvent.AddListener(call);
			}
			else
			{
				baseEvent.RemoveListener(call);
			}
		}
	}

	public static void SetListener<T0, T1, T2, T3>(this UnityEvent<T0, T1, T2, T3> baseEvent, UnityAction<T0, T1, T2, T3> call, bool add)
	{
		if (baseEvent != null && call != null)
		{
			if (add)
			{
				baseEvent.AddListener(call);
			}
			else
			{
				baseEvent.RemoveListener(call);
			}
		}
	}
}
