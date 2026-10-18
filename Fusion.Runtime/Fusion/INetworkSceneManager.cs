using UnityEngine;
using UnityEngine.SceneManagement;

namespace Fusion;

public interface INetworkSceneManager
{
	bool IsBusy { get; }

	Scene MainRunnerScene { get; }

	void Initialize(NetworkRunner runner);

	void Shutdown();

	bool IsRunnerScene(Scene scene);

	bool TryGetPhysicsScene2D(out PhysicsScene2D scene2D);

	bool TryGetPhysicsScene3D(out PhysicsScene scene3D);

	void MakeDontDestroyOnLoad(GameObject obj);

	bool MoveGameObjectToScene(GameObject gameObject, SceneRef sceneRef);

	NetworkSceneAsyncOp LoadScene(SceneRef sceneRef, NetworkLoadSceneParameters parameters);

	NetworkSceneAsyncOp UnloadScene(SceneRef sceneRef);

	SceneRef GetSceneRef(GameObject gameObject);

	SceneRef GetSceneRef(string sceneNameOrPath);

	bool OnSceneInfoChanged(NetworkSceneInfo sceneInfo, NetworkSceneInfoChangeSource changeSource);
}
