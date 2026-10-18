using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.Initialization;
using UnityEngine.ResourceManagement.Util;

namespace UnityEngine.ResourceManagement.AsyncOperations;

internal class InitalizationObjectsOperation : AsyncOperationBase<bool>
{
	private AsyncOperationHandle<ResourceManagerRuntimeData> m_RtdOp;

	private AddressablesImpl m_Addressables;

	private AsyncOperationHandle<IList<AsyncOperationHandle>> m_DepOp;

	protected override string DebugName => "InitializationObjectsOperation";

	public void Init(AsyncOperationHandle<ResourceManagerRuntimeData> rtdOp, AddressablesImpl addressables)
	{
		m_RtdOp = rtdOp;
		m_Addressables = addressables;
		m_Addressables.ResourceManager.RegisterForCallbacks();
	}

	internal bool LogRuntimeWarnings(string pathToBuildLogs)
	{
		if (!File.Exists(pathToBuildLogs))
		{
			return false;
		}
		PackedPlayModeBuildLogs packedPlayModeBuildLogs = JsonUtility.FromJson<PackedPlayModeBuildLogs>(File.ReadAllText(pathToBuildLogs));
		bool result = false;
		foreach (PackedPlayModeBuildLogs.RuntimeBuildLog runtimeBuildLog in packedPlayModeBuildLogs.RuntimeBuildLogs)
		{
			result = true;
			switch (runtimeBuildLog.Type)
			{
			case LogType.Warning:
				Addressables.LogWarning(runtimeBuildLog.Message);
				break;
			case LogType.Error:
				Addressables.LogError(runtimeBuildLog.Message);
				break;
			}
		}
		return result;
	}

	protected override bool InvokeWaitForCompletion()
	{
		if (base.IsDone)
		{
			return true;
		}
		if (m_RtdOp.IsValid() && !m_RtdOp.IsDone)
		{
			m_RtdOp.WaitForCompletion();
		}
		m_RM?.Update(Time.unscaledDeltaTime);
		if (!HasExecuted)
		{
			InvokeExecute();
		}
		if (m_DepOp.IsValid() && !m_DepOp.IsDone)
		{
			m_DepOp.WaitForCompletion();
		}
		m_RM?.Update(Time.unscaledDeltaTime);
		return base.IsDone;
	}

	protected override void Execute()
	{
		ResourceManagerRuntimeData result = m_RtdOp.Result;
		if (result == null)
		{
			Addressables.LogError("RuntimeData is null.  Please ensure you have built the correct Player Content.");
			Complete(result: true, success: true, "");
			return;
		}
		List<AsyncOperationHandle> list = new List<AsyncOperationHandle>();
		foreach (ObjectInitializationData initializationObject in result.InitializationObjects)
		{
			if (!(initializationObject.ObjectType.Value == null))
			{
				try
				{
					AsyncOperationHandle asyncInitHandle = initializationObject.GetAsyncInitHandle(m_Addressables.ResourceManager);
					list.Add(asyncInitHandle);
				}
				catch (Exception ex)
				{
					Addressables.LogErrorFormat("Exception thrown during initialization of object {0}: {1}", initializationObject, ex.ToString());
				}
			}
		}
		if (list.Count > 0)
		{
			m_DepOp = m_Addressables.ResourceManager.CreateGenericGroupOperation(list, releasedCachedOpOnComplete: true);
			m_DepOp.Completed += delegate(AsyncOperationHandle<IList<AsyncOperationHandle>> obj)
			{
				bool flag = obj.Status == AsyncOperationStatus.Succeeded;
				Complete(result: true, flag, flag ? "" : $"{obj.DebugName}, status={obj.Status}, result={obj.Result} failed initialization.");
				m_DepOp.Release();
			};
		}
		else
		{
			Complete(result: true, success: true, "");
		}
	}
}
