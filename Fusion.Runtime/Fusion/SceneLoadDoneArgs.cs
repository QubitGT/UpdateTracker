using UnityEngine;
using UnityEngine.SceneManagement;

namespace Fusion;

public readonly struct SceneLoadDoneArgs
{
	public readonly SceneRef SceneRef;

	public readonly NetworkObject[] SceneObjects;

	public readonly Scene Scene;

	public readonly GameObject[] RootGameObjects;

	public SceneLoadDoneArgs(SceneRef sceneRef, NetworkObject[] sceneObjects, Scene scene = default(Scene), GameObject[] rootGameObjects = null)
	{
		SceneRef = sceneRef;
		SceneObjects = sceneObjects;
		Scene = scene;
		RootGameObjects = rootGameObjects;
	}
}
