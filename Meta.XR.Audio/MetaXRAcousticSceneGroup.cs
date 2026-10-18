using UnityEngine;

[CreateAssetMenu(menuName = "MetaXRAudio/Acoustic Scene Group")]
internal class MetaXRAcousticSceneGroup : ScriptableObject
{
	[SerializeField]
	internal string[] sceneGuids;
}
