using System;
using UnityEngine.Localization.PropertyVariants.TrackedProperties;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace UnityEngine.Localization.PropertyVariants.TrackedObjects;

[Serializable]
[DisplayName("Mesh Filter", null)]
[CustomTrackedObject(typeof(MeshFilter), false)]
public class TrackedMeshFilter : TrackedObject
{
	private const string k_MeshProperty = "m_Mesh";

	private AsyncOperationHandle<Mesh> m_CurrentOperation;

	public override bool CanTrackProperty(string propertyPath)
	{
		return propertyPath == "m_Mesh";
	}

	public override AsyncOperationHandle ApplyLocale(Locale variantLocale, Locale defaultLocale)
	{
		if (base.TrackedProperties.Count == 0)
		{
			return default(AsyncOperationHandle);
		}
		if (m_CurrentOperation.IsValid())
		{
			if (!m_CurrentOperation.IsDone)
			{
				m_CurrentOperation.Completed -= MeshOperationCompleted;
			}
			AddressablesInterface.SafeRelease(m_CurrentOperation);
			m_CurrentOperation = default(AsyncOperationHandle<Mesh>);
		}
		ITrackedProperty trackedProperty = base.TrackedProperties[0];
		if (trackedProperty is UnityObjectProperty unityObjectProperty)
		{
			LocaleIdentifier fallback = ((defaultLocale != null) ? defaultLocale.Identifier : default(LocaleIdentifier));
			if (unityObjectProperty.GetValue(variantLocale.Identifier, fallback, out var foundValue))
			{
				SetMesh(foundValue as Mesh);
			}
		}
		else if (trackedProperty is LocalizedAssetProperty localizedAssetProperty && !localizedAssetProperty.LocalizedObject.IsEmpty)
		{
			m_CurrentOperation = localizedAssetProperty.LocalizedObject.LoadAssetAsync<Mesh>();
			if (m_CurrentOperation.IsDone)
			{
				MeshOperationCompleted(m_CurrentOperation);
			}
			else
			{
				if (!localizedAssetProperty.LocalizedObject.ForceSynchronous)
				{
					m_CurrentOperation.Completed += MeshOperationCompleted;
					return m_CurrentOperation;
				}
				m_CurrentOperation.WaitForCompletion();
				MeshOperationCompleted(m_CurrentOperation);
			}
		}
		return default(AsyncOperationHandle);
	}

	private void MeshOperationCompleted(AsyncOperationHandle<Mesh> assetOp)
	{
		SetMesh(assetOp.Result);
	}

	private void SetMesh(Mesh mesh)
	{
		((MeshFilter)base.Target).sharedMesh = mesh;
	}
}
