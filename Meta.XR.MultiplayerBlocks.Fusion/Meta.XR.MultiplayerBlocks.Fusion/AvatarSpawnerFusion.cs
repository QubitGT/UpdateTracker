using Meta.XR.MultiplayerBlocks.Shared;
using UnityEngine;

namespace Meta.XR.MultiplayerBlocks.Fusion;

public class AvatarSpawnerFusion : MonoBehaviour
{
	[Tooltip("Control when you want to load the avatar.")]
	[SerializeField]
	private bool loadAvatarWhenConnected = true;

	[SerializeField]
	private GameObject avatarBehavior;

	[SerializeField]
	private GameObject avatarBehaviorSdk28Plus;

	[Tooltip("Specify the number of preset avatars available in the project. The maximum size depends on the SDK version.")]
	[SerializeField]
	private int preloadedSampleAvatarSize = 6;

	[Tooltip("Adjust the level of detail used when streaming the avatars.")]
	[SerializeField]
	private AvatarStreamLOD avatarStreamLOD = AvatarStreamLOD.Medium;

	[Tooltip("Adjust the update interval used when streaming the avatars.")]
	[SerializeField]
	private float avatarUpdateIntervalInSec = 0.08f;
}
