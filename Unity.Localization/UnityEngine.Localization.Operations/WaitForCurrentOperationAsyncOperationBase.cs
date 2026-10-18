using UnityEngine.ResourceManagement.AsyncOperations;

namespace UnityEngine.Localization.Operations;

internal abstract class WaitForCurrentOperationAsyncOperationBase<TObject> : AsyncOperationBase<TObject>
{
	private bool m_Waiting;

	protected AsyncOperationHandle CurrentOperation { get; set; }

	public AsyncOperationHandle Dependency { get; set; }

	protected override bool InvokeWaitForCompletion()
	{
		if (!base.IsRunning || m_Waiting)
		{
			return true;
		}
		try
		{
			m_Waiting = true;
			if (!HasExecuted && Dependency.IsValid())
			{
				Dependency.WaitForCompletion();
				if (Dependency.IsValid() && Dependency.Status == AsyncOperationStatus.Failed)
				{
					Complete(default(TObject), success: false, "Dependency `" + base.Handle.DebugName + "` failed to complete.");
					return true;
				}
				return false;
			}
			if (!CurrentOperation.IsValid())
			{
				Complete(default(TObject), success: false, "Expected to have a current operation to wait on. Can not complete " + ToString() + ".");
				return true;
			}
			CurrentOperation.WaitForCompletion();
			return false;
		}
		finally
		{
			m_Waiting = false;
		}
	}

	protected override void Destroy()
	{
		base.Destroy();
		Dependency = default(AsyncOperationHandle);
		CurrentOperation = default(AsyncOperationHandle);
		HasExecuted = false;
	}
}
