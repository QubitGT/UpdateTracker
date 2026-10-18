using UnityEngine.SceneManagement;

namespace UnityEngine.ProBuilder;

internal abstract class EntityBehaviour : MonoBehaviour
{
	[Tooltip("Allow ProBuilder to automatically hide and show this object when entering or exiting play mode.")]
	public bool manageVisibility = true;

	public abstract void Initialize();

	public abstract void OnEnterPlayMode();

	public abstract void OnSceneLoaded(Scene scene, LoadSceneMode mode);

	protected void SetMaterial(Material material)
	{
		if ((bool)GetComponent<Renderer>())
		{
			GetComponent<Renderer>().sharedMaterial = material;
		}
		else
		{
			base.gameObject.AddComponent<MeshRenderer>().sharedMaterial = material;
		}
	}
}
