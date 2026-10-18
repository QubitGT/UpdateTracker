using System.Collections;
using System.Collections.Generic;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.Util;

namespace UnityEngine.ResourceManagement.AsyncOperations;

internal class GetDownloadSizeOperation : AsyncOperationBase<long>
{
	private IEnumerable<IResourceLocation> m_Locations;

	private Coroutine m_AsyncCalculation;

	public void Init(IEnumerable<IResourceLocation> locations, ResourceManager resourceManager)
	{
		m_Locations = locations;
		m_RM = resourceManager;
	}

	private IEnumerator Calculate()
	{
		long size = 0L;
		foreach (IResourceLocation location in m_Locations)
		{
			if (location.Data is ILocationSizeData locationSizeData)
			{
				size += locationSizeData.ComputeSize(location, m_RM);
				yield return null;
			}
		}
		Complete(size, success: true, "");
	}

	private void CalculateSync()
	{
		long num = 0L;
		foreach (IResourceLocation location in m_Locations)
		{
			if (location.Data is ILocationSizeData locationSizeData)
			{
				num += locationSizeData.ComputeSize(location, m_RM);
			}
		}
		Complete(num, success: true, "");
	}

	protected override void Execute()
	{
		m_AsyncCalculation = ComponentSingleton<MonoBehaviourCallbackHooks>.Instance.StartCoroutine(Calculate());
	}

	protected override bool InvokeWaitForCompletion()
	{
		ComponentSingleton<MonoBehaviourCallbackHooks>.Instance.StopCoroutine(m_AsyncCalculation);
		CalculateSync();
		return true;
	}
}
