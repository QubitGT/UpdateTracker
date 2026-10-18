using System;
using System.Runtime.CompilerServices;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UIElements;

namespace UnityEngine.Localization;

[UxmlObject]
public abstract class LocalizedAssetBase : LocalizedReference
{
	[Serializable]
	[CompilerGenerated]
	public new abstract class UxmlSerializedData : LocalizedReference.UxmlSerializedData
	{
	}

	public abstract AsyncOperationHandle<Object> LoadAssetAsObjectAsync();

	public abstract AsyncOperationHandle<TObject> LoadAssetAsync<TObject>() where TObject : Object;
}
