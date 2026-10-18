using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Fusion;

internal sealed class NetworkSceneManagerDummy : INetworkSceneManager
{
	public bool IsBusy => false;

	public Scene MainRunnerScene => SceneManager.GetActiveScene();

	public void Initialize(NetworkRunner runner)
	{
	}

	public void Shutdown()
	{
	}

	public bool IsRunnerScene(Scene scene)
	{
		return true;
	}

	public bool MoveGameObjectToScene(GameObject gameObject, SceneRef sceneRef)
	{
		throw new NotImplementedException();
	}

	public SceneRef GetSceneRef(GameObject gameObject)
	{
		throw new NotImplementedException();
	}

	public bool TryGetPhysicsScene2D(out PhysicsScene2D scene2D)
	{
		scene2D = Physics2D.defaultPhysicsScene;
		return true;
	}

	public bool TryGetPhysicsScene3D(out PhysicsScene scene3D)
	{
		scene3D = Physics.defaultPhysicsScene;
		return true;
	}

	public void MakeDontDestroyOnLoad(GameObject obj)
	{
		UnityEngine.Object.DontDestroyOnLoad(obj);
	}

	public void MoveToRunnerScene(GameObject obj)
	{
		SceneManager.MoveGameObjectToScene(obj, SceneManager.GetActiveScene());
	}

	public NetworkSceneAsyncOp LoadScene(SceneRef sceneRef, NetworkLoadSceneParameters parameters)
	{
		throw new NotImplementedException();
	}

	public NetworkSceneAsyncOp UnloadScene(SceneRef sceneRef)
	{
		throw new NotImplementedException();
	}

	public void OnSceneInfoChanged()
	{
	}

	public SceneRef GetSceneRef(string sceneNameOrPath)
	{
		throw new NotImplementedException();
	}

	public bool OnSceneInfoChanged(NetworkSceneInfo sceneInfo, NetworkSceneInfoChangeSource changeSource)
	{
		return false;
	}
}
