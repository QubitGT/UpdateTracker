using UnityEngine.Pool;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace UnityEngine.Localization.Operations;

internal class LocalizationGroupOperation : GroupOperation
{
	public static readonly ObjectPool<LocalizationGroupOperation> Pool = new ObjectPool<LocalizationGroupOperation>(() => new LocalizationGroupOperation(), null, null, null, collectionCheck: false);

	protected override bool InvokeWaitForCompletion()
	{
		if (base.IsDone || base.Result == null)
		{
			return true;
		}
		for (int i = 0; i < base.Result.Count; i++)
		{
			base.Result[i].WaitForCompletion();
			if (base.Result == null)
			{
				return true;
			}
		}
		m_RM?.Update(Time.unscaledDeltaTime);
		if (!base.IsDone && base.Result != null)
		{
			Execute();
		}
		m_RM?.Update(Time.unscaledDeltaTime);
		return base.IsDone;
	}

	protected override void Destroy()
	{
		Pool.Release(this);
		base.Destroy();
	}
}
