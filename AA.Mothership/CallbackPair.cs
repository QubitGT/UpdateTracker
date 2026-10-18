using System;

public class CallbackPair<T>
{
	public Action<T> successCallback;

	public Action<MothershipError, int> errorCallback;
}
