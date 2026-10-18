using Meta.XR.ImmersiveDebugger.Utils;
using UnityEngine.SceneManagement;

namespace Meta.XR.ImmersiveDebugger.Hierarchy;

internal class SceneRegistry : ItemWithChildren<object, SceneItem, Scene>
{
	public override string Label => null;

	public override bool Valid => true;

	protected override InstanceHandle BuildHandle()
	{
		return default(InstanceHandle);
	}

	protected override bool CompareChildren(Scene lhs, Scene rhs)
	{
		return lhs == rhs;
	}

	public override void Unregister()
	{
	}

	public override void Register(Item parent)
	{
	}

	protected override Scene[] FetchExpectedChildren()
	{
		Scene[] array = new Scene[SceneManager.sceneCount];
		for (int i = 0; i < SceneManager.sceneCount; i++)
		{
			array[i] = SceneManager.GetSceneAt(i);
		}
		return array;
	}
}
