namespace Fusion;

public interface ISceneLoadDone : IPublicFacingInterface
{
	void SceneLoadDone(in SceneLoadDoneArgs sceneInfo);
}
