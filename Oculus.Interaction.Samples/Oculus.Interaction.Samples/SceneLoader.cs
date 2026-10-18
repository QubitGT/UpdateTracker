using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Oculus.Interaction.Samples;

public class SceneLoader : MonoBehaviour
{
	private bool _loading;

	public Action<string> WhenLoadingScene = delegate
	{
	};

	public Action<string> WhenSceneLoaded = delegate
	{
	};

	private int _waitingCount;

	public void Load(string sceneName)
	{
		if (!_loading)
		{
			_loading = true;
			_waitingCount = WhenLoadingScene.GetInvocationList().Length - 1;
			if (_waitingCount == 0)
			{
				HandleReadyToLoad(sceneName);
			}
			else
			{
				WhenLoadingScene(sceneName);
			}
		}
	}

	public void HandleReadyToLoad(string sceneName)
	{
		_waitingCount--;
		if (_waitingCount <= 0)
		{
			StartCoroutine(LoadSceneAsync(sceneName));
		}
	}

	private IEnumerator LoadSceneAsync(string sceneName)
	{
		AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
		while (!asyncLoad.isDone)
		{
			yield return null;
		}
		WhenSceneLoaded(sceneName);
	}
}
