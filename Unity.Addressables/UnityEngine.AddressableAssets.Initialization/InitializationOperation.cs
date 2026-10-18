using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.AddressableAssets.ResourceProviders;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.ResourceManagement.Util;

namespace UnityEngine.AddressableAssets.Initialization;

internal class InitializationOperation : AsyncOperationBase<IResourceLocator>
{
	private AsyncOperationHandle<ResourceManagerRuntimeData> m_rtdOp;

	private AsyncOperationHandle<IResourceLocator> m_loadCatalogOp;

	private string m_ProviderSuffix;

	private AddressablesImpl m_Addressables;

	private InitalizationObjectsOperation m_InitGroupOps;

	protected override float Progress
	{
		get
		{
			if (m_rtdOp.IsValid())
			{
				return m_rtdOp.PercentComplete;
			}
			return 0f;
		}
	}

	protected override string DebugName => "InitializationOperation";

	public InitializationOperation(AddressablesImpl aa)
	{
		m_Addressables = aa;
	}

	internal static AsyncOperationHandle<IResourceLocator> CreateInitializationOperation(AddressablesImpl aa, string playerSettingsLocation, string providerSuffix)
	{
		JsonAssetProvider item = new JsonAssetProvider();
		aa.ResourceManager.ResourceProviders.Add(item);
		TextDataProvider item2 = new TextDataProvider();
		aa.ResourceManager.ResourceProviders.Add(item2);
		aa.ResourceManager.ResourceProviders.Add(new ContentCatalogProvider(aa.ResourceManager));
		ResourceLocationBase location = new ResourceLocationBase("RuntimeData", playerSettingsLocation, typeof(JsonAssetProvider).FullName, typeof(ResourceManagerRuntimeData));
		InitializationOperation initializationOperation = new InitializationOperation(aa)
		{
			m_rtdOp = aa.ResourceManager.ProvideResource<ResourceManagerRuntimeData>(location),
			m_ProviderSuffix = providerSuffix,
			m_InitGroupOps = new InitalizationObjectsOperation()
		};
		initializationOperation.m_InitGroupOps.Init(initializationOperation.m_rtdOp, aa);
		AsyncOperationHandle<bool> asyncOperationHandle = aa.ResourceManager.StartOperation(initializationOperation.m_InitGroupOps, initializationOperation.m_rtdOp);
		return aa.ResourceManager.StartOperation(initializationOperation, asyncOperationHandle);
	}

	protected override bool InvokeWaitForCompletion()
	{
		if (base.IsDone)
		{
			return true;
		}
		if (m_rtdOp.IsValid() && !m_rtdOp.IsDone)
		{
			m_rtdOp.WaitForCompletion();
		}
		m_RM?.Update(Time.unscaledDeltaTime);
		if (!HasExecuted)
		{
			InvokeExecute();
		}
		if (m_loadCatalogOp.IsValid() && !m_loadCatalogOp.IsDone)
		{
			m_loadCatalogOp.WaitForCompletion();
			m_RM?.Update(Time.unscaledDeltaTime);
		}
		if (m_rtdOp.IsDone)
		{
			return m_loadCatalogOp.IsDone;
		}
		return false;
	}

	protected override void Execute()
	{
		if (m_rtdOp.Result == null)
		{
			Addressables.LogWarningFormat("Addressables - Unable to load runtime data at location {0}.", m_rtdOp);
			Complete(base.Result, success: false, $"Addressables - Unable to load runtime data at location {m_rtdOp}.");
			return;
		}
		ResourceManagerRuntimeData result = m_rtdOp.Result;
		WebRequestQueue.SetMaxConcurrentRequests(result.MaxConcurrentWebRequests);
		m_Addressables.CatalogRequestsTimeout = result.CatalogRequestsTimeout;
		foreach (ResourceLocationData catalogLocation in result.CatalogLocations)
		{
			if (catalogLocation.Data != null && catalogLocation.Data is ProviderLoadRequestOptions providerLoadRequestOptions)
			{
				providerLoadRequestOptions.WebRequestTimeout = result.CatalogRequestsTimeout;
			}
		}
		m_rtdOp.Release();
		if (result.CertificateHandlerType != null)
		{
			m_Addressables.ResourceManager.CertificateHandlerInstance = Activator.CreateInstance(result.CertificateHandlerType) as CertificateHandler;
		}
		if (!result.LogResourceManagerExceptions)
		{
			ResourceManager.ExceptionHandler = null;
		}
		if (m_Addressables.ResourceManager.ResourceProviders.FirstOrDefault((IResourceProvider rp) => rp.GetType() == typeof(ContentCatalogProvider)) is ContentCatalogProvider contentCatalogProvider)
		{
			contentCatalogProvider.DisableCatalogUpdateOnStart = result.DisableCatalogUpdateOnStartup;
			contentCatalogProvider.IsLocalCatalogInBundle = result.IsLocalCatalogInBundle;
		}
		ResourceLocationMap resourceLocationMap = new ResourceLocationMap("CatalogLocator", result.CatalogLocations);
		m_Addressables.AddResourceLocator(resourceLocationMap);
		if (!resourceLocationMap.Locate("AddressablesMainContentCatalog", typeof(ContentCatalogData), out var locations))
		{
			Addressables.LogWarningFormat("Addressables - Unable to find any catalog locations in the runtime data.");
			m_Addressables.RemoveResourceLocator(resourceLocationMap);
			Complete(base.Result, success: false, "Addressables - Unable to find any catalog locations in the runtime data.");
			return;
		}
		IResourceLocation remoteHashLocation = null;
		if (locations[0].Dependencies.Count == 3 && result.DisableCatalogUpdateOnStartup)
		{
			remoteHashLocation = locations[0].Dependencies[0];
			locations[0].Dependencies[0] = locations[0].Dependencies[1];
		}
		m_loadCatalogOp = LoadContentCatalogInternal(locations, 0, resourceLocationMap, remoteHashLocation);
	}

	private static void LoadProvider(AddressablesImpl addressables, ObjectInitializationData providerData, string providerSuffix)
	{
		int num = -1;
		string text = (string.IsNullOrEmpty(providerSuffix) ? providerData.Id : (providerData.Id + providerSuffix));
		for (int i = 0; i < addressables.ResourceManager.ResourceProviders.Count; i++)
		{
			if (addressables.ResourceManager.ResourceProviders[i].ProviderId == text)
			{
				num = i;
				break;
			}
		}
		if (num >= 0 && string.IsNullOrEmpty(providerSuffix))
		{
			return;
		}
		IResourceProvider resourceProvider = providerData.CreateInstance<IResourceProvider>(text);
		if (resourceProvider != null)
		{
			if (num < 0 || !string.IsNullOrEmpty(providerSuffix))
			{
				addressables.ResourceManager.ResourceProviders.Add(resourceProvider);
			}
			else
			{
				addressables.ResourceManager.ResourceProviders[num] = resourceProvider;
			}
		}
		else
		{
			Addressables.LogWarningFormat("Addressables - Unable to load resource provider from {0}.", providerData);
		}
	}

	private static AsyncOperationHandle<IResourceLocator> OnCatalogDataLoaded(AddressablesImpl addressables, AsyncOperationHandle<ContentCatalogData> op, string providerSuffix, IResourceLocation remoteHashLocation)
	{
		ContentCatalogData result = op.Result;
		if (result == null)
		{
			Exception exception = ((op.OperationException != null) ? new Exception("Failed to load content catalog.", op.OperationException) : new Exception("Failed to load content catalog."));
			op.Release();
			return addressables.ResourceManager.CreateCompletedOperationWithException<IResourceLocator>(null, exception);
		}
		op.Release();
		if (result.ResourceProviderData != null)
		{
			foreach (ObjectInitializationData resourceProviderDatum in result.ResourceProviderData)
			{
				LoadProvider(addressables, resourceProviderDatum, providerSuffix);
			}
		}
		if (addressables.InstanceProvider == null)
		{
			IInstanceProvider instanceProvider = result.InstanceProviderData.CreateInstance<IInstanceProvider>();
			if (instanceProvider != null)
			{
				addressables.InstanceProvider = instanceProvider;
			}
		}
		if (addressables.SceneProvider == null)
		{
			ISceneProvider sceneProvider = result.SceneProviderData.CreateInstance<ISceneProvider>();
			if (sceneProvider != null)
			{
				addressables.SceneProvider = sceneProvider;
			}
		}
		if (remoteHashLocation != null)
		{
			result.location.Dependencies[0] = remoteHashLocation;
		}
		IResourceLocator resourceLocator = result.CreateCustomLocator(result.location.PrimaryKey, providerSuffix);
		addressables.AddResourceLocator(resourceLocator, result.LocalHash, result.location);
		addressables.AddResourceLocator(new DynamicResourceLocator(addressables));
		return addressables.ResourceManager.CreateCompletedOperation(resourceLocator, string.Empty);
	}

	public static AsyncOperationHandle<IResourceLocator> LoadContentCatalog(AddressablesImpl addressables, IResourceLocation loc, string providerSuffix, IResourceLocation remoteHashLocation = null)
	{
		Type typeFromHandle = typeof(ProviderOperation<ContentCatalogData>);
		ProviderOperation<ContentCatalogData> providerOperation = addressables.ResourceManager.CreateOperation<ProviderOperation<ContentCatalogData>>(typeFromHandle, typeFromHandle.GetHashCode(), null, null);
		IResourceProvider provider = null;
		foreach (IResourceProvider resourceProvider in addressables.ResourceManager.ResourceProviders)
		{
			if (resourceProvider is ContentCatalogProvider)
			{
				provider = resourceProvider;
				break;
			}
		}
		AsyncOperationHandle<IList<AsyncOperationHandle>> asyncOperationHandle = addressables.ResourceManager.CreateGroupOperation<string>(loc.Dependencies, allowFailedDependencies: true);
		providerOperation.Init(addressables.ResourceManager, provider, loc, asyncOperationHandle, releaseDependenciesOnFailure: true);
		AsyncOperationHandle<ContentCatalogData> dependentOp = addressables.ResourceManager.StartOperation(providerOperation, asyncOperationHandle);
		asyncOperationHandle.Release();
		return addressables.ResourceManager.CreateChainOperation(dependentOp, (AsyncOperationHandle<ContentCatalogData> res) => OnCatalogDataLoaded(addressables, res, providerSuffix, remoteHashLocation));
	}

	public AsyncOperationHandle<IResourceLocator> LoadContentCatalog(IResourceLocation loc, string providerSuffix, IResourceLocation remoteHashLocation)
	{
		return LoadContentCatalog(m_Addressables, loc, providerSuffix, remoteHashLocation);
	}

	internal AsyncOperationHandle<IResourceLocator> LoadContentCatalogInternal(IList<IResourceLocation> catalogs, int index, ResourceLocationMap locMap, IResourceLocation remoteHashLocation)
	{
		AsyncOperationHandle<IResourceLocator> asyncOperationHandle = LoadContentCatalog(catalogs[index], m_ProviderSuffix, remoteHashLocation);
		if (asyncOperationHandle.IsDone)
		{
			LoadOpComplete(asyncOperationHandle, catalogs, locMap, index, remoteHashLocation);
		}
		else
		{
			asyncOperationHandle.Completed += delegate(AsyncOperationHandle<IResourceLocator> op)
			{
				LoadOpComplete(op, catalogs, locMap, index, remoteHashLocation);
			};
		}
		return asyncOperationHandle;
	}

	private void LoadOpComplete(AsyncOperationHandle<IResourceLocator> op, IList<IResourceLocation> catalogs, ResourceLocationMap locMap, int index, IResourceLocation remoteHashLocation)
	{
		if (op.Result != null)
		{
			m_Addressables.RemoveResourceLocator(locMap);
			base.Result = op.Result;
			Complete(base.Result, success: true, string.Empty);
			op.Release();
		}
		else if (index + 1 >= catalogs.Count)
		{
			Addressables.LogWarningFormat("Addressables - initialization failed.", op);
			m_Addressables.RemoveResourceLocator(locMap);
			if (op.OperationException != null)
			{
				Complete(base.Result, success: false, op.OperationException);
			}
			else
			{
				Complete(base.Result, success: false, "LoadContentCatalogInternal");
			}
			op.Release();
		}
		else
		{
			m_loadCatalogOp = LoadContentCatalogInternal(catalogs, index + 1, locMap, remoteHashLocation);
			op.Release();
		}
	}
}
