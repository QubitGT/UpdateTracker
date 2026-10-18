using System;
using System.Collections.Generic;
using Unity.XR.CoreUtils.Collections;

namespace UnityEngine.XR.Interaction.Toolkit.Utilities;

[AddComponentMenu("")]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.Utilities.DisposableManagerSingleton.html")]
internal sealed class DisposableManagerSingleton : MonoBehaviour
{
	private static DisposableManagerSingleton s_DisposableManagerSingleton;

	private readonly HashSetList<IDisposable> m_Disposables = new HashSetList<IDisposable>();

	private static DisposableManagerSingleton instance => Initialize();

	private static DisposableManagerSingleton Initialize()
	{
		if (s_DisposableManagerSingleton == null)
		{
			GameObject obj = new GameObject("[DisposableManagerSingleton]");
			Object.DontDestroyOnLoad(obj);
			s_DisposableManagerSingleton = obj.AddComponent<DisposableManagerSingleton>();
		}
		return s_DisposableManagerSingleton;
	}

	private void Awake()
	{
		if (s_DisposableManagerSingleton != null && s_DisposableManagerSingleton != this)
		{
			Object.Destroy(this);
		}
		else if (s_DisposableManagerSingleton == null)
		{
			s_DisposableManagerSingleton = this;
		}
	}

	private void OnDestroy()
	{
		DisposeAll();
	}

	private void OnApplicationQuit()
	{
		DisposeAll();
	}

	private void DisposeAll()
	{
		IReadOnlyList<IDisposable> readOnlyList = m_Disposables.AsList();
		for (int i = 0; i < readOnlyList.Count; i++)
		{
			readOnlyList[i].Dispose();
		}
		m_Disposables.Clear();
	}

	public static void RegisterDisposable(IDisposable disposableToRegister)
	{
		instance.m_Disposables.Add(disposableToRegister);
	}
}
